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
          alert("User already logged in elsewhere. Logging out first.");
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
    case "logout":
      {
        const userInfo = {
          id: id,
          room_id: "", // clear room_id on logout
        };
        const payload = JSON.stringify({
          action: "SAVE_FILE",
          content: userInfo,
        });
        sendMessageToCSharp(payload);
      }
      break;
    case "join_room":
      {
        //user already in the room
        if (statusCode === 4003) {
          const payload = JSON.stringify({
            action: "USER_ALREADY_IN_ROOM",
            content: msg,
          });
          sendMessageToCSharp(payload);
        }
        // room not exist
        if (statusCode === 4004) {
          const payload = JSON.stringify({
            action: "ROOM_NOT_EXIST",
            content: msg,
          });
          sendMessageToCSharp(payload);
        }
        if (statusCode === 2000) {
          // sent new room_id to C#
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
    case "leave_room":
      {
        if (statusCode === 2000) {
          const userInfo = {
            id: id,
            room_id: roomId, // will be detribute to a new room_id by server
          };
          const payload = JSON.stringify({
            action: "SAVE_FILE",
            content: userInfo,
          });
          sendMessageToCSharp(payload);
        }
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
  const videoList = data.room.video_queue;
  const payload = JSON.stringify({
    action: "UPDATE_VIDEO_LIST",
    content: videoList,
  });
  sendMessageToCSharp(payload);

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
  } else if (state === "Next") {
    if (player && player.loadVideoById) {
      const nextVideo = roomInfo.video_queue[0];
      if (nextVideo) {
        player.loadVideoById(extractYouTubeVideoId(nextVideo.url));
      }
    }
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

function extractYouTubeVideoId(url) {
  if (!url) return null;

  var regExp =
    /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
  var match = url.match(regExp);

  if (match && match[2].length == 11) {
    return match[2];
  } else {
    console.error("無法解析 YouTube ID:", url);
    return null;
  }
}

function playerControl(action) {
  console.log(`Player action: ${action}`);
}

// define globally for HTML to access because of script type="module"
window.page_load = page_load;
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
    videoId: "",
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
  if (action === "mute") {
    if (player && player.mute) {
      player.mute();
    }
  }
  if (action === "unmute") {
    if (player && player.unMute) {
      player.unMute();
    }
  }
  if (action === "addVolume") {
    if (player && player.getVolume) {
      let currentVolume = player.getVolume();
      let newVolume = Math.min(currentVolume + 10, 100);
      player.setVolume(newVolume);
    }
  }
  if (action === "reduceVolume") {
    if (player && player.getVolume) {
      let currentVolume = player.getVolume();
      let newVolume = Math.max(currentVolume - 10, 0);
      player.setVolume(newVolume);
    }
  }
};

window.logout = async function () {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "logout",
    id: userInfo.id,
    room_id: userInfo.room_id,
  });
};

async function login() {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "login",
    id: userInfo.id,
    room_id: userInfo.room_id,
  });
}

window.joinRoom = async function (newRoomId) {
  const userInfo = await fetchUserInfo();
  alert("Joining room: " + newRoomId + " with user ID: " + userInfo.id);
  ws.send({
    event: "join_room",
    id: userInfo.id,
    room_id: newRoomId,
  });
};

window.leaveRoom = async function () {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "leave_room",
    id: userInfo.id,
    room_id: userInfo.room_id,
  });
};

window.addVideo = async function (videoUrl) {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "transmission",
    method: OperationMethod.ADD,
    value: videoUrl,
    room_id: userInfo.room_id,
  });
};

window.changeToNextVideo = async function () {
  const userInfo = await fetchUserInfo();
  ws.send({
    event: "transmission",
    method: OperationMethod.NEXT,
    value: "",
    room_id: userInfo.room_id,
  });
};
