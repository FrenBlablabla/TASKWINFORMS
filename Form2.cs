using System.Net.Http.Json;

namespace IsvuWinForms
{
    public class Form2 : Form
    {
        private ComboBox comboCourses;
        private ListBox listExams;
        private Label lblTitle, lblExams, lblDeadline;

        private ComboBox comboDeadlines;
        private Button btnRegister;

        private readonly HttpClient _httpClient;
        private readonly string _studentJmbag;
        private readonly string _apiBaseUrl;

        public Form2(HttpClient httpClient, string apiBaseUrl, string jmbag)
        {
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl;
            _studentJmbag = jmbag;

            SetupLayout();
            this.Load += Form2_Load;
        }

        private void SetupLayout()
        {
            this.Text = "Odabir ispita i Prijava";
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label { Text = "Enrolled Courses:", Location = new Point(30, 20), Width = 200 };
            comboCourses = new ComboBox { Location = new Point(30, 45), Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
            comboCourses.SelectedIndexChanged += ComboCourses_SelectedIndexChanged;

            lblExams = new Label { Text = "Exam Registrations:", Location = new Point(30, 90), Width = 200 };
            listExams = new ListBox { Location = new Point(30, 115), Width = 420, Height = 180 };

            lblDeadline = new Label { Text = "Select Exam Deadline (Rok ID):", Location = new Point(30, 315), Width = 200, Enabled = false };
            comboDeadlines = new ComboBox { Location = new Point(30, 340), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };

            btnRegister = new Button { Text = "Register for Exam", Location = new Point(250, 339), Width = 200, Height = 25, Enabled = false };
            btnRegister.Click += BtnRegister_Click;

            this.Controls.AddRange(new Control[] { lblTitle, comboCourses, lblExams, listExams, lblDeadline, comboDeadlines, btnRegister });
        }

        private async void Form2_Load(object? sender, EventArgs e)
        {
            await LoadCourses();
            await LoadDeadlines();
        }

        private async Task LoadCourses()
        {
            try
            {
                var courses = await _httpClient.GetFromJsonAsync<List<UpisPredmetaDto>>($"{_apiBaseUrl}courses/{_studentJmbag}");
                if (courses != null && courses.Count > 0)
                {
                    comboCourses.DataSource = courses;
                    comboCourses.DisplayMember = "PredmetSifra";
                    comboCourses.ValueMember = "Id";
                }
                else
                {
                    MessageBox.Show("This student has no enrolled courses.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading courses: {ex.Message}"); }
        }

        private async Task LoadDeadlines()
        {
            try
            {
                var deadlines = await _httpClient.GetFromJsonAsync<List<int>>($"{_apiBaseUrl}deadlines");
                if (deadlines != null)
                {
                    comboDeadlines.DataSource = deadlines;
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading deadlines: {ex.Message}"); }
        }

        private async void ComboCourses_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (comboCourses.SelectedItem is not UpisPredmetaDto selectedCourse) return;

            int selectedEnrollmentId = selectedCourse.Id;
            await RefreshExamList(selectedEnrollmentId);

            if (selectedCourse.Status?.Trim().Equals("Položen", StringComparison.OrdinalIgnoreCase) == true)
            {
                lblDeadline.Enabled = false;
                comboDeadlines.Enabled = false;
                btnRegister.Enabled = false;
                listExams.Items.Add("🔒 COURSE PASSED: New registrations are blocked.");
            }
            else
            {
                lblDeadline.Enabled = true;
                comboDeadlines.Enabled = true;
                btnRegister.Enabled = true;
            }
        }

        private async Task RefreshExamList(int enrollmentId)
        {
            listExams.Items.Clear();
            try
            {
                var exams = await _httpClient.GetFromJsonAsync<List<PrijavaIspitaDto>>($"{_apiBaseUrl}exams/{enrollmentId}");
                if (exams != null && exams.Count > 0)
                {
                    foreach (var exam in exams)
                    {
                        string dateStr = exam.DatumPrijave?.ToString("dd.MM.yyyy") ?? "No Date";
                        listExams.Items.Add($"Attempt #{exam.RedniBrojIzlaska} | Date: {dateStr} | Status: {exam.Status} (Rok ID: {exam.IspitniRokId})");
                    }
                }
                else
                {
                    listExams.Items.Add("No previous exam registrations found for this course.");
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error loading exams: {ex.Message}"); }
        }

        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (comboCourses.SelectedValue == null || comboDeadlines.SelectedItem == null) return;

            int selectedEnrollmentId = (int)comboCourses.SelectedValue;
            int selectedDeadlineId = (int)comboDeadlines.SelectedItem;

            var newRegistration = new PrijavaIspitaDto
            {
                UpisPredmetaId = selectedEnrollmentId,
                IspitniRokId = selectedDeadlineId
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}register-exam", newRegistration);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully registered for the exam! Row appended to database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await RefreshExamList(selectedEnrollmentId);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Registration failed: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Network processing error: {ex.Message}");
            }
        }
    }
}