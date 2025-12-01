use axum::extract::ws::{Message, WebSocket};
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
use std::sync::Arc;
use tokio::net::TcpListener;
use tokio::sync::Mutex;
use tokio_postgres::NoTls;
use uuid::Uuid;

type ConnectionPool = Pool<PostgresConnectionManager<tokio_postgres::NoTls>>;
type WsSender = Arc<Mutex<futures_util::stream::SplitSink<WebSocket, Message>>>;
#[derive(Clone)]
struct DatabaseConnectionState {
    pool: Arc<ConnectionPool>,
}

#[derive(Serialize, Deserialize, Debug)]
struct LoginRequestPayload {
    id: String,
    room_id: String,
}

#[derive(Serialize, Deserialize, Debug)]
struct LoginResponsePayload {
    id: String,
    room_id: String,
    msg: String,
}

#[derive(Serialize, Deserialize, Debug)]
struct LogoutRequestPayload {
    id: String,
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

static EVENT_MAPPING: Lazy<HashMap<&'static str, fn(String)>> = Lazy::new(|| {
    let mut m = HashMap::new();
    m.insert("login", login as fn(String));
    m
});

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

    // 讀取來自 client 的訊息並廣播給其他連線 (示範)
    while let Some(msg_res) = stream.next().await {
        match msg_res {
            Ok(Message::Text(txt)) => {
                //deal event by event mapping
                EVENT_MAPPING.get(txt.as_str()).map(|f| f(txt.to_string()));
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

fn login(s: String) {
    println!("Login event received with payload: {}", s);
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
    let client = pool.get().await.unwrap();

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
