import { wSocket } from "./myWS.js";

//Interface for video queue data
class VideoQueueItem {
  constructor(avatarUrl, title, url) {
    this.avatarUrl = avatarUrl;
    this.title = title;
    this.url = url;
  }
}

function fetchVideoQueue() {
  const videoQueue = [];
  refreshVideoQueue(videoQueue);
}

function refreshVideoQueue(videoQueue) {
  const container = document.querySelector(".video-queue-box");
  container.innerHTML = ""; // clear existing list
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
              src="${video.avatarUrl}"
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

window.updateVideoList = function (newVideoQueue) {
  newVideoQueue = newVideoQueue.map(
    (video) => new VideoQueueItem(video.thumbnail_url, video.title, video.url)
  );
  refreshVideoQueue(newVideoQueue);
};
