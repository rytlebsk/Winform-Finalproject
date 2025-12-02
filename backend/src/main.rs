use axum::extract::ws::{self, Message, WebSocket};
use axum::{
    Json, Router,
    extract::{self, State, WebSocketUpgrade},
    http::StatusCode,
    response::IntoResponse,
    routing::{get, post},
};
use bb8::Pool;
use bb8_postgres::PostgresConnectionManager;
use chrono::{DateTime, Utc};
use futures_util::{SinkExt, StreamExt};
use once_cell::sync::Lazy;
use postgres::Socket;
use serde::{Deserialize, Serialize};
use serde_json::json;
use std::collections::HashMap;
use std::future::Future;
use std::pin::Pin;
use std::sync::Arc;
use tokio::net::TcpListener;
use tokio::sync::Mutex;
use tokio_postgres::NoTls;
use uuid::Uuid;

type ConnectionPool = Pool<PostgresConnectionManager<tokio_postgres::NoTls>>;
type WsSender = Arc<Mutex<futures_util::stream::SplitSink<WebSocket, Message>>>;
type EventFuture = Pin<Box<dyn Future<Output = ()> + Send + 'static>>;
type EventHandler = Box<dyn Fn(EventParms) -> EventFuture + Send + Sync + 'static>;
#[derive(Clone)]
struct DatabaseConnectionState {
    pool: Arc<ConnectionPool>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
struct User {
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
struct Room {
    id: Uuid,
    members: Vec<Uuid>,
    video_queue: Vec<String>,
}

struct Controller {
    users: HashMap<Uuid, User>,
    rooms: HashMap<Uuid, Room>,
    ws_senders: HashMap<Uuid, WsSender>,
}

static CONTROLLER: Lazy<Mutex<Controller>> = Lazy::new(|| {
    Mutex::new(Controller {
        users: HashMap::new(),
        rooms: HashMap::new(),
        ws_senders: HashMap::new(),
    })
});

struct EventParms {
    ws: WsSender,
    v: serde_json::Value,
    pool: Arc<ConnectionPool>,
}

enum EventHandlers {
    login(EventParms),
    // Add more event types here
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("System online at {:?}", Utc::now());

    let manager = PostgresConnectionManager::new_from_stringlike(
        "host=localhost user=postgres port=5433 dbname=postgres",
        NoTls,
    )?;
    let pool = Pool::builder().build(manager).await?;

    run_init(pool.clone()).await;

    let app = Router::new()
        .route("/health", get(|| async { "OK" })) //render health check endpoint
        .route("/", get(ws_handler))
        .with_state(DatabaseConnectionState {
            pool: Arc::new(pool.clone()),
        });

