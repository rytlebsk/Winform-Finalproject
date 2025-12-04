import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://localhost:3000/");

ws.connectHandshake = () => {
  //fetch video queue after login
  fetchVideoQueue();
};

ws.onReceive = (message) => {
  console.log("Received video queue message:", message);
  // Handle the received video queue message here
};

//Interface for video queue data
class VideoQueueItem {
  constructor(id, title, url) {
    this.id = id;
    this.title = title;
    this.url = url;
  }
}

// receive handler
ws.onReceive = (message) => {
  console.log("Received video queue message:", message);
  // Handle the received video queue message here
};

function fetchVideoQueue() {
  ws.send({ action: "getVideoQueue" });
  // Simulated video queue data; replace with server response
  const videoQueue = [
    new VideoQueueItem(1, "Sample Video 1", "https://www.example.com/video1"),
    new VideoQueueItem(2, "Sample Video 2", "https://www.example.com/video2"),
    new VideoQueueItem(3, "Sample Video 3", "https://www.example.com/video3"),
  ];
  refreshVideoQueue(videoQueue);
}

function refreshVideoQueue(videoQueue) {
  const container = document.querySelector(".video-queue-box");
  //container.innerHTML = ""; // clear existing list
  videoQueue.forEach((video) => {
    addVideoQueueItem(container, video);
  });
}

function addVideoQueueItem(container, video) {
  const videoItem = document.createElement("div");
  videoItem.className = "video-item-box";
  videoItem.innerHTML = `
  <div class="video-preview-picture">
            <img
              class="video-preview-img"
              src="${video.url}"
              alt="Video Preview"
            />
          </div>
          <div class="video-info-box">
            <div class="video-title">
              <p>${video.title}</p>
            </div>
          </div>
        </div>
  `;
  container.appendChild(videoItem);
}

// define globally for HTML to access because of script type="module"
window.refreshVideoQueue = refreshVideoQueue;
window.fetchVideoQueue = fetchVideoQueue;
