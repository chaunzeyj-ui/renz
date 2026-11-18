using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace PrintConnect.WinForms
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://localhost:5214/");
        }

        // 1. Submit Complaint / Inquiry Button
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var inquiryData = new
                {
                    customerName = textBox1.Text,
                    emailAddress = textBox2.Text,
                    subject = textBox3.Text,
                    message = textBox4.Text,
                    type = comboBox1.Text,
                    status = "Open"
                };

                var response = await client.PostAsJsonAsync("tenant/1/inquiries", inquiryData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Submitted successfully to the Cloud!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear(); textBox2.Clear(); textBox3.Clear(); textBox4.Clear();
                    comboBox1.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Error: " + response.StatusCode, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to API. Is the API running?\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 2. Go to Cashier Button
        private void button2_Click(object sender, EventArgs e)
        {
            OrderForm orderScreen = new OrderForm();
            orderScreen.Show();
        }

        // 3. View Complaints Button
        private void button3_Click(object sender, EventArgs e)
        {
            ViewComplaintsForm complaintsScreen = new ViewComplaintsForm();
            complaintsScreen.Show();
        }

        // This empty method fixes a Visual Studio designer bug
        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}