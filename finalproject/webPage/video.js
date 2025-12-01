function checkVideoQueueEmpty() {
  var iframe = document.getElementById("video-item");
  var defaultScreen = document.getElementById("default-screen");
  var currentSrc = iframe.src;
  if (currentSrc === "about:blank") {
    iframe.style.display = "none";
    defaultScreen.style.display = "flex";
    return;
  }
  iframe.style.display = "flex";
  defaultScreen.style.display = "none";
}
