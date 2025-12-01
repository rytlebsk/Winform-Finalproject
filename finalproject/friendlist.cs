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
    public partial class friendlist : Form
    {
        public friendlist()
        {
            InitializeComponent();
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void friendlist_Load(object sender, EventArgs e)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8666/");
            listener.Start();

            // 2. 在背景啟動 Server 監聽
            var content = new FileStream("videoQueue.html", FileMode.Open, FileAccess.Read);
            Task.Run(() =>
            {
                while (true)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;
                    // 取得請求的檔名
                    string filename = request.Url.AbsolutePath.TrimStart('/');

                    // 設定「預設首頁」：如果沒有指定檔名，就給 video.html
                    if (string.IsNullOrEmpty(filename))
                    {
                        filename = "video.html";
                    }

                    // 組合完整的檔案路徑
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                    if (File.Exists(filePath))
                    {
                        // 根據副檔名設定 Content-Type
                        if (filename.EndsWith(".css")) response.ContentType = "text/css";
                        else if (filename.EndsWith(".js")) response.ContentType = "application/javascript";
                        else if (filename.EndsWith(".html")) response.ContentType = "text/html";
                        else response.ContentType = "application/octet-stream";

                        // 讀取檔案並傳回給 WebView
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            response.ContentLength64 = fs.Length;
                            fs.CopyTo(response.OutputStream);
                        }
                    }
                    else
                    {
                        response.StatusCode = 404; // 找不到檔案
                    }
                    response.OutputStream.Close();

                }
            });

            // 3. 設定單一 WebView2
            try
            {
                webView21.Source = new Uri("http://localhost:8666/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("friend list Error: " + ex.Message);
            }
        }
    }
}
