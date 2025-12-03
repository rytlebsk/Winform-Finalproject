import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000/");

const getCurrentRoomId = () => {
  const queryString = window.location.search;
  const urlParams = new URLSearchParams(queryString);
  return urlParams.get("room") || "";
};

// login
ws.connectHandshake = () => {
  const id = localStorage.getItem("id") || "";
  const currentRoomId = getCurrentRoomId() || "";

  console.log("WebSocket handshake sent:", { id: id, room_id: currentRoomId });
  ws.send({
    event: "login",
    id: id,
    room_id: currentRoomId,
  });
};

ws.onReceive = (message) => {
  const id = message.id;
  const roomId = message.room_id;
  const msg = message.msg;
  const statusCode = message.status_code;

  console.log("WebSocket message processed:", message);

  switch (statusCode) {
    case 2000:
      console.log("Login successful.");
      localStorage.setItem("id", id);
      break;
    case 4000:
      console.error("request format error");
      return;
    case 4003:
      console.log("user already logged in");
      updateRoomUrl(getCurrentRoomId());
      return;
    case 4004:
      if (msg === "User ID does not exist") {
        console.log("clean localStorage and relogin");
        localStorage.removeItem("id");
        const roomId = getCurrentRoomId() || "";
        ws.send({
          event: "login",
          id: id,
          room_id: roomId,
        });
      } else if (msg === "Room ID does not exist") {
        updateRoomUrl(getCurrentRoomId());
      }
      return;
  }

  if (roomId !== getCurrentRoomId()) {
    updateRoomUrl(roomId);
    // fetch room info
  }
};

function updateRoomUrl(newRoomId) {
  const url = new URL(window.location);

  url.searchParams.set("room", newRoomId);
  window.history.pushState({}, "", url);
}
