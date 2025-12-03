using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace finalproject
{
    public partial class Form2 : Form
    {
        private HttpListener listener;
        public Form2()
        {
            InitializeComponent();
        }

        private async void webView21_Click(object sender, EventArgs e)
        {
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8591/");
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
                        if (string.IsNullOrEmpty(filename)) filename = "video.html";
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
                webView21.Source = new Uri("http://localhost:8591/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("add friend Error: " + ex.Message);
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
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
    }
}
