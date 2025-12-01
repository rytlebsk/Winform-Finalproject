export class ws {
  constructor(_url) {
    this.url = _url;
    this.ws = null;
    this.reconnectInterval = 3000; // 3 seconds

    // receive message handler
    this.onReceive = null;

    this.connect();
  }
  connect() {
    this.ws = new WebSocket(this.url);

    this.ws.onopen = () => {
      console.log("Connected to video queue WebSocket server.");
    };

    this.ws.onmessage = (event) => {
      try {
        const message = JSON.parse(event.data);
        this.handleMessage(message);
      } catch (e) {
        console.error("Receive: Error parsing message:", e);
      }
    };

    this.ws.onclose = () => {
      console.log("Video queue WebSocket connection closed. Reconnecting...");
      this.ws = null;
      //reconnect after a delay
      setTimeout(() => this.connect(), this.reconnectInterval);
    };

    this.ws.onerror = (error) => {
      console.error("Video queue WebSocket error:", error);
      this.ws.close();
    };
  }

  send(data) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(data));
    } else {
      console.error("WebSocket is not open. Unable to send message.");
    }
  }
}
