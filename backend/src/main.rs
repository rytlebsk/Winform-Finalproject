use axum::{
    Json, Router,
    extract::{self, State},
    http::StatusCode,
    response::IntoResponse,
    routing::{get, post},
};
use bb8::Pool;
use bb8_postgres::PostgresConnectionManager;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use serde_json::json;
use std::sync::Arc;
use tokio::net::TcpListener;
use tokio_postgres::NoTls;
use uuid::Uuid;

type ConnectionPool = Pool<PostgresConnectionManager<tokio_postgres::NoTls>>;

#[derive(Clone)]
struct DatabaseConnectionState {
    pool: Arc<ConnectionPool>,
}

#[derive(Serialize, Deserialize, Debug)]
struct LoginRequestPayload {
    id: String,
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

#[derive(Serialize, Deserialize, Debug)]
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
        .route("/", get(|| async { "Hello, World!" }))
        .route("/health", get(|| async { "OK" })) //render health check endpoint
        //login when open
        .route("/login", post(login))
        .with_state(DatabaseConnectionState {
            pool: Arc::new(pool.clone()),
        })
        //logout when close
        .route("/logout", post(logout))
        .with_state(AppState {
            pool: Arc::new(pool.clone()),
        })
        //insert test data
        .route("/insert_test", get(insert_test))
        .with_state(AppState {
            pool: Arc::new(pool.clone()),
        });

    let listener = TcpListener::bind("0.0.0.0:80").await.unwrap();
    axum::serve(listener, app).await.unwrap();
    Ok(())
}

//login when open
async fn login(
    State(state): State<DatabaseConnectionState>,
    extract::Json(payload): extract::Json<LoginRequestPayload>,
) -> impl IntoResponse {
    //search database by id
    let mut id: String = payload.id;
    let uuid_id = Uuid::parse_str(&id).unwrap_or(Uuid::nil());

    match state.pool.get().await {
        Ok(client) => {
            let user_exists = match client
                .query_one(
                    "SELECT EXISTS(SELECT 1 FROM users WHERE id = $1)",
                    &[&uuid_id],
                )
                .await
            {
                Err(_) => false,
                Ok(_) => true,
            };

            /* register */
            if id.is_empty() || !user_exists {
                //create new user
                let user = User {
                    id: Uuid::new_v4(),
                    username: "new_user".to_string(),
                    avatar_url: "http://example.com/avatar.png".to_string(),
                    last_login: Utc::now(),
                    created_at: Utc::now(),
                    friends: vec![],
                    status: Uuid::new_v4(),
                    online: false,
                };

                //insert new user into database
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
                    Err(e) => return (
                        StatusCode::INTERNAL_SERVER_ERROR,
                        Json(LoginResponsePayload {
                            id: id.clone(),
                            room_id: String::new(),
                            msg: format!("Database insertion failed: {}", e),
                        }),
                    ),
                    Ok(_) => (
                        id = user.id.to_string(),
                    ),
                };
            }

            /* login */

            //generate room id (for simplicity, use a fixed room id here)
            let room_id = Uuid::new_v4();

            //update last login time & tag as online
            match client
                .execute(
                    "UPDATE users SET last_login = $1, online = TRUE, status = $2 WHERE id = $3",
                    &[&Utc::now(), &room_id, &uuid_id],
                )
                .await
            {
                Err(e) => {
                    return (
                        StatusCode::INTERNAL_SERVER_ERROR,
                        Json(LoginResponsePayload {
                            id: id.clone(),
                            room_id: String::new(),
                            msg: format!("Database update failed: {}", e),
                        }),
                    );
                }
                Ok(_) => {
                    ("User login time updated and tagged as online successfully").into_response()
                }
            };

            return (
                StatusCode::OK,
                Json(LoginResponsePayload {
                    id: id.clone(),
                    room_id: room_id.to_string(),
                    msg: "User logged in successfully".to_string(),
                }),
            );
        }
        Err(_) => {
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(LoginResponsePayload {
                    id: id.clone(),
                    room_id: String::new(),
                    msg: "Failed to get database connection".to_string(),
                }),
            );
        }
    };
}

async fn logout(
    State(state): State<DatabaseConnectionState>,
    extract::Json(payload): extract::Json<LogoutRequestPayload>,
) -> impl IntoResponse {
    let id: String = payload.id;
    let uuid_id = Uuid::parse_str(&id).unwrap_or(Uuid::nil());
    let client = match state.pool.get().await {
        Ok(client) => client,
        Err(_) => {
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(json!({"id": id, "msg": "Failed to get database connection"})),
            );
        }
    };

    match client
        .execute("UPDATE users SET online = FALSE WHERE id = $1", &[&uuid_id])
        .await
    {
        Err(e) => {
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(json!({"id": id, "msg": format!("Database update failed: {}", e)})),
            );
        }
        Ok(_) => (
            StatusCode::OK,
            Json(json!({"id": id, "msg": "User tagged as offline successfully"})),
        ),
    }
}

async fn run_init(pool: ConnectionPool) {
    let client = pool.get().await.unwrap();

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

async fn insert_test(State(state): State<AppState>) -> impl IntoResponse {
    //insert test data into database
    let client = match state.pool.get().await {
        Ok(client) => client,
        Err(_) => return Err("Failed to get database connection".into()),
    };

    let user = User {
        id: Uuid::new_v4(),
        username: "test_user".to_string(),
        avatar_url: "http://example.com/avatar.png".to_string(),
        last_login: Utc::now(),
        created_at: Utc::now(),
        friends: vec![],
        status: Uuid::new_v4(),
        online: true,
    };

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
        .await{
        Err(e) => return Err(format!("Database insertion failed: {}", e)),
        Ok(_) => (),
        };

    match client
        .query_one("SELECT * FROM users WHERE id = $1", &[&user.id])
        .await
    {
        Err(e) => return Err(format!("Database query failed: {}", e)),
        Ok(row) => {
            let fetched_username: String = row.get("name");
            if (fetched_username == user.username) {
                Ok(format!("User inserted successfully: {:?}", user))
            } else {
                Err("Inserted user data does not match.".to_string())
            }
        }
    }
}
