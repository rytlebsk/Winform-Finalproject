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

            Task.Run(() =>
            {
                while (true)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    string reqPath = request.Url.AbsolutePath;
                    Console.WriteLine($"[Request] {request.HttpMethod} {reqPath}");

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
        private void button7_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('addVolume')");
        }
        private void button8_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('reduceVolume')");
        }
        private void button9_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('mute')");
        }
        private void button10_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("playerControl('unmute')");
        }
        private void button6_Click(object sender, EventArgs e)
        {
            string videoId = textBox1.Text;
            webView21.ExecuteScriptAsync($"addVideo('https://www.youtube.com/embed/{videoId}')");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("changeToNextVideo()");
        }
        private void button11_Click(object sender, EventArgs e)
        {
            webView21.ExecuteScriptAsync("leaveRoom()");
        }
        private void button12_Click(object sender, EventArgs e)
        {
            string roomId = roomIdLabel.Text.Replace("Room ID: ", "");
            Clipboard.SetText(roomId);
            MessageBox.Show("Room ID copied to clipboard!");
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
            joinroom joinroom = new joinroom(this);
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

                    roomIdLabel.Text = "Room ID: " + root.GetProperty("content").GetProperty("room_id").GetString();
                }
                else if(root.TryGetProperty("action", out JsonElement actionEl1) && actionEl1.GetString() == "UPDATE_VIDEO_LIST")
                {
                    string content = root.GetProperty("content").ToString();
                    webView22.ExecuteScriptAsync($"updateVideoList({content})");

                }
                else if (root.TryGetProperty("action", out JsonElement actionEl2) && actionEl2.GetString() == "ROOM_NOT_EXIST")
                {
                    MessageBox.Show("Room does not exist. Please check the room ID and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (root.TryGetProperty("action", out JsonElement actionEl3) && actionEl3.GetString() == "USER_ALREADY_IN_ROOM")
                {
                    MessageBox.Show("You are already in this room.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (root.TryGetProperty("action", out JsonElement actionEl4) && actionEl4.GetString() == "UPDATE_MEMBER_INFO")
                {
                    string content = root.GetProperty("content").ToString();
                    webView23.ExecuteScriptAsync($"updateMemberInfo({content})");

                    Console.WriteLine("member info updated！");

                    LoginMessage userInfo = readUserInfo();
                    if (userInfo.id == root.GetProperty("content").GetProperty("id").ToString())
                    {
                        userNameLabel.Text = "User: " + root.GetProperty("content").GetProperty("username").ToString() + "\t#"
                            + root.GetProperty("content").GetProperty("numeric_id").ToString();
                    }
                }
                if (root.TryGetProperty("action", out JsonElement actionEl5) && actionEl5.GetString() == "CLEAR_MEMBER_LIST")
                {
                    webView23.ExecuteScriptAsync($"clearMemberList()");

                    Console.WriteLine("member List cleared！");
                }
            }
        }


        public ClientWebSocket clientWebSocket;
        public CancellationTokenSource cancellationTokenSource;
        public async Task Connect(string serverUri)
        {
            clientWebSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Console.WriteLine($"Connecting {serverUri}...");

                await clientWebSocket.ConnectAsync(
                    new Uri(serverUri),
                    cancellationTokenSource.Token
                );

                Console.WriteLine("connect success！status：" + clientWebSocket.State);

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
        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (clientWebSocket.State == WebSocketState.Open)
                {
                    var segment = new ArraySegment<byte>(buffer);

                    WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(
                        segment,
                        cancellationTokenSource.Token
                    );

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

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        Console.WriteLine($"Receive Message: {receivedMessage}");

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
                Console.WriteLine("Receive task has been cancel.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Receive Error: {ex.Message}");
            }
        }

        //發送
        public async Task SendMessage(string message)
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                await clientWebSocket.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true,
                    cancellationTokenSource.Token
                );

                Console.WriteLine($"sent: {message}");
            }
            else
            {
                Console.WriteLine("disconnected, cant send message");
            }
        }

        public async Task Close()
        {
            if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
            {
                cancellationTokenSource.Cancel();

                await clientWebSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "client close",
                    CancellationToken.None
                );
                Console.WriteLine("webSocket connection closed");
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

        private bool isLoggedOut = false;
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isLoggedOut)
            {
                e.Cancel = true;

                try
                {
                    await webView21.ExecuteScriptAsync("logout()");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("logout failed or timeout: " + ex.Message);
                }
                finally
                {
                    isLoggedOut = true;

                    this.Close();
                }
            }
            else
            {
                Console.WriteLine("logout successful");
            }
        }

        public void saveUserInfo()
        {

        }

        public void joinRoom(string room_id)
        {
            Console.WriteLine("Joining room: " + room_id);
            this.webView21.ExecuteScriptAsync($"joinRoom('{room_id}')");
        }

        private void webView23_Click(object sender, EventArgs e)
        {

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
