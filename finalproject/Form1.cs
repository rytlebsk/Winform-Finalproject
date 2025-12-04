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

namespace finalproject
{
    public partial class Form1 : Form
    {
        string videoId = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Connect("ws://localhost:3000");
            // Login
            SendMessage("{}");
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
            /*
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (videoId != null) webView21.ExecuteScriptAsync("playerControl('pause')");
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
                Console.WriteLine($"正在连接到 {serverUri}...");

                // 建立连接
                await clientWebSocket.ConnectAsync(
                    new Uri(serverUri),
                    cancellationTokenSource.Token
                );

                Console.WriteLine("连接成功！状态：" + clientWebSocket.State);

                // 连接成功后，启动后台任务来持续接收消息
                Task.Run(() => ReceiveLoop());

            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"连接失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生未知错误: {ex.Message}");
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
                        Console.WriteLine("服务器关闭了连接。");
                        break;
                    }

                    // 处理接收到的文本消息
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // 将接收到的字节转换为字符串
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        // *** 在这里处理您的业务逻辑 ***
                        Console.WriteLine($"收到服务器消息: {receivedMessage}");
                        // 如果是 UI 应用，需要在这里使用 Dispatcher 或 SynchronizationContext 更新 UI
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                Console.WriteLine("连接意外中断。");
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

                Console.WriteLine($"已发送: {message}");
            }
            else
            {
                Console.WriteLine("连接未打开，无法发送消息。");
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
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }
    }
}
