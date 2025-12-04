import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000");

ws.connectHandshake = () => {
  // login
  const userId = ""; // get from client json
  const roomId = ""; // somehow

  /*
  ws.send({
    event: "login",
    id: userId,
    room_id: roomId,
  });
  */
};

ws.onReceive = (message) => {
  // handle received messages
};

function fetchFriendList() {
  // request friend list
  const friendList = [
    {
      name: "homo_sapien",
      avatarUrl: "https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg",
      isOnline: true,
    },
    {
      name: "homo_sapien1",
      avatarUrl: "https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg",
      isOnline: true,
    },
    {
      name: "homo_sapien2",
      avatarUrl: "https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg",
      isOnline: false,
    },
  ]; // get from server response

  const container = document.querySelector(".friend-list-container");
  container.innerHTML = ""; // clear existing list
  ws.send({});
  friendList.forEach((friend) => {
    const friendItem = document.createElement("div");
    friendItem.className = "friend-item-container";
    friendItem.innerHTML = `
      <div class="friend-item-box">
        <picture class="friend-avatar">
          <img
            class="friend-avatar-img"
            src="${friend.avatarUrl}"
            alt="Friend Avatar"
          />
        </picture>
        <p class="friend-name">${friend.name}</p>
        <div class="friend-status-container">
          <span class="${
            friend.isOnline
              ? "friend-status-indicator-online"
              : "friend-status-indicator-offline"
          }"></span>
          <span class="friend-status-text">${
            friend.isOnline ? "Online" : "Offline"
          }</span>
        </div>
        <div class="friend-action-buttons">
          <button class="invite-button" onclick="inviteFriend()">
            Invite
          </button>
          <button class="delete-button" onclick="deleteFriend()">
            X
          </button>
        </div>
      </div>
    </div>
    `;
    container.appendChild(friendItem);
  });
}

function inviteFriend() {
  //get friend room_id by send uid
  ws.send({});
  // generate invite link
  const urlString = new URL(window.location).host + "/video.html";
  console.log("Current URL:", urlString);
  const url = new URL("http://" + urlString);
  const newRoomId = "some-generated-room-id"; // get from server response
  url.searchParams.set("room", newRoomId);

  navigator.clipboard.writeText(url.toString()).then(() => {
    alert("Invite link copied to clipboard!");
  });
}

function deleteFriend() {
  // TODO: implement delete logic (send ws message or call API)
  //delete event
  ws.send({});
  fetchFriendList(); // refresh list
}

// Expose functions to global scope so inline handlers in HTML can call them
// Module scripts do not automatically populate `window` with their functions.
window.fetchFriendList = fetchFriendList;
window.inviteFriend = inviteFriend;
window.deleteFriend = deleteFriend;
