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

  let finalSrc = member.avatar_url || "assets/default_avatar.png";
  const timestamp = new Date().getTime();
  finalSrc += `?t=${timestamp}`;

  memberItem.innerHTML = `
    <picture class="member-avatar">
          <img
            class="member-avatar-img"
            src="${finalSrc}"
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
