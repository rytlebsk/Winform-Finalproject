using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            var content = new FileStream("videoQueue.html", FileMode.Open, FileAccess.Read);
            Task.Run(() =>
            {
                while (true)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    // 取得請求的檔案名稱
                    string filename = request.Url.AbsolutePath.TrimStart('/');
                    if (string.IsNullOrEmpty(filename))
                        filename = "videoQueue.html"; // 預設頁面

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
                webView21.Source = new Uri("http://localhost:8080/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to web page: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (videoId != null) webView21.ExecuteScriptAsync("playerControl('pause')");
        }

        private void button3_Click(object sender, EventArgs e)
        {
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
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    videolist.Items.Add(textBox1.Text);
                    textBox1.Clear();
                }
            }
        }
    }
}
