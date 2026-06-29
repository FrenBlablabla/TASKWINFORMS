using System.Net.Http.Json;

namespace IsvuWinForms
{
    public partial class Form1 : Form
    {
        private ComboBox comboBoxOib;
        private Label lblPrompt;
        private DateTimePicker dtpBirthDate;
        private Button btnVerify;

        private Label[] labels = new Label[7];
        private TextBox txtJmbag, txtIme, txtPrezime, txtEmail, txtStudijskiProgramId;
        private DateTimePicker dtpEnrollmentDate;
        private ComboBox comboStatus;
        private Button btnSave;

        private HttpClient? _httpClient;
        private StudentDto? _currentStudent;

        private const string ApiBaseUrl = "https://127.0.0.1:7074/api/Student/";

        public Form1()
        {
            InitializeComponent();
            SetupFormLayout();

            this.Shown += Form1_Shown;
        }

        private void InitializeNetwork()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        private async void Form1_Shown(object? sender, EventArgs e)
        {
            InitializeNetwork();

            await LoadOibDropdown();
        }

        private async Task LoadOibDropdown()
        {
            try
            {
                if (_httpClient == null) return;

                var oibs = await _httpClient.GetFromJsonAsync<List<string>>($"{ApiBaseUrl}oib");

                if (oibs != null)
                {
                    comboBoxOib.Items.Clear();
                    comboBoxOib.Items.Add("Select OIB");
                    foreach (var oib in oibs) comboBoxOib.Items.Add(oib);
                    comboBoxOib.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $"\nDetails: {ex.InnerException.Message}" : "";
                MessageBox.Show($"Error connecting to API: {ex.Message}{innerMessage}\n\nTarget URL tried: {ApiBaseUrl}oib",
                                "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupFormLayout()
        {
            this.Text = "Edit Student Information";
            this.Size = new Size(450, 560);
            this.StartPosition = FormStartPosition.CenterScreen;

            comboBoxOib = new ComboBox { Location = new Point(30, 30), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            comboBoxOib.SelectedIndexChanged += ComboBoxOib_SelectedIndexChanged;
            this.Controls.Add(comboBoxOib);

            lblPrompt = new Label { Text = "Enter Date of Birth to unlock details:", Location = new Point(30, 80), Size = new Size(300, 20), Visible = false };
            dtpBirthDate = new DateTimePicker { Location = new Point(30, 105), Width = 200, Format = DateTimePickerFormat.Short, Visible = false };
            btnVerify = new Button { Text = "Verify Date", Location = new Point(240, 104), Width = 100, Visible = false };
            btnVerify.Click += BtnVerify_Click;

            this.Controls.AddRange(new Control[] { lblPrompt, dtpBirthDate, btnVerify });

            string[] fieldNames = { "JMBAG:", "Ime:", "Prezime:", "Email:", "Program ID:", "Datum Upisa:", "Status:" };
            int startY = 160;

            for (int i = 0; i < fieldNames.Length; i++)
            {
                labels[i] = new Label { Text = fieldNames[i], Location = new Point(30, startY + (i * 40)), Width = 100, Visible = false };
                this.Controls.Add(labels[i]);
            }

            txtJmbag = new TextBox { Location = new Point(140, startY), Width = 200, Visible = false };
            txtIme = new TextBox { Location = new Point(140, startY + 40), Width = 200, Visible = false };
            txtPrezime = new TextBox { Location = new Point(140, startY + 80), Width = 200, Visible = false };
            txtEmail = new TextBox { Location = new Point(140, startY + 120), Width = 200, Visible = false };
            dtpEnrollmentDate = new DateTimePicker { Location = new Point(140, startY + 160), Width = 200, Format = DateTimePickerFormat.Short, Visible = false };

            comboStatus = new ComboBox { Location = new Point(140, startY + 200), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            comboStatus.Items.AddRange(new string[] { "Redoviti", "Izvanredni", "Završen", "Ispisan" });
            txtStudijskiProgramId = new TextBox { Location = new Point(140, startY + 240), Width = 200, Visible = false, ReadOnly = true, BackColor = SystemColors.Control };
            btnSave = new Button { Text = "Save Changes", Location = new Point(140, startY + 290), Width = 120, Height = 35, Visible = false };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { txtJmbag, txtIme, txtPrezime, txtEmail, txtStudijskiProgramId, dtpEnrollmentDate, comboStatus, btnSave });
        }

        private async void ComboBoxOib_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ToggleEditUI(false);
            ToggleVerificationUI(false);
            _currentStudent = null;

            if (comboBoxOib.SelectedIndex <= 0 || _httpClient == null) return;

            string selectedOib = comboBoxOib.SelectedItem.ToString()!;
            try
            {
                _currentStudent = await _httpClient.GetFromJsonAsync<StudentDto>($"{ApiBaseUrl}details/{selectedOib}");
                if (_currentStudent != null)
                {
                    ToggleVerificationUI(true);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading data: {ex.Message}"); }
        }

        private void BtnVerify_Click(object? sender, EventArgs e)
        {
            if (_currentStudent == null) return;

            if (_currentStudent.DatumRodenja?.Date == dtpBirthDate.Value.Date)
            {
                txtJmbag.Text = _currentStudent.Jmbag;
                txtIme.Text = _currentStudent.Ime;
                txtPrezime.Text = _currentStudent.Prezime;
                txtEmail.Text = _currentStudent.Email;
                dtpEnrollmentDate.Value = _currentStudent.DatumUpisa ?? DateTime.Now;
                comboStatus.SelectedItem = comboStatus.Items.Contains(_currentStudent.Status ?? "") ? _currentStudent.Status : null;
                txtStudijskiProgramId.Text = _currentStudent.StudijskiProgramId?.ToString() ?? "";

                ToggleVerificationUI(false);
                ToggleEditUI(true);
            }
            else
            {
                MessageBox.Show("Incorrect Date of Birth! Access Denied.", "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_currentStudent == null || comboBoxOib.SelectedIndex <= 0 || _httpClient == null) return;

            var updatedData = new StudentDto
            {
                Jmbag = txtJmbag.Text,
                Oib = comboBoxOib.SelectedItem.ToString(),
                Ime = txtIme.Text,
                Prezime = txtPrezime.Text,
                Email = txtEmail.Text,
                DatumUpisa = dtpEnrollmentDate.Value,
                Status = comboStatus.SelectedItem?.ToString()
            };

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}update/{updatedData.Oib}", updatedData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Student details saved directly to SQL Database!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string targetJmbag = txtJmbag.Text;
                    ToggleEditUI(false);
                    comboBoxOib.SelectedIndex = 0;
                    Form2 examForm = new Form2(_httpClient!, ApiBaseUrl, targetJmbag);
                    this.Hide();

                    examForm.ShowDialog(); 

                    this.Show();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to update database: {errorMsg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Network error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleVerificationUI(bool visible)
        {
            lblPrompt.Visible = visible;
            dtpBirthDate.Visible = visible;
            btnVerify.Visible = visible;
        }

        private void ToggleEditUI(bool visible)
        {
            txtJmbag.Visible = visible;
            txtIme.Visible = visible;
            txtPrezime.Visible = visible;
            txtEmail.Visible = visible;
            dtpEnrollmentDate.Visible = visible;
            comboStatus.Visible = visible;
            txtStudijskiProgramId.Visible = visible;
            btnSave.Visible = visible;
        }
    }
}