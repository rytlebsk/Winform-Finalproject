using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace finalproject
{
    public partial class Form3: Form
    {
        private Form1 _parentForm;
        public Form3(Form1 parentForm)
        {
            InitializeComponent();
            _parentForm = parentForm;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            userInfo _userInfo = new userInfo();
            _userInfo = _parentForm.GetUserInfo();
            textBox1.Text = _userInfo.userName;
            textBox2.Text = _userInfo.userNumericId;
        }

        private void confirmBtn_Click(object sender, EventArgs e)
        {
            try
            {
                UserDetailsDto dataToSend = new UserDetailsDto
                {
                    userName = textBox1.Text,
                    userNumericId = textBox2.Text,

                    UserProfileImage = ImageToBytes(pictureBox1.Image)
                };
                Console.WriteLine("packed data: " + dataToSend.userName + ", " + dataToSend.userNumericId + ", " + (dataToSend.UserProfileImage != null ? dataToSend.UserProfileImage.Length.ToString() : "null"));

                string jsonString = JsonSerializer.Serialize(dataToSend);

                _parentForm.SendUserDataToWeb(jsonString);

                Console.WriteLine("packed data sent: " + jsonString);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("error: " + ex.Message);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void importImageBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
            }
        }
        public byte[] ImageToBytes(Image image)
        {
            if (image == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);

                return ms.ToArray();
            }
        }

    }

    public class UserDetailsDto
    {
        [JsonPropertyName("userName")]
        public string userName { get; set; }

        [JsonPropertyName("userNumericId")]
        public string userNumericId { get; set; }

        [JsonPropertyName("userProfileImage")]
        public byte[] UserProfileImage { get; set; }
    }
}
