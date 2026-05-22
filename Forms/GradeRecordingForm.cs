using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class GradeRecordingForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch, txtGrade;
        private Button btnSearch, btnRefresh, btnSaveGrade;
        private Label lblStatus, lblSelected;

        public GradeRecordingForm()
        {
            InitializeComponents();
            LoadEnrollments();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Grade Recording";
            this.Size = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(850, 580);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);

            // ── Top bar ──
            var topBar = new Panel { Size = new Size(1000, 60), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            topBar.Controls.Add(new Label { Text = "🎓  Grade Recording Transaction", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 16), BackColor = Color.Transparent });
            var btnClose = new Button { Text = "✕", Size = new Size(40, 40), Location = new Point(946, 10), Font = new Font("Segoe UI", 12f), ForeColor = Color.FromArgb(180, 200, 225), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            // ── Search toolbar ──
            var toolbar = new Panel { Location = new Point(0, 60), Size = new Size(1000, 55), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 54, toolbar.Width, 54);

            txtSearch = new TextBox { Location = new Point(20, 14), Size = new Size(250, 28), Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252) };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadEnrollments(txtSearch.Text.Trim()); };

            btnSearch = EnrollmentForm.Btn("🔍 Search", Color.FromArgb(52, 152, 219), 90, 28);
            btnSearch.Location = new Point(278, 14);
            btnSearch.Click += (s, e) => LoadEnrollments(txtSearch.Text.Trim());

            btnRefresh = EnrollmentForm.Btn("↺ Refresh", Color.FromArgb(100, 120, 150), 90, 28);
            btnRefresh.Location = new Point(376, 14);
            btnRefresh.Click += (s, e) => { txtSearch.Text = ""; LoadEnrollments(); };

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh });

            // ── Grade entry bar ──
            var gradeBar = new Panel { Location = new Point(0, 115), Size = new Size(1000, 58), BackColor = Color.FromArgb(248, 250, 253), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            gradeBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 0, gradeBar.Width, 0);
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 57, gradeBar.Width, 57);
            };

            lblSelected = new Label { Text = "Select a row, then type the grade below and click Save Grade.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 120, 150), AutoSize = true, Location = new Point(20, 6), BackColor = Color.Transparent };
            gradeBar.Controls.Add(lblSelected);

            gradeBar.Controls.Add(new Label { Text = "GRADE (e.g. A, B+, C-):", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 100, 130), AutoSize = true, Location = new Point(20, 32), BackColor = Color.Transparent });
            txtGrade = new TextBox { Location = new Point(170, 28), Size = new Size(70, 24), Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252), MaxLength = 2 };
            gradeBar.Controls.Add(txtGrade);

            btnSaveGrade = EnrollmentForm.Btn("✔ Save Grade", Color.FromArgb(39, 174, 96), 120, 28);
            btnSaveGrade.Location = new Point(252, 25);
            btnSaveGrade.Click += BtnSaveGrade_Click;
            gradeBar.Controls.Add(btnSaveGrade);

            // ── DataGridView ──
            dgv = new DataGridView
            {
                Location = new Point(15, 188),
                Size = new Size(970, 440),
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
            EnrollmentForm.ApplyGridStyle(dgv);
            dgv.SelectionChanged += Dgv_SelectionChanged;

            lblStatus = new Label { Text = "Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 140, 165), AutoSize = true, Location = new Point(20, 643), BackColor = Color.Transparent, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            this.Controls.AddRange(new Control[] { topBar, toolbar, gradeBar, dgv, lblStatus });
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var row = dgv.SelectedRows[0];
            string g = row.Cells["Grade"].Value?.ToString() ?? "";
            txtGrade.Text = g == "-" ? "" : g;
            string student = row.Cells["Student"].Value?.ToString() ?? "";
            string course  = row.Cells["Course"].Value?.ToString() ?? "";
            lblSelected.Text = $"Selected: {student}  —  {course}";
        }

        private void LoadEnrollments(string search = "")
        {
            try
            {
                string sql = @"SELECT e.enrollment_id AS ID,
                                      CONCAT(s.first_name,' ',s.last_name) AS Student,
                                      c.course_name AS Course,
                                      IFNULL(d.department_name,'-') AS Department,
                                      e.enrollment_date AS Date,
                                      IFNULL(e.grade,'-') AS Grade
                               FROM enrollments e
                               JOIN students s ON e.student_id = s.student_id
                               JOIN courses c ON e.course_id = c.course_id
                               LEFT JOIN departments d ON c.department_id = d.department_id";
                if (!string.IsNullOrEmpty(search))
                    sql += " WHERE s.first_name LIKE @s OR s.last_name LIKE @s OR c.course_name LIKE @s";
                sql += " ORDER BY e.enrollment_id DESC";

                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(search))
                        cmd.Parameters.AddWithValue("@s", $"%{search}%");
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    dgv.DataSource = dt;
                    if (dgv.Columns["ID"] != null) dgv.Columns["ID"].Visible = false;
                    lblStatus.Text = $"{dt.Rows.Count} enrollment(s) found.";
                }
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void BtnSaveGrade_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            { MessageBox.Show("Please select an enrollment row first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string grade = txtGrade.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(grade))
            { MessageBox.Show("Please enter a grade.", "Grade Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (grade.Length > 2)
            { MessageBox.Show("Grade must be at most 2 characters (e.g. A, B+, C-).", "Invalid Grade", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int enrollmentId = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("UPDATE enrollments SET grade=@g WHERE enrollment_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@g", grade);
                    cmd.Parameters.AddWithValue("@id", enrollmentId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Grade saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtGrade.Text = "";
                LoadEnrollments(txtSearch.Text.Trim());
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
