using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace finalproject
{
    public partial class joinroom : Form
    {
        private HttpListener listener;
        private Form1 _parentForm;
        public joinroom(Form1 parentForm)
        {
            InitializeComponent();
            _parentForm = parentForm;
        }

        private async void joinroom_Load(object sender, EventArgs e)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8777/");
            listener.Start();

            // 2. 在背景啟動 Server 監聽
            var content = new FileStream("videoQueue.html", FileMode.Open, FileAccess.Read);
            Task.Run(() =>
            {
                // 檢查 listener 是否存在且正在監聽
                while (listener != null && listener.IsListening)
                {
                    try
                    {
                        // 這裡會暫停等待請求
                        // 當你在 FormClosing 呼叫 Stop() 時，這裡會報錯，這是正常的
                        var context = listener.GetContext();

                        // --- 處理請求的邏輯 ---
                        var request = context.Request;
                        var response = context.Response;

                        // 取得檔名與路徑邏輯...
                        string filename = request.Url.AbsolutePath.TrimStart('/');
                        if (string.IsNullOrEmpty(filename)) filename = "videoList.html";
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                        if (File.Exists(filePath))
                        {
                            // 設定 Content-Type
                            if (filename.EndsWith(".css")) response.ContentType = "text/css";
                            else if (filename.EndsWith(".js")) response.ContentType = "application/javascript";
                            else if (filename.EndsWith(".html")) response.ContentType = "text/html";
                            else response.ContentType = "application/octet-stream";

                            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                response.ContentLength64 = fs.Length;
                                fs.CopyTo(response.OutputStream);
                            }
                        }
                        else
                        {
                            response.StatusCode = 404;
                        }
                        response.OutputStream.Close();
                        // --- 處理結束 ---
                    }
                    catch (HttpListenerException ex)
                    {
                        // 錯誤代碼 995 代表 "I/O 操作已中止"，這就是我們呼叫 Stop() 時會發生的錯誤
                        // 這種情況下，我們只需要跳出迴圈即可
                        if (ex.ErrorCode == 995 || !listener.IsListening)
                        {
                            break;
                        }
                        Console.WriteLine("HttpListenerException: " + ex.Message);
                    }
                    catch (ObjectDisposedException)
                    {
                        // 如果物件已經被 Dispose，也直接跳出
                        break;
                    }
                    catch (Exception ex)
                    {
                        // 其他未知的錯誤才需要顯示
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            });

            // 3. 設定單一 WebView2
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("join room Error: " + ex.Message);
            }
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void joinroom_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (listener != null && listener.IsListening)
            {
                try
                {
                    listener.Stop();  // 停止監聽，這會導致 GetContext() 拋出例外並結束 Task
                    listener.Close(); // 釋放資源
                }
                catch (Exception ex)
                {
                    // 處理關閉時可能的錯誤，通常可以忽略
                    Console.WriteLine("Closing Error: " + ex.Message);
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string roomId = textBox1.Text;
            _parentForm.joinRoom(roomId);
        }

        //websocket
        public ClientWebSocket clientWebSocket;
        public CancellationTokenSource cancellationTokenSource;
        public async Task Connect(string serverUri)
        {
            clientWebSocket = new ClientWebSocket();
            // 使用 CancellationTokenSource 来管理连接和接收任务的取消
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Console.WriteLine($"Connecting {serverUri}...");

                // 建立连接
                await clientWebSocket.ConnectAsync(
                    new Uri(serverUri),
                    cancellationTokenSource.Token
                );

                Console.WriteLine("connect success！status：" + clientWebSocket.State);

                // 连接成功后，启动后台任务来持续接收消息
                Task.Run(() => ReceiveLoop());

            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"connect error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error: {ex.Message}");
            }
        }
        //接收
        private async Task ReceiveLoop()
        {
            // 定义一个缓冲区来存储接收到的数据
            var buffer = new byte[1024 * 4];

            try
            {
                while (clientWebSocket.State == WebSocketState.Open)
                {
                    // 创建一个 ArraySegment 来接收数据
                    var segment = new ArraySegment<byte>(buffer);

                    // 异步等待接收数据
                    WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(
                        segment,
                        cancellationTokenSource.Token
                    );

                    // 检查连接是否被服务器关闭
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await clientWebSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "由服务器发起关闭",
                            CancellationToken.None
                        );
                        Console.WriteLine("websocket disconnected");
                        break;
                    }

                    // 处理接收到的文本消息
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // 将接收到的字节转换为字符串
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        // *** 在这里处理您的业务逻辑 ***
                        Console.WriteLine($"Receive Message: {receivedMessage}");
                        // 如果是 UI 应用，需要在这里使用 Dispatcher 或 SynchronizationContext 更新 UI

                        var jsonDoc = JsonDocument.Parse(receivedMessage);
                        var root = jsonDoc.RootElement;
                        RoomReceiveMessage data = new RoomReceiveMessage
                        {
                            Id = root.GetProperty("id").GetString(),
                            RoomId = root.GetProperty("room_id").GetString(),
                            Msg = root.GetProperty("msg").GetString(),
                            statusCode = root.GetProperty("status_code").GetInt32()
                        };
                        if(data.statusCode == 2000)
                        {
                            Console.WriteLine("join room success");
                        }
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                Console.WriteLine("unintentially disconnect");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("消息接收任务被取消。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收消息时发生错误: {ex.Message}");
            }
        }

        //發送
        public async Task SendMessage(string message)
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                // 1. 将字符串编码为 UTF-8 字节数组
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                // 2. 异步发送消息
                await clientWebSocket.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true, // endOfMessage: true 表示这是一个完整的消息
                    cancellationTokenSource.Token
                );

                Console.WriteLine($"sent: {message}");
            }
            else
            {
                Console.WriteLine("disconnected, cant send message");
            }
        }

        //關機
        public async Task Close()
        {
            if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
            {
                // 1. 取消正在运行的接收任务
                cancellationTokenSource.Cancel();

                // 2. 异步关闭连接
                await clientWebSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "客户端主动关闭",
                    CancellationToken.None
                );
                Console.WriteLine("连接已关闭。");
            }
        }
        public LoginMessage readUserInfo()
        {
            return ReadJSONFile("userInfo.json");
        }

        public LoginMessage ReadJSONFile(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                LoginMessage result = JsonSerializer.Deserialize<LoginMessage>(jsonString, options);

                return result;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("cannot find the file");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parse error: {ex.Message}");
                return null;
            }
        }
    }

    public class RoomMessage
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("room_id")]
        public string RoomId { get; set; }
    }

    public class RoomReceiveMessage : RoomMessage
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
        [JsonPropertyName("status_code")]
        public int statusCode { get; set; }
    }
}
