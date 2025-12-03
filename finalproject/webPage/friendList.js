import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000");

ws.connectHandshake = () => {
  // login
  const userId = ""; // get from client json
  const roomId = ""; // somehow

  ws.send({
    event: "login",
    id: userId,
    room_id: roomId,
  });
};

ws.onReceive = (message) => {
  // handle received messages
};

function fetchFriendList() {
  // request friend list
  ws.send({});
}

function inviteFriend() {}
