using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace PrintConnect.WinForms
{
    public partial class LoginForm : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public LoginForm()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://localhost:5214/");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Get the email and password from the textboxes
                var loginData = new
                {
                    email = textBox1.Text,    // Assuming textBox1 is Email
                    password = textBox2.Text  // Assuming textBox2 is Password
                };

                // 2. Send it to the API's login endpoint
                var response = await client.PostAsJsonAsync("login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Login Successful! Welcome to PrintConnect.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 3. Open the main dashboard (Form1) and hide the login screen
                    Form1 mainForm = new Form1();
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to API. Is it running?\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // This empty method fixes the error you just got!
        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}