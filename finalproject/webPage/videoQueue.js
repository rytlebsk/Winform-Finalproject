import { wSocket } from "./myWS.js";

const ws = new wSocket("ws://your-websocket-url");

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

function requestVideoQueue() {
  ws.send({ action: "getVideoQueue" });
}
