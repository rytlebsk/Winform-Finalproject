use axum::extract::ws::{self, Message, WebSocket};
use bb8::Pool;
use bb8_postgres::PostgresConnectionManager;
use chrono::{DateTime, Utc};
use futures_util::{SinkExt, StreamExt};
use serde::{Deserialize, Serialize};
use serde_json::json;
use tokio::sync::Mutex;
use std::sync::Arc;
use uuid::Uuid;
use crate::controller::controller::CONTROLLER;

pub type ConnectionPool = Pool<PostgresConnectionManager<tokio_postgres::NoTls>>;
pub type WsSender = Arc<Mutex<futures_util::stream::SplitSink<WebSocket, Message>>>;

pub struct EventParms {
    pub ws: WsSender,
    pub v: serde_json::Value,
    pub pool: Arc<ConnectionPool>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct User {
    id: Uuid,
    username: String,
    avatar_url: String,
    last_login: DateTime<Utc>,
    created_at: DateTime<Utc>,
    friends: Vec<Uuid>,
    status: Uuid,
    online: bool,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Room {
    id: Uuid,
    members: Vec<Uuid>,
    video_queue: Vec<String>,
}

pub async fn login(params: EventParms) {
    //check payload validity
    let mut id: Option<String> = params
        .v
        .get("id")
        .and_then(|v| v.as_str().map(|s| s.to_string()));
    let room_id: Option<String> = params
        .v
        .get("room_id")
        .and_then(|v| v.as_str().map(|s| s.to_string()));

    if (id.is_none() || room_id.is_none()) {
        let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
            "status_code":4000,
            "id": id.clone().unwrap_or_default(),
            "room_id": room_id.clone().unwrap_or_default(),
            "msg":"Invalid payload",
        }).to_string()))).await;
        return;
    }

    println!("Login attempt with id: {:?}, room_id: {:?}", id, room_id);

    match params.pool.get().await {
        Ok(client) => {
            let mut user = User {
                id: Uuid::new_v4(),
                username: "new_user".to_string(),
                avatar_url: "http://example.com/avatar.png".to_string(),
                last_login: Utc::now(),
                created_at: Utc::now(),
                friends: vec![],
                status: Uuid::new_v4(),
                online: false,
            };
            let id_uuid = Uuid::parse_str(&id.clone().unwrap()).unwrap_or(Uuid::nil());

            //check if user exists at database
            let user_exists = match client
                .query_one(
                    "SELECT EXISTS(SELECT 1 FROM users WHERE id = $1)",
                    &[&id_uuid],
                )
                .await
            {
                Err(_) => false,
                Ok(_) => {
                    let user_data = match client
                        .query_one(
                            "SELECT id, name, avatar_url, last_login, created_at, friends, status, online FROM users WHERE id = $1",
                            &[&id_uuid],
                        )
                        .await
                    {
                        Err(_) => None,
                        Ok(row) => Some(User {
                            id: row.get(0),
                            username: row.get(1),
                            avatar_url: row.get(2),
                            last_login: row.get(3),
                            created_at: row.get(4),
                            friends: row.get(5),
                            status: row.get(6),
                            online: row.get(7),
                        }),
                    };
                    if let Some(existing_user) = user_data {
                        let users = CONTROLLER.lock().await.users.clone();
                        if(!users.contains_key(&id_uuid)){
                            user = existing_user;
                        } else{
                            let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
                                "status_code":4003,
                                "id": id.clone().unwrap_or_default(),
                                "room_id": String::new(),
                                "msg":"User already logged in",
                            }).to_string()))).await;
                        }
                        true
                    } else {
                        false
                    }
                }
            };

            if id.clone().unwrap().is_empty() {
                //create new user in database
                match client
                    .execute(
                        "INSERT INTO users (id, name, avatar_url, last_login, created_at, friends, status, online) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)",
                        &[
                            &user.id,
                            &user.username,
                            &user.avatar_url,
                            &user.last_login,
                            &user.created_at,
                            &user.friends,
                            &user.status,
                            &user.online,
                        ],
                    )
                    .await
                {
                    Err(e) => ({
                        let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
                            "status_code":5000,
                            "id": id.clone().unwrap_or_default(),
                            "room_id": String::new(),
                            "msg":format!("Database insertion failed: {}", e),
                        }).to_string()))).await;
                        return;
                    }),
                    
                    Ok(_) => (
                        id = Some(user.id.to_string()),
                    ),
                };
            } else if !user_exists {
                //user id is provided but does not exist
                let response = json!({
                    "status_code":4004,
                    "id": id.clone().unwrap_or_default(),
                    "room_id": String::new(),
                    "msg":"User ID does not exist",
                });
                let mut sender = params.ws.lock().await;
                let _ = sender
                    .send(Message::Text(ws::Utf8Bytes::from(response.to_string())))
                    .await;
                return;
            }

            /* login */
            //generate room id (for simplicity, use a fixed room id here)
            let room_exist = room_id.clone().unwrap().is_empty()
                && CONTROLLER
                    .lock()
                    .await
                    .rooms
                    .contains_key(&Uuid::parse_str(room_id.clone().unwrap().as_str()).unwrap_or(Uuid::nil()));

            //validate room id
            let mut room_uuid = Uuid::nil();
            if(room_id.clone().unwrap().is_empty()){
                    room_uuid = Uuid::new_v4();
            }
            else{
                if (!room_exist) {
                    let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
                        "status_code":4004,
                        "id": id.clone().unwrap_or_default(),
                        "room_id": String::new(),
                            "msg": "Room ID does not exist".to_string(),
                        }).to_string()))).await;
                    return;
                } else {
                    room_uuid = Uuid::parse_str(room_id.unwrap().as_str()).unwrap_or(Uuid::nil());
                }
            }

            //update last login time & tag as online
            match client
                .execute(
                    "UPDATE users SET last_login = $1, online = TRUE, status = $2 WHERE id = $3",
                    &[&Utc::now(), &room_uuid, &id_uuid],
                )
                .await
            {
                Err(e) => {
                    let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
                        "status_code":5000,
                        "id": id.clone().unwrap_or_default(),
                        "room_id": String::new(),
                        "msg":format!("Database update failed: {}", e),
                    }).to_string()))).await;
                    return;
                }
                Ok(_) => (),
            };

            let mut controller = CONTROLLER.lock().await;

            //update in-memory data
            controller.users.insert(
                id_uuid,
                user.clone(),
            );

            //add user to room
            match controller.rooms.get_mut(&room_uuid) {
                Some(room) => {
                    if !room.members.contains(&id_uuid) {
                        room.members.push(id_uuid);
                    }
                }
                None => {
                    controller.rooms.insert(
                        room_uuid,
                        Room {
                            id: room_uuid,
                            members: vec![id_uuid],
                            video_queue: vec![],
                        },
                    );
                }
            }
            let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
                "status_code":2000,
                "id": id.clone().unwrap_or_default(),
                "room_id": room_uuid.to_string(),
                "msg":"User logged in successfully",
            }).to_string()))).await;
        }
        Err(_) => {
            let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
                "status_code":5000,
                "id": id.clone().unwrap_or_default(),
                "room_id": room_id.clone().unwrap_or_default(),
                "msg":"Failed to get database connection",
            }).to_string()))).await;
            return;
        }
    };
}

