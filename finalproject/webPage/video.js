import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000/");

const OperationMethod = {
  ADD: "Add",
  NEXT: "Next",
  PAUSE: "Pause",
  PLAY: "Play",
  FORWARD: "Forward",
  BACKWARD: "Backward",
};

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
  console.log("WebSocket message processed:", message);
  const event = message.event;

  const id = message.id;
  const roomId = message.room_id;
  const msg = message.msg;
  const statusCode = message.status_code;

  switch (event) {
    case "login":
      {
        if (statusCode === 4003) {
          // logout and login again
          logout();
          login();
        }
        if (statusCode === 2000) {
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
        }
      }
      break;
    case "join_room":
      {
        console.log(`Joined room: ${roomId}`);
      }
      break;
    case "update":
      {
        updateRoom(message);
      }
      break;
  }
};

function updateRoom(data) {
  if (data.status_code !== 2001) {
    console.log(data.msg);
    return;
  }
  // update video list
  // update members list
  const roomInfo = {
    room_id: data.room.id,
    members: data.room.members,
    video_playing: data.room.video_playing,
    video_queue: data.room.video_queue,
  };
  const state = data.state;

  // video control
  if (state === "Play") {
    player.playVideo();
  } else if (state === "Pause") {
    player.pauseVideo();
  } else if (state === "Forward") {
    const newTime = player.getCurrentTime() + 10; // forward 10 seconds
    player.seekTo(newTime, true);
  } else if (state === "Backward") {
    const newTime = player.getCurrentTime() - 10; // backward 10 seconds
    player.seekTo(newTime, true);
  }
}

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
window.updateRoom = updateRoom;

// -----YouTube IFrame API setup ------
var tag = document.createElement("script");
tag.src = "https://www.youtube.com/iframe_api";
var firstScriptTag = document.getElementsByTagName("script")[0];
if (firstScriptTag) {
  firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
} else {
  document.head.appendChild(tag);
}

var player;

window.onYouTubeIframeAPIReady = function () {
  player = new YT.Player("player", {
    height: "100%",
    width: "100%",
    videoId: "Y0SWx_n_AeQ",
    playerVars: {
      playsinline: 1,
      controls: 1,
      rel: 0,
    },
    events: {
      onReady: onPlayerReady,
      onStateChange: onPlayerStateChange,
    },
  });
};

function onPlayerReady(event) {
  event.target.playVideo();
}

function onPlayerStateChange(event) {
  if (event.data == YT.PlayerState.PLAYING) {
    console.log("Video is playing");
  }

  if (event.data == YT.PlayerState.PAUSED) {
    console.log("偵測到暫停，時間:", player.getCurrentTime());
    notifyCSharp("Pause", player.getCurrentTime());
  }

  if (event.data == YT.PlayerState.BUFFERING) {
    console.log("偵測到跳轉/緩衝");
  }
}

function notifyCSharp(action, time) {
  // 防呆檢查
  if (window.chrome && window.chrome.webview) {
    var payload = JSON.stringify({ action: action, time: time });
    window.chrome.webview.postMessage(payload);
  } else {
    console.log("不在 WebView2 環境，無法傳送訊息:", action);
  }
}

window.playerControl = async function (action) {
  const userInfo = await fetchUserInfo();
  if (action === "pause") {
    ws.send({
      event: "transmission",
      method: OperationMethod.PAUSE,
      value: "",
      room_id: userInfo.room_id,
    });
  }
  if (action === "play") {
    ws.send({
      event: "transmission",
      method: OperationMethod.PLAY,
      value: "",
      room_id: userInfo.room_id,
    });
  }
  if (action === "next") {
    ws.send({
      event: "transmission",
      method: OperationMethod.NEXT,
      value: "",
      room_id: userInfo.room_id,
    });
  }
  if (action === "forward") {
    ws.send({
      event: "transmission",
      method: OperationMethod.FORWARD,
      value: "",
      room_id: userInfo.room_id,
    });
  }
  if (action === "rewind") {
    ws.send({
      event: "transmission",
      method: OperationMethod.BACKWARD,
      value: "",
      room_id: userInfo.room_id,
    });
  }
};

async function logout() {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "logout",
    id: userInfo.id,
    room_id: userInfo.room_id,
  });
}

async function login() {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "login",
    id: userInfo.id,
    room_id: userInfo.room_id,
  });
}

window.joinRoom = async function (newRoomId) {
  alert(`Joining room: ${newRoomId}`);
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "join_room",
    id: userInfo.id,
    room_id: newRoomId,
  });
};
