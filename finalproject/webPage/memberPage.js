function page_on_load() {
  // Initially hide menus
  document.getElementById("member-action-page-container").style.display =
    "none";
  document.getElementById("member-invite-page-container").style.display =
    "none";
  document.getElementById("add-friend-menu").style.display = "none";
}

function openMemberInfoPage() {
  var memberActionPageOpened =
    document.getElementById("member-action-page-container").style.display ===
    "flex";
  var invitePageOpened =
    document.getElementById("member-invite-page-container").style.display ===
    "flex";
  if (memberActionPageOpened) {
    document.getElementById("member-action-page-container").style.display =
      "none";
    return;
  }
  if (invitePageOpened) {
    document.getElementById("member-invite-page-container").style.display =
      "none";
  }
  document.getElementById("member-action-page-container").style.display =
    "flex";
}
function openInvitePage() {
  var invitePageOpened =
    document.getElementById("member-invite-page-container").style.display ===
    "flex";
  var memberActionPageOpened =
    document.getElementById("member-action-page-container").style.display ===
    "flex";
  if (invitePageOpened) {
    document.getElementById("member-invite-page-container").style.display =
      "none";
    return;
  }
  if (memberActionPageOpened) {
    document.getElementById("member-action-page-container").style.display =
      "none";
  }
  document.getElementById("member-invite-page-container").style.display =
    "flex";
}

function openAddFriendMenu() {
  document.getElementById("member-action-page-container").style.display =
    "none";
  document.getElementById("add-friend-menu").style.display = "flex";
}

function closeAddFriendMenu() {
  document.getElementById("add-friend-menu").style.display = "none";
}

function logout() {
  // logout logic
  window.location.href = "index.html";
}
