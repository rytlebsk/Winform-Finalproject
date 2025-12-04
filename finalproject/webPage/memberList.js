import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000");

ws.connectHandshake = () => {
  // login
  const userId = ""; // get from client json
  const roomId = ""; // somehow

  //getCurrentRoom member list by roomId
  /*
    ws.send({
    });
    */
};

ws.onReceive = (message) => {
  // handle received messages
};

function refreshMemberList() {
  const container = document.querySelector(".member-list-container");
  //container.innerHTML = ""; // clear existing list
  const memberList = [
    {
      name: "member_1",
      avatarUrl: "picture-lake.jpg",
    },
    {
      name: "member_2",
      avatarUrl: "https://img.youtube.com/vi/dQw4w9WgXcQ/hqdefault.jpg",
    },
  ]; // get from server response
  ws.send({});
  memberList.forEach((member) => {
    addMemberItem(container, member);
  });
}

function addMemberItem(container, member) {
  const memberItem = document.createElement("div");
  memberItem.className = "member-list-box";
  memberItem.innerHTML = `
    <picture class="member-avatar">
          <img
            class="member-avatar-img"
            src="${member.avatarUrl}"
            alt="Member 1 avatar"
          />
        </picture>
        <h2 class="member-name">${member.name}</h2>
      </div>
      `;
  container.appendChild(memberItem);
}

// define globally for HTML to access because of script type="module"
window.refreshMemberList = refreshMemberList;
