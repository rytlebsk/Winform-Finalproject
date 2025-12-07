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

const memberList = [];

function refreshMemberList() {
  const container = document.querySelector(".member-list-container");
  container.innerHTML = ""; // clear existing list
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
            alt="Member ${member.numeric_id} avatar"
          />
        </picture>
        <h2 class="member-name">${member.username}  #${member.numeric_id}</h2>
      </div>
      `;
  container.appendChild(memberItem);
}

// define globally for HTML to access because of script type="module"
window.refreshMemberList = refreshMemberList;

// C# message handler
window.updateMemberInfo = (memberInfo) => {
  memberList.push(memberInfo);
  refreshMemberList();
};

window.clearMemberList = () => {
  memberList.length = 0;
  refreshMemberList();
};
