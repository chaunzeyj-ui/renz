using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace PrintConnect.WinForms
{
    public partial class ViewComplaintsForm : Form
    {
        private static readonly HttpClient client = new HttpClient();
        private List<InquiryData> _allInquiries = new List<InquiryData>(); // Hold all data in memory

        public ViewComplaintsForm()
        {
            InitializeComponent();
            client.BaseAddress = new Uri("http://localhost:5214/");

            // Set default filter to "All" when form loads
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }

            // Automatically load the data when the form opens
            this.Shown += async (s, e) => await LoadData();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var response = await client.GetAsync("tenant/1/inquiries");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _allInquiries = JsonSerializer.Deserialize<List<InquiryData>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<InquiryData>();

                    ApplyFilter(); // Display the data
                }
                else
                {
                    MessageBox.Show("Failed to load data from API.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to API. Is it running?\n" + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // This runs when you change the ComboBox dropdown
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string selectedType = comboBox1.Text;

            if (selectedType == "All" || string.IsNullOrEmpty(selectedType))
            {
                dataGridView1.DataSource = _allInquiries;
            }
            else
            {
                // Filter the list based on the dropdown selection
                var filteredList = _allInquiries
                    .Where(x => x.Type.Equals(selectedType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                dataGridView1.DataSource = filteredList;
            }
        }

        // --- This empty method fixes the Visual Studio Designer error ---
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Do nothing when a cell is clicked
        }

        public class InquiryData
        {
            public int InquiryId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}