import { wSocket } from "./myWS";

const ws = new wSocket("ws://your-websocket-url");

const getCurrentRoomId = () => {
  const queryString = window.location.search;
  const urlParams = new URLSearchParams(queryString);
  return urlParams.get("room") || "";
};

// login
ws.connectHandshake = () => {
  const id = localStorage.getItem("id") || "";
  const currentRoomId = getCurrentRoomId();

  ws.send({
    id: id,
    room_id: currentRoomId,
  });
};

ws.onReceive = (message) => {
  const id = message.id;
  const roomId = message.room_id;
  const msg = message.msg;

  // Re-request if msg is 500
  if ((id && msg === 500) || (roomId && msg === 500)) {
    ws.send({
      id: id,
      room_id: roomId,
    });
    return;
  }

  localStorage.setItem("id", id);
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
