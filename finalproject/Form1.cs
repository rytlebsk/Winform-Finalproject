using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace finalproject
{
    public partial class Form1 : Form
    {
        string videoId = null;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await webView21.EnsureCoreWebView2Async();
            webView21.WebMessageReceived += WebView21_WebMessageReceived;

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Prefixes.Add("http://localhost:8000/");
            listener.Prefixes.Add("http://localhost:8964/");
            listener.Start();

            var content = new FileStream("videoQueue.html", FileMode.Open, FileAccess.Read);
            Task.Run(() =>
            {
                while (true)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    string defaultFile = "videoQueue.html";
                    if (request.LocalEndPoint.Port == 8000)
                        defaultFile = "memberList.html";
                    if (request.LocalEndPoint.Port == 8964)
                        defaultFile = "video.html";

                    string filename = request.Url.AbsolutePath.TrimStart('/');
                    if (string.IsNullOrEmpty(filename))
                        filename = defaultFile;

                    // 檔案完整路徑
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                    if (File.Exists(filePath))
                    {
                        // 設定Content-Type
                        if (filename.EndsWith(".css"))
                            response.ContentType = "text/css";
                        else if (filename.EndsWith(".js"))
                            response.ContentType = "application/javascript";
                        else if (filename.EndsWith(".html"))
                            response.ContentType = "text/html";
                        else
                            response.ContentType = "application/octet-stream";

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
                }
            });

            try
            {
                webView21.Source = new Uri("http://localhost:8964/");
                webView22.Source = new Uri("http://localhost:8080/");
                webView23.Source = new Uri("http://localhost:8000/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to web page: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('play')");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('pause')");
        }
        private void button4_Click_1(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('rewind')");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('forward')");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*
            videolist.Items.RemoveAt(0);
            string id = videolist.Items[0].ToString();

            if (id != videoId)
            {
                if (id.Length != 0)
                {
                    try
                    {
                        webView21.Source = new Uri("http://localhost:8080/?videoId=" + id);
                        videoId = id;
                    }
                    catch
                    {
                        MessageBox.Show("Error");
                    }
                }
            }
            else
            {
                webView21.ExecuteScriptAsync("playerControl('play')");
            }
            */
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    videolist.Items.Add(textBox1.Text);
                    textBox1.Clear();
                }
            }
            */
        }

        private void webView22_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2();
            newForm.ShowDialog();
        }

        private void friendlist_Click(object sender, EventArgs e)
        {
            friendlist friendlist = new friendlist();
            friendlist.ShowDialog();
        }

        private void joinroom_Click(object sender, EventArgs e)
        {
            joinroom joinroom = new joinroom();
            joinroom.ShowDialog();
        }

        private void findvideo_Click(object sender, EventArgs e)
        {
            findvideo findvideo = new findvideo();
            findvideo.ShowDialog();
        }

        public void WebView21_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();
            Console.WriteLine("Received message from WebView2: " + message);

            using (JsonDocument doc = JsonDocument.Parse(message))
            {
                JsonElement root = doc.RootElement;

                // 判斷動作
                if (root.TryGetProperty("action", out JsonElement actionEl) && actionEl.GetString() == "SAVE_FILE")
                {
                    // 取得要存的內容
                    string contentToSave = root.GetProperty("content").ToString(); // 轉回 JSON 字串

                    // 寫入檔案
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userInfo.json");
                    File.WriteAllText(path, contentToSave);

                    Console.WriteLine("save success！");
                }
                else if(root.TryGetProperty("action", out JsonElement actionEl1) && actionEl1.GetString() == "UPDATE_VIDEO_LIST")
                {
                    string content = root.GetProperty("content").ToString();
                    webView22.ExecuteScriptAsync($"updateVideoList({content})");

                    Console.WriteLine("video list updated！");
                }
            }

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

                        // save json
                        var jsonDoc = JsonDocument.Parse(receivedMessage);
                        var root = jsonDoc.RootElement;
                        string message = root.GetProperty("msg").GetString();
                        switch (message)
                        {
                            case "Room status changed":

                                break;
                            default:
                                object dataToSave = new
                                {
                                    id = root.GetProperty("id").GetString(),
                                    room_id = root.GetProperty("room_id").GetString(),
                                };
                                await SaveJsonAsync(dataToSave, "userInfo.json");
                                break;
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
        public async Task SaveJsonAsync(object data, string filename)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

            using (FileStream createStream = File.Create(filePath))
            {
                await JsonSerializer.SerializeAsync(createStream, data, options);
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
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            webView21.ExecuteScriptAsync("logout");
            Console.WriteLine("Form closed, logout sent.");
            Close();
        }

        public void saveUserInfo()
        {

        }

        public void joinRoom(string room_id)
        {
            Console.WriteLine("Joining room: " + room_id);
            webView21.ExecuteScriptAsync($"joinRoom('{room_id}')");
        }

    }

    public class SocketMessages
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }
    }

    public class LoginMessage : SocketMessages
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        [JsonPropertyName("room_id")]
        public string roomId { get; set; }
        [JsonPropertyName("msg")]
        public string message { get; set; }
    }
}
