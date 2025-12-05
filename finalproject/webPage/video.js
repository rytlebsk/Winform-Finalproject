import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000/");
// login
ws.connectHandshake = async () => {
  const userInfo = await fetchUserInfo();
  //fetch video queue
  ws.send({
    event: "login",
    id: userInfo.id,
    room_id: userInfo.room_id,
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
      break;
    case 4000:
      console.error("request format error");
      break;
    case 4003:
      console.log("user already logged in");
      break;
    case 4004:
      console.log("room not found or user not found");
      break;
  }

  // sent login data to C#
  const userInfo = {
    id: id,
    room_id: roomId,
  };
  const payload = JSON.stringify({
    action: "SAVE_FILE",
    content: userInfo,
  });
  sendMessageToCSharp(payload);
};

function sendMessageToCSharp(msg) {
  console.log("Attempting to send message to C#:", msg);
  // 1. 先檢查 window.chrome 是否存在
  // 2. 再檢查 window.chrome.webview 是否存在
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage(msg);
  } else {
    console.warn(
      "目前不在 WebView2 環境中，或是 Webview 尚未載入完成，無法傳送訊息。"
    );
    alert(
      "目前不在 WebView2 環境中，或是 Webview 尚未載入完成，無法傳送訊息。"
    );
    // 你可以在這裡寫個 alert 方便除錯
    // alert("無法連線到 C#");
  }
}

async function page_load() {
  const data = await fetchUserInfo();
  console.log(data);
}

async function fetchUserInfo() {
  const response = await fetch("userInfo.json");
  // if dont exist
  if (!response.ok) {
    data = {
      id: "",
      room_id: "",
    };
    return data;
  }
  const data = await response.json();
  return data;
}

function updateRoomUrl(newRoomId) {
  const url = new URL(window.location);

  url.searchParams.set("room", newRoomId);
  window.history.pushState({}, "", url);
}

function playerControl(action) {
  console.log(`Player action: ${action}`);
}

// define globally for HTML to access because of script type="module"
window.page_load = page_load;
window.updateRoomUrl = updateRoomUrl;
window.playerControl = playerControl;
window.sendMessageToCSharp = sendMessageToCSharp;
