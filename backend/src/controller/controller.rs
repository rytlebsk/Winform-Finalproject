use crate::events::event::{Room, User, WsSender};
use once_cell::sync::Lazy;
use std::collections::HashMap;
use tokio::sync::Mutex;
use uuid::Uuid;

pub struct Controller {
    pub users: HashMap<Uuid, User>,
    pub rooms: HashMap<Uuid, Room>,
    pub ws_senders: HashMap<Uuid, WsSender>,
}

pub static CONTROLLER: Lazy<Mutex<Controller>> = Lazy::new(|| {
    Mutex::new(Controller {
        users: HashMap::new(),
        rooms: HashMap::new(),
        ws_senders: HashMap::new(),
    })
});
