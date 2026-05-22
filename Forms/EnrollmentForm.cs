using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class EnrollmentForm : Form
    {
        private DataGridView dgv;
        private ComboBox cmbStudent, cmbCourse;
        private DateTimePicker dtpDate;
        private Button btnEnroll, btnClear, btnRefresh;
        private Label lblStatus, lblError;

        public EnrollmentForm()
        {
            InitializeComponents();
            LoadComboBoxes();
            LoadEnrollments();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Student Enrollment";
            this.Size = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(850, 580);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);

            // ── Top bar ──
            var topBar = new Panel { Size = new Size(1000, 60), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            topBar.Controls.Add(new Label { Text = "📋  Student Enrollment Transaction", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 16), BackColor = Color.Transparent });
            var btnClose = new Button { Text = "✕", Size = new Size(40, 40), Location = new Point(946, 10), Font = new Font("Segoe UI", 12f), ForeColor = Color.FromArgb(180, 200, 225), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            // ── Entry panel ──
            var entry = new Panel { Location = new Point(0, 60), Size = new Size(1000, 115), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            entry.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 114, entry.Width, 114);

            FLbl(entry, "STUDENT", 20, 15);
            cmbStudent = new ComboBox { Location = new Point(20, 35), Size = new Size(270, 28), Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(cmbStudent);

            FLbl(entry, "COURSE", 310, 15);
            cmbCourse = new ComboBox { Location = new Point(310, 35), Size = new Size(320, 28), Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(cmbCourse);

            FLbl(entry, "ENROLLMENT DATE", 650, 15);
            dtpDate = new DateTimePicker { Location = new Point(650, 35), Size = new Size(155, 28), Font = new Font("Segoe UI", 9.5f), Value = DateTime.Today };
            entry.Controls.Add(dtpDate);

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(192, 57, 43), AutoSize = true, Location = new Point(20, 80), BackColor = Color.Transparent };
            entry.Controls.Add(lblError);

            btnEnroll = Btn("✔ Enroll Student", Color.FromArgb(39, 174, 96), 150, 34);
            btnEnroll.Location = new Point(650, 70);
            btnEnroll.Click += BtnEnroll_Click;
            entry.Controls.Add(btnEnroll);

            btnClear = Btn("✕ Clear", Color.FromArgb(150, 165, 185), 80, 34);
            btnClear.Location = new Point(810, 70);
            btnClear.Click += (s, e) => { cmbStudent.SelectedIndex = -1; cmbCourse.SelectedIndex = -1; dtpDate.Value = DateTime.Today; lblError.Text = ""; };
            entry.Controls.Add(btnClear);

            // ── Grid header ──
            var gh = new Panel { Location = new Point(15, 125), Size = new Size(970, 44), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            gh.Controls.Add(new Label { Text = "Enrollment Records", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(0, 12), BackColor = Color.Transparent });
            btnRefresh = Btn("↺ Refresh", Color.FromArgb(100, 120, 150), 90, 30);
            btnRefresh.Location = new Point(875, 7);
            btnRefresh.Click += (s, e) => LoadEnrollments();
            gh.Controls.Add(btnRefresh);

            // ── DataGridView ──
            dgv = new DataGridView
            {
                Location = new Point(15, 172),
                Size = new Size(970, 455),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f),
                GridColor = Color.FromArgb(230, 235, 245),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            ApplyGridStyle(dgv);

            lblStatus = new Label { Text = "Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 140, 165), AutoSize = true, Location = new Point(20, 640), BackColor = Color.Transparent, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            this.Controls.AddRange(new Control[] { topBar, entry, gh, dgv, lblStatus });
        }

        internal static void ApplyGridStyle(DataGridView g)
        {
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 100, 130);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 36;
            g.EnableHeadersVisualStyles = false;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 254);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 255);
            g.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 40, 70);
        }

        internal static void FLbl(Panel p, string t, int x, int y) =>
            p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 115, 135), AutoSize = true, Location = new Point(x, y), BackColor = Color.Transparent });

        internal static Button Btn(string text, Color color, int w, int h)
        {
            var b = new Button { Text = text, Size = new Size(w, h), Font = new Font("Segoe UI", 9f), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    cmbStudent.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT student_id, CONCAT(first_name,' ',last_name) FROM students ORDER BY last_name, first_name", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) cmbStudent.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));

                    cmbCourse.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT course_id, course_name FROM courses ORDER BY course_name", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) cmbCourse.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
                }
            }
            catch (Exception ex) { lblError.Text = "Error loading data: " + ex.Message; }
        }

        public void LoadEnrollments()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand(@"
                    SELECT e.enrollment_id AS ID,
                           CONCAT(s.first_name,' ',s.last_name) AS Student,
                           c.course_name AS Course,
                           IFNULL(d.department_name,'-') AS Department,
                           e.enrollment_date AS Date,
                           IFNULL(e.grade,'-') AS Grade
                    FROM enrollments e
                    JOIN students s ON e.student_id = s.student_id
                    JOIN courses c ON e.course_id = c.course_id
                    LEFT JOIN departments d ON c.department_id = d.department_id
                    ORDER BY e.enrollment_id DESC", conn))
                {
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    dgv.DataSource = dt;
                    if (dgv.Columns["ID"] != null) dgv.Columns["ID"].Visible = false;
                    lblStatus.Text = $"{dt.Rows.Count} enrollment(s) loaded.";
                }
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void BtnEnroll_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (cmbStudent.SelectedIndex < 0 || cmbCourse.SelectedIndex < 0)
            { lblError.Text = "Please select a student and a course."; return; }

            int studentId = ((ComboItem)cmbStudent.SelectedItem).Id;
            int courseId  = ((ComboItem)cmbCourse.SelectedItem).Id;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    using (var chk = new MySqlCommand("SELECT COUNT(*) FROM enrollments WHERE student_id=@s AND course_id=@c", conn))
                    {
                        chk.Parameters.AddWithValue("@s", studentId);
                        chk.Parameters.AddWithValue("@c", courseId);
                        if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                        { lblError.Text = "Student is already enrolled in this course."; return; }
                    }
                    using (var cmd = new MySqlCommand("INSERT INTO enrollments (student_id, course_id, enrollment_date) VALUES (@s,@c,@d)", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", studentId);
                        cmd.Parameters.AddWithValue("@c", courseId);
                        cmd.Parameters.AddWithValue("@d", dtpDate.Value.Date);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Student enrolled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStudent.SelectedIndex = -1;
                cmbCourse.SelectedIndex = -1;
                LoadEnrollments();
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; }
        }
    }

    // Shared helper for ComboBoxes that need an ID alongside display text
    public class ComboItem
    {
        public int Id { get; }
        private readonly string name;
        public ComboItem(int id, string name) { Id = id; this.name = name; }
        public override string ToString() => name;
    }
}
