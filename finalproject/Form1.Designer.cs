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
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.roomIdLabel = new System.Windows.Forms.Label();
            this.button12 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView23)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(394, 104);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(259, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(357, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "ID:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(245, 685);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(162, 92);
            this.button1.TabIndex = 3;
            this.button1.Text = "play";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(513, 685);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(162, 92);
            this.button2.TabIndex = 4;
            this.button2.Text = "pause";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(782, 685);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(162, 92);
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
            this.webView21.Enabled = false;
            this.webView21.Location = new System.Drawing.Point(285, 163);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(668, 487);
            this.webView21.TabIndex = 0;
            this.webView21.ZoomFactor = 1D;
            this.webView21.Click += new System.EventHandler(this.webView21_Click);
            // 
            // webView22
            // 
            this.webView22.AllowExternalDrop = true;
            this.webView22.CreationProperties = null;
            this.webView22.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView22.Location = new System.Drawing.Point(969, 163);
            this.webView22.Name = "webView22";
            this.webView22.Size = new System.Drawing.Size(436, 487);
            this.webView22.TabIndex = 7;
            this.webView22.ZoomFactor = 1D;
            this.webView22.Click += new System.EventHandler(this.webView22_Click);
            // 
            // webView23
            // 
            this.webView23.AllowExternalDrop = true;
            this.webView23.CreationProperties = null;
            this.webView23.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView23.Location = new System.Drawing.Point(13, 163);
            this.webView23.Name = "webView23";
            this.webView23.Size = new System.Drawing.Size(247, 487);
            this.webView23.TabIndex = 8;
            this.webView23.ZoomFactor = 1D;
            this.webView23.Click += new System.EventHandler(this.webView23_Click);
            // 
            // addFriend
            // 
            this.addFriend.Location = new System.Drawing.Point(804, 68);
            this.addFriend.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.addFriend.Name = "addFriend";
            this.addFriend.Size = new System.Drawing.Size(88, 58);
            this.addFriend.TabIndex = 9;
            this.addFriend.Text = "添加好友";
            this.addFriend.UseVisualStyleBackColor = true;
            this.addFriend.Click += new System.EventHandler(this.button4_Click);
            // 
            // friendlist
            // 
            this.friendlist.Location = new System.Drawing.Point(897, 68);
            this.friendlist.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.friendlist.Name = "friendlist";
            this.friendlist.Size = new System.Drawing.Size(88, 58);
            this.friendlist.TabIndex = 10;
            this.friendlist.Text = "好友清單";
            this.friendlist.UseVisualStyleBackColor = true;
            this.friendlist.Click += new System.EventHandler(this.friendlist_Click);
            // 
            // joinroom
            // 
            this.joinroom.Location = new System.Drawing.Point(990, 68);
            this.joinroom.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.joinroom.Name = "joinroom";
            this.joinroom.Size = new System.Drawing.Size(88, 58);
            this.joinroom.TabIndex = 11;
            this.joinroom.Text = "加入房間";
            this.joinroom.UseVisualStyleBackColor = true;
            this.joinroom.Click += new System.EventHandler(this.joinroom_Click);
            // 
            // findvideo
            // 
            this.findvideo.Location = new System.Drawing.Point(1083, 68);
            this.findvideo.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.findvideo.Name = "findvideo";
            this.findvideo.Size = new System.Drawing.Size(88, 58);
            this.findvideo.TabIndex = 12;
            this.findvideo.Text = "搜尋影片";
            this.findvideo.UseVisualStyleBackColor = true;
            this.findvideo.Click += new System.EventHandler(this.findvideo_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(437, 702);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(56, 59);
            this.button4.TabIndex = 13;
            this.button4.Text = "-10s";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(702, 702);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(56, 59);
            this.button5.TabIndex = 14;
            this.button5.Text = "+10s";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(680, 93);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(94, 41);
            this.button6.TabIndex = 15;
            this.button6.Text = "加入影片";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(1050, 702);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(88, 49);
            this.button7.TabIndex = 16;
            this.button7.Text = "+10%";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(1164, 702);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(90, 49);
            this.button8.TabIndex = 17;
            this.button8.Text = "-10%";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(1284, 702);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(90, 49);
            this.button9.TabIndex = 18;
            this.button9.Text = "mute";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(1079, 664);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(274, 24);
            this.label2.TabIndex = 19;
            this.label2.Text = "Volume Control(Client side)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(1284, 763);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(90, 49);
            this.button10.TabIndex = 20;
            this.button10.Text = "unmute";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(1175, 68);
            this.button11.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(88, 58);
            this.button11.TabIndex = 21;
            this.button11.Text = "離開房間";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // roomIdLabel
            // 
            this.roomIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roomIdLabel.Location = new System.Drawing.Point(12, 35);
            this.roomIdLabel.Name = "roomIdLabel";
            this.roomIdLabel.Size = new System.Drawing.Size(507, 27);
            this.roomIdLabel.TabIndex = 22;
            this.roomIdLabel.Text = "Room ID:";
            this.roomIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(12, 65);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(91, 27);
            this.button12.TabIndex = 23;
            this.button12.Text = "複製Room ID";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1432, 824);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.roomIdLabel);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
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
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
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
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Label roomIdLabel;
        private System.Windows.Forms.Button button12;
    }
}