    let listener = TcpListener::bind("0.0.0.0:3000").await.unwrap();
    axum::serve(listener, app).await.unwrap();
    Ok(())
}

async fn ws_handler(
    ws: WebSocketUpgrade,
    State(state): State<DatabaseConnectionState>,
) -> impl IntoResponse {
    ws.on_upgrade(|socket| handle_socket(socket, state))
}

async fn handle_socket(socket: WebSocket, _state: DatabaseConnectionState) {
    let (sink, mut stream) = socket.split();
    let client_id = Uuid::new_v4();
    let wrapped_sender: WsSender = Arc::new(Mutex::new(sink));

    // 把 sender 放進全域 controller
    {
        let mut ctrl = CONTROLLER.lock().await;
        ctrl.ws_senders.insert(client_id, wrapped_sender.clone());
    }

    //message handling loop
    while let Some(msg_res) = stream.next().await {
        match msg_res {
            Ok(Message::Text(txt)) => {
                match serde_json::from_str::<serde_json::Value>(&txt) {
                    Ok(v) => {
                        if let Some(event_type) = v.get("event").and_then(|e| e.as_str()) {
                            let params = EventParms {
                                ws: wrapped_sender.clone(),
                                v: v.clone(),
                                pool: _state.pool.clone(),
                            };
                            match event_type {
                                "login" => {
                                    login(params).await;
                                }
                                // Add more event types here
                                _ => {
                                    // Unknown event type
                                }
                            }
                        }
                    }
                    Err(e) => {
                        wrapped_sender
                            .lock()
                            .await
                            .send(Message::Text(ws::Utf8Bytes::from(json!({
                                "status_code":4001,
                                "msg":format!("Invalid JSON format"),
                            }).to_string())))
                            .await
                            .unwrap();
                    }
                }
            }
            Ok(Message::Binary(_)) => {
                // 若需處理 binary，可在此加入
            }
            Ok(Message::Close(_)) => break,
            Ok(_) => {}
            Err(_) => break,
        }
    }

    // 連線結束，從 controller 移除
    {
        let mut ctrl = CONTROLLER.lock().await;
        ctrl.ws_senders.remove(&client_id);
    }
}

async fn login(params: EventParms) {
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
            controller.rooms.get_mut(&room_uuid);
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

// //login when open
// async fn login(
//     State(state): State<DatabaseConnectionState>,
//     extract::Json(payload): extract::Json<LoginRequestPayload>,
// ) -> impl IntoResponse {
//     //search database by id
//     let mut id: String = payload.id;
//     let uuid_id = Uuid::parse_str(&id).unwrap_or(Uuid::nil());

//     match state.pool.get().await {
//         Ok(client) => {
//             let user = User {
//                 id: Uuid::new_v4(),
//                 username: "new_user".to_string(),
//                 avatar_url: "http://example.com/avatar.png".to_string(),
//                 last_login: Utc::now(),
//                 created_at: Utc::now(),
//                 friends: vec![],
//                 status: Uuid::new_v4(),
//                 online: false,
//             };

//             let user_exists = match client
//                 .query_one(
//                     "SELECT EXISTS(SELECT 1 FROM users WHERE id = $1)",
//                     &[&uuid_id],
//                 )
//                 .await
//             {
//                 Err(_) => false,
//                 Ok(_) => {
//                     let user_data = match client
//                         .query_one(
//                             "SELECT id, name, avatar_url, last_login, created_at, friends, status, online FROM users WHERE id = $1",
//                             &[&uuid_id],
//                         )
//                         .await
//                     {
//                         Err(_) => None,
//                         Ok(row) => Some(User {
//                             id: row.get(0),
//                             username: row.get(1),
//                             avatar_url: row.get(2),
//                             last_login: row.get(3),
//                             created_at: row.get(4),
//                             friends: row.get(5),
//                             status: row.get(6),
//                             online: row.get(7),
//                         }),
//                     };
//                     if let Some(existing_user) = user_data {
//                         CONTROLLER.lock().await.users.insert(uuid_id, existing_user);
//                         true
//                     } else {
//                         false
//                     }
//                 }
//             };

//             /* register */
//             if id.is_empty() {
//                 //insert new user into database
//                 match client
//                     .execute(
//                         "INSERT INTO users (id, name, avatar_url, last_login, created_at, friends, status, online) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)",
//                         &[
//                             &user.id,
//                             &user.username,
//                             &user.avatar_url,
//                             &user.last_login,
//                             &user.created_at,
//                             &user.friends,
//                             &user.status,
//                             &user.online,
//                         ],
//                     )
//                     .await
//                 {
//                     Err(e) => return (
//                         StatusCode::INTERNAL_SERVER_ERROR,
//                         Json(LoginResponsePayload {
//                             id: id.clone(),
//                             room_id: String::new(),
//                             msg: format!("Database insertion failed: {}", e),
//                         }),
//                     ),
//                     Ok(_) => (
//                         id = user.id.to_string(),
//                     ),
//                 };
//             } else if !user_exists {
//                 return (
//                     StatusCode::BAD_REQUEST,
//                     Json(LoginResponsePayload {
//                         id: id.clone(),
//                         room_id: String::new(),
//                         msg: "User ID does not exist".to_string(),
//                     }),
//                 );
//             }

//             /* login */
//             //generate room id (for simplicity, use a fixed room id here)
//             let room_exist = !payload.room_id.is_empty()
//                 && CONTROLLER
//                     .lock()
//                     .await
//                     .rooms
//                     .contains_key(&Uuid::parse_str(&payload.room_id).unwrap_or(Uuid::nil()));

//             //validate room id
//             let room_id: Uuid;
//             if (!room_exist && !payload.room_id.is_empty()) {
//                 return (
//                     StatusCode::BAD_REQUEST,
//                     Json(LoginResponsePayload {
//                         id: id.clone(),
//                         room_id: String::new(),
//                         msg: "Room ID does not exist".to_string(),
//                     }),
//                 );
//             } else if room_exist {
//                 room_id = Uuid::parse_str(&payload.room_id).unwrap_or(Uuid::nil());
//             } else {
//                 room_id = Uuid::new_v4();
//             }

//             //update last login time & tag as online
//             match client
//                 .execute(
//                     "UPDATE users SET last_login = $1, online = TRUE, status = $2 WHERE id = $3",
//                     &[&Utc::now(), &room_id, &uuid_id],
//                 )
//                 .await
//             {
//                 Err(e) => {
//                     return (
//                         StatusCode::INTERNAL_SERVER_ERROR,
//                         Json(LoginResponsePayload {
//                             id: id.clone(),
//                             room_id: String::new(),
//                             msg: format!("Database update failed: {}", e),
//                         }),
//                     );
//                 }
//                 Ok(_) => (),
//             };

//             let mut controller = CONTROLLER.lock().await;

//             //update in-memory data
//             controller.users.insert(
//                 uuid_id,
//                 User {
//                     id: uuid_id,
//                     username: "existing_user".to_string(),
//                     avatar_url: "http://example.com/avatar.png".to_string(),
//                     last_login: Utc::now(),
//                     created_at: Utc::now(),
//                     friends: vec![],
//                     status: room_id,
//                     online: true,
//                 },
//             );

//             //add user to room
//             controller.rooms.get_mut(&room_id);
//             match controller.rooms.get_mut(&room_id) {
//                 Some(room) => {
//                     if !room.members.contains(&uuid_id) {
//                         room.members.push(uuid_id);
//                     }
//                 }
//                 None => {
//                     controller.rooms.insert(
//                         room_id,
//                         Room {
//                             id: room_id,
//                             members: vec![uuid_id],
//                             video_queue: vec![],
//                         },
//                     );
//                 }
//             }
//             return (
//                 StatusCode::OK,
//                 Json(LoginResponsePayload {
//                     id: id.clone(),
//                     room_id: room_id.to_string(),
//                     msg: "User logged in successfully".to_string(),
//                 }),
//             );
//         }
//         Err(_) => {
//             return (
//                 StatusCode::INTERNAL_SERVER_ERROR,
//                 Json(LoginResponsePayload {
//                     id: id.clone(),
//                     room_id: String::new(),
//                     msg: "Failed to get database connection".to_string(),
//                 }),
//             );
//         }
//     };
// }

// async fn logout(
//     State(state): State<DatabaseConnectionState>,
//     extract::Json(payload): extract::Json<LogoutRequestPayload>,
// ) -> impl IntoResponse {
//     let id: String = payload.id;
//     let uuid_id = Uuid::parse_str(&id).unwrap_or(Uuid::nil());
//     let client = match state.pool.get().await {
//         Ok(client) => client,
//         Err(_) => {
//             return (
//                 StatusCode::INTERNAL_SERVER_ERROR,
//                 Json(json!({"id": id, "msg": "Failed to get database connection"})),
//             );
//         }
//     };

//     match client
//         .execute("UPDATE users SET online = FALSE WHERE id = $1", &[&uuid_id])
//         .await
//     {
//         Err(e) => {
//             return (
//                 StatusCode::INTERNAL_SERVER_ERROR,
//                 Json(json!({"id": id, "msg": format!("Database update failed: {}", e)})),
//             );
//         }
//         Ok(_) => (
//             StatusCode::OK,
//             Json(json!({"id": id, "msg": "User tagged as offline successfully"})),
//         ),
//     }
// }

async fn run_init(pool: ConnectionPool) {
    println!(
        "Running database initialization...(Connection will unavailable warning is normal if database is not set up yet)"
    );
    let client = match pool.get().await {
        Ok(client) => client,
        Err(e) => {
            println!("Failed to get database connection: {}", e);
            println!("\x1b[11mWarning! No database connection available!\x1b[0m");
            return;
        }
    };
    println!("Database connection acquired successfully. You can connect to the socket now.");

    let clear = std::env::args().any(|arg| arg == "clear");

    if clear {
        client
            .execute("DROP TABLE IF EXISTS users", &[])
            .await
            .unwrap();
    }

    client
        .execute(
            "CREATE TABLE IF NOT EXISTS users (
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                avatar_url TEXT,
                last_login TIMESTAMPTZ,
                created_at TIMESTAMPTZ DEFAULT NOW(),
                friends UUID[],
                status UUID,
                online BOOLEAN DEFAULT FALSE
            )",
            &[],
        )
        .await
        .unwrap();
}
