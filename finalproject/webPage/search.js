function openAddVideoConfirmPage() {
  document.getElementById("videoConfirmContainer").style.display = "flex";
}

function confirmAddVideo() {
  // Here you can add the logic to actually add the video
  document.getElementById("videoConfirmContainer").style.display = "none";
}

function cancelAddVideo() {
  document.getElementById("videoConfirmContainer").style.display = "none";
}
