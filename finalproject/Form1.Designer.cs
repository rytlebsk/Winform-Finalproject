namespace finalproject
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.webView22 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.webView23 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.addFriend = new System.Windows.Forms.Button();
            this.friendlist = new System.Windows.Forms.Button();
            this.joinroom = new System.Windows.Forms.Button();
            this.findvideo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView23)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(628, 120);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(344, 25);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(579, 123);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "ID:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(355, 790);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(216, 106);
            this.button1.TabIndex = 3;
            this.button1.Text = "play";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(684, 790);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(216, 106);
            this.button2.TabIndex = 4;
            this.button2.Text = "pause";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1003, 790);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(216, 106);
            this.button3.TabIndex = 5;
            this.button3.Text = "next";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // webView21
            // 
            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Location = new System.Drawing.Point(355, 188);
            this.webView21.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(891, 562);
            this.webView21.TabIndex = 0;
            this.webView21.ZoomFactor = 1D;
            this.webView21.Click += new System.EventHandler(this.webView21_Click);
            // 
            // webView22
            // 
            this.webView22.AllowExternalDrop = true;
            this.webView22.CreationProperties = null;
            this.webView22.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView22.Location = new System.Drawing.Point(1253, 188);
            this.webView22.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.webView22.Name = "webView22";
            this.webView22.Size = new System.Drawing.Size(309, 562);
            this.webView22.TabIndex = 7;
            this.webView22.ZoomFactor = 1D;
            this.webView22.Click += new System.EventHandler(this.webView22_Click);
            // 
            // webView23
            // 
            this.webView23.AllowExternalDrop = true;
            this.webView23.CreationProperties = null;
            this.webView23.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView23.Location = new System.Drawing.Point(17, 188);
            this.webView23.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.webView23.Name = "webView23";
            this.webView23.Size = new System.Drawing.Size(329, 562);
            this.webView23.TabIndex = 8;
            this.webView23.ZoomFactor = 1D;
            // 
            // addFriend
            // 
            this.addFriend.Location = new System.Drawing.Point(1072, 78);
            this.addFriend.Name = "addFriend";
            this.addFriend.Size = new System.Drawing.Size(118, 67);
            this.addFriend.TabIndex = 9;
            this.addFriend.Text = "添加好友";
            this.addFriend.UseVisualStyleBackColor = true;
            this.addFriend.Click += new System.EventHandler(this.button4_Click);
            // 
            // friendlist
            // 
            this.friendlist.Location = new System.Drawing.Point(1196, 78);
            this.friendlist.Name = "friendlist";
            this.friendlist.Size = new System.Drawing.Size(118, 67);
            this.friendlist.TabIndex = 10;
            this.friendlist.Text = "好友清單";
            this.friendlist.UseVisualStyleBackColor = true;
            this.friendlist.Click += new System.EventHandler(this.friendlist_Click);
            // 
            // joinroom
            // 
            this.joinroom.Location = new System.Drawing.Point(1320, 78);
            this.joinroom.Name = "joinroom";
            this.joinroom.Size = new System.Drawing.Size(118, 67);
            this.joinroom.TabIndex = 11;
            this.joinroom.Text = "加入房間";
            this.joinroom.UseVisualStyleBackColor = true;
            this.joinroom.Click += new System.EventHandler(this.joinroom_Click);
            // 
            // findvideo
            // 
            this.findvideo.Location = new System.Drawing.Point(1444, 78);
            this.findvideo.Name = "findvideo";
            this.findvideo.Size = new System.Drawing.Size(118, 67);
            this.findvideo.TabIndex = 12;
            this.findvideo.Text = "搜尋影片";
            this.findvideo.UseVisualStyleBackColor = true;
            this.findvideo.Click += new System.EventHandler(this.findvideo_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1579, 951);
            this.Controls.Add(this.findvideo);
            this.Controls.Add(this.joinroom);
            this.Controls.Add(this.friendlist);
            this.Controls.Add(this.addFriend);
            this.Controls.Add(this.webView23);
            this.Controls.Add(this.webView22);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.webView21);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView23)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView22;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView23;
        private System.Windows.Forms.Button addFriend;
        private System.Windows.Forms.Button friendlist;
        private System.Windows.Forms.Button joinroom;
        private System.Windows.Forms.Button findvideo;
    }
}

