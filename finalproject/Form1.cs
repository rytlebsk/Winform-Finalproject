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

            var content = new FileStream("test.html", FileMode.Open, FileAccess.Read);
            Task.Run(() =>
            {
                while (true)
                {
                    var context = listener.GetContext();
                    var response = context.Response;
                    response.ContentLength64 = content.Length;
                    content.Position = 0;
                    content.CopyTo(response.OutputStream);
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
            string id = textBox1.Text;

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

        }
    }
}
