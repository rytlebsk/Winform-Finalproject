use axum::extract::ws::{self, Message, WebSocket};
use axum::{
    Json, Router,
    extract::{self, State, WebSocketUpgrade},
    response::IntoResponse,
    routing::{get, post},
};
use backend::controller::controller::CONTROLLER;
use backend::events::event::{ConnectionPool, EventParms, WsSender, login};
use bb8::Pool;
use bb8_postgres::PostgresConnectionManager;
use chrono::{DateTime, Utc};
use futures_util::{SinkExt, StreamExt};
use serde_json::json;
use std::sync::Arc;
use tokio::net::TcpListener;
use tokio::sync::Mutex;
use tokio_postgres::NoTls;
use uuid::Uuid;

#[derive(Clone)]
struct DatabaseConnectionState {
    pool: Arc<ConnectionPool>,
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
                            .send(Message::Text(ws::Utf8Bytes::from(
                                json!({
                                    "status_code":4001,
                                    "msg":format!("Invalid JSON format"),
                                })
                                .to_string(),
                            )))
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
