let player;
let videoId = null;

function page_on_load() {
  console.log("Page loaded");
  const urlParams = new URLSearchParams(window.location.search);
  videoId = urlParams.get("videoId") || null; // Default video ID

  if (videoId) {
    console.log("Video ID:", videoId);
    player = new YT.Player("video-container", {
      videoId: videoId,
      height: "450",
      width: "800",
      playerVars: {
        controls: 0,
        autoplay: 1,
      },
      events: {},
    });
  } else {
    console.log("No Video ID provided.");
    document.getElementById("message").innerHTML =
      "<h1>Enter the ID to play a video.</h1>";
  }

  // Initially hide menus
  document.getElementById("login-menu").style.visibility = "hidden";
  document.getElementById("register-menu").style.visibility = "hidden";
}

function playerControl(action) {
  if (!player) {
    document.getElementById("message").innerHTML =
      "<h1>No video player available.</h1>";
    return;
  }
  switch (action) {
    case "play":
      player.playVideo();
      break;
    case "pause":
      player.pauseVideo();
      break;
    case "stop":
      player.stopVideo();
      break;
  }
}

function navigateVideoPage() {
  window.location.href = `videoPage.html`;
}

function openLoginMenu() {
  document.getElementById("login-menu").style.visibility = "visible";
}

function closeLoginMenu() {
  document.getElementById("login-menu").style.visibility = "hidden";
}

function openRegisterPage() {
  var loginMenuOpened =
    document.getElementById("login-menu").style.visibility === "visible";
  if (loginMenuOpened) {
    closeLoginMenu();
  }
  document.getElementById("register-menu").style.visibility = "visible";
}

function closeRegisterMenu() {
  document.getElementById("register-menu").style.visibility = "hidden";
}