// pub async fn logout(params: EventParms) {
//     //check payload validity
//     let id: Option<String> = params
//         .v
//         .get("id")
//         .and_then(|v| v.as_str().map(|s| s.to_string()));
//     if (id.is_none()) {
//         let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
//             "status_code":4000,
//             "id": id.clone().unwrap_or_default(),
//             "msg":"Invalid payload",
//         }).to_string()))).await;
//         return;
//     }

//     if(id.clone().unwrap().is_empty()){
//         let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
//             "status_code":4000,
//             "id": id.clone().unwrap_or_default(),
//             "msg":"Invalid id",
//         }).to_string()))).await;
//         return;
//     }

//     let id_uuid = Uuid::parse_str(&id.clone().unwrap()).unwrap_or(Uuid::nil());
//     match CONTROLLER.lock().await.users.iter().find(|(&uid, _)| uid == id_uuid) {
//         Some(_) => {
            
//         },
//         None => {
//             let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
//                 "status_code":4004,
//                 "id": id.clone().unwrap_or_default(),
//                 "msg":"User ID does not exist",
//             }).to_string()))).await;
//             return;
//         }
//     }


//     // let client = match params.pool.get().await {
//     //     Ok(client) => client,
//     //     Err(_) => {
//     //         let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
//     //             "status_code":5000,
//     //             "id": id.clone().unwrap_or_default(),
//     //             "msg":"Failed to get database connection",
//     //         }).to_string()))).await;
//     //         return;
//     //     }
//     // };

//     // match client
//     //     .execute("UPDATE users SET online = FALSE WHERE id = $1", &[&id_uuid])
//     //     .await
//     // {
//     //     Err(e) => {
//     //         let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
//     //             "status_code":5000,
//     //             "id": id.clone().unwrap_or_default(),
//     //             "msg":format!("Database update failed: {}", e),
//     //         }).to_string()))).await;
//     //         return;
//     //     }
//     //     Ok(_) => {
//     //         let _ = params.ws.lock().await.send(Message::Text(ws::Utf8Bytes::from(json!({
//     //             "status_code":2000,
//     //             "id": id.clone().unwrap_or_default(),
//     //             "msg":"User tagged as offline successfully",
//     //         }).to_string()))).await;
//     //     }
//     // }
// }

