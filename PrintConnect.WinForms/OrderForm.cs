using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace PrintConnect.WinForms
{
    public partial class OrderForm : Form
    {
        public OrderForm()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Gather data from the textboxes
                var orderData = new
                {
                    customerId = int.Parse(textBox1.Text), // Customer ID box
                    items = new[]
                    {
                        new
                        {
                            productId = int.Parse(textBox2.Text), // Product ID box
                            quantity = int.Parse(textBox3.Text)   // Quantity box
                        }
                    }
                };

                // 2. Send to the API
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:5214/");
                var response = await client.PostAsJsonAsync("tenant/1/orders", orderData);

                // 3. Show result
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Order placed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear boxes for the next order
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                }
                else
                {
                    MessageBox.Show("Error: " + response.StatusCode + "\nCheck if Customer/Product IDs exist.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numbers for ID and Quantity.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to API. Is the API running?\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OrderForm orderScreen = new OrderForm();
            orderScreen.Show(); // This opens the new form!
        }
    }
}