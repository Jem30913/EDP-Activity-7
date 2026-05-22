using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class InstructorManagementForm : Form
    {
        private DataGridView dgv;
        private TextBox txtFirstName, txtLastName, txtEmail;
        private ComboBox cmbDepartment;
        private Button btnSave, btnCancelEdit, btnRefresh;
        private Label lblStatus, lblError, lblFormMode;
        private int editingId = 0;

        public InstructorManagementForm()
        {
            InitializeComponents();
            LoadDepartments();
            LoadInstructors();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Instructor Management";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(850, 600);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);

            // ── Top bar ──
            var topBar = new Panel { Size = new Size(1000, 60), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            topBar.Controls.Add(new Label { Text = "🎓  Instructor Management", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 16), BackColor = Color.Transparent });
            var btnClose = new Button { Text = "✕", Size = new Size(40, 40), Location = new Point(946, 10), Font = new Font("Segoe UI", 12f), ForeColor = Color.FromArgb(180, 200, 225), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            // ── Entry panel ──
            var entry = new Panel { Location = new Point(0, 60), Size = new Size(1000, 145), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            entry.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 144, entry.Width, 144);

            lblFormMode = new Label { Text = "REGISTER NEW INSTRUCTOR", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 52, 96), AutoSize = true, Location = new Point(20, 12), BackColor = Color.Transparent };
            entry.Controls.Add(lblFormMode);

            EnrollmentForm.FLbl(entry, "FIRST NAME", 20, 32);
            txtFirstName = new TextBox { Location = new Point(20, 52), Size = new Size(180, 28), Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(txtFirstName);

            EnrollmentForm.FLbl(entry, "LAST NAME", 215, 32);
            txtLastName = new TextBox { Location = new Point(215, 52), Size = new Size(180, 28), Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(txtLastName);

            EnrollmentForm.FLbl(entry, "EMAIL", 410, 32);
            txtEmail = new TextBox { Location = new Point(410, 52), Size = new Size(230, 28), Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(txtEmail);

            EnrollmentForm.FLbl(entry, "DEPARTMENT", 660, 32);
            cmbDepartment = new ComboBox { Location = new Point(660, 52), Size = new Size(220, 28), Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(cmbDepartment);

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(192, 57, 43), AutoSize = true, Location = new Point(20, 96), BackColor = Color.Transparent };
            entry.Controls.Add(lblError);

            btnSave = EnrollmentForm.Btn("✔ Save Instructor", Color.FromArgb(39, 174, 96), 150, 34);
            btnSave.Location = new Point(660, 98);
            btnSave.Click += BtnSave_Click;
            entry.Controls.Add(btnSave);

            btnCancelEdit = EnrollmentForm.Btn("✕ Cancel Edit", Color.FromArgb(150, 165, 185), 115, 34);
            btnCancelEdit.Location = new Point(820, 98);
            btnCancelEdit.Visible = false;
            btnCancelEdit.Click += (s, e) => ClearForm();
            entry.Controls.Add(btnCancelEdit);

            // ── Grid header ──
            var gh = new Panel { Location = new Point(15, 215), Size = new Size(970, 44), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            gh.Controls.Add(new Label { Text = "Instructor Records", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(0, 12), BackColor = Color.Transparent });
            btnRefresh = EnrollmentForm.Btn("↺ Refresh", Color.FromArgb(100, 120, 150), 90, 30);
            btnRefresh.Location = new Point(875, 7);
            btnRefresh.Click += (s, e) => { ClearForm(); LoadInstructors(); };
            gh.Controls.Add(btnRefresh);

            // ── DataGridView ──
            dgv = new DataGridView
            {
                Location = new Point(15, 262),
                Size = new Size(970, 385),
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
            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick    += Dgv_CellClick;

            lblStatus = new Label { Text = "Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 140, 165), AutoSize = true, Location = new Point(20, 660), BackColor = Color.Transparent, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            this.Controls.AddRange(new Control[] { topBar, entry, gh, dgv, lblStatus });
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            Color bg = col == "Edit"   ? Color.FromArgb(52, 152, 219)  :
                       col == "Delete" ? Color.FromArgb(231, 76, 60)    : Color.Empty;
            if (bg == Color.Empty) return;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.CellBounds);
            using (var sf = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center })
            using (var f = new Font("Segoe UI", 8.5f))
                e.Graphics.DrawString(e.Value?.ToString() ?? "", f, Brushes.White, e.CellBounds, sf);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ID"].Value);
            if (col == "Edit")   PopulateEditForm(id);
            else if (col == "Delete") DeleteInstructor(id);
        }

        private void LoadDepartments()
        {
            cmbDepartment.Items.Clear();
            cmbDepartment.Items.Add(new ComboItem(0, "(No Department)"));
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("SELECT department_id, department_name FROM departments ORDER BY department_name", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) cmbDepartment.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
            }
            catch (Exception ex) { lblError.Text = "Error loading departments: " + ex.Message; }
            cmbDepartment.SelectedIndex = 0;
        }

        public void LoadInstructors()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand(@"
                    SELECT i.instructor_id AS ID,
                           i.first_name AS 'First Name',
                           i.last_name AS 'Last Name',
                           IFNULL(i.email, '-') AS Email,
                           IFNULL(d.department_name, '-') AS Department,
                           COUNT(c.course_id) AS Courses
                    FROM instructors i
                    LEFT JOIN departments d ON i.department_id = d.department_id
                    LEFT JOIN courses c ON c.instructor_id = i.instructor_id
                    GROUP BY i.instructor_id, i.first_name, i.last_name, i.email, d.department_name
                    ORDER BY i.last_name, i.first_name", conn))
                {
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    dgv.DataSource = dt;
                    if (dgv.Columns["ID"] != null) dgv.Columns["ID"].Visible = false;

                    if (dgv.Columns["Edit"] == null)
                    {
                        dgv.Columns.Add(new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "✏ Edit", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 80 });
                        dgv.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "🗑 Delete", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 90 });
                    }
                    lblStatus.Text = $"{dt.Rows.Count} instructor(s) found.";
                }
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void PopulateEditForm(int instructorId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("SELECT first_name, last_name, email, department_id FROM instructors WHERE instructor_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", instructorId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        txtFirstName.Text = r["first_name"].ToString();
                        txtLastName.Text  = r["last_name"].ToString();
                        txtEmail.Text     = r["email"].ToString();
                        int deptId = r.IsDBNull(r.GetOrdinal("department_id")) ? 0 : r.GetInt32("department_id");
                        for (int i = 0; i < cmbDepartment.Items.Count; i++)
                            if (((ComboItem)cmbDepartment.Items[i]).Id == deptId) { cmbDepartment.SelectedIndex = i; break; }
                    }
                }
                editingId = instructorId;
                lblFormMode.Text = "EDIT INSTRUCTOR";
                btnSave.Text = "✔ Update Instructor";
                btnCancelEdit.Visible = true;
                lblError.Text = "";
            }
            catch (Exception ex) { lblError.Text = "Error loading instructor: " + ex.Message; }
        }

        private void ClearForm()
        {
            editingId = 0;
            txtFirstName.Text = "";
            txtLastName.Text  = "";
            txtEmail.Text     = "";
            cmbDepartment.SelectedIndex = 0;
            lblFormMode.Text = "REGISTER NEW INSTRUCTOR";
            btnSave.Text = "✔ Save Instructor";
            btnCancelEdit.Visible = false;
            lblError.Text = "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            { lblError.Text = "First name and last name are required."; return; }

            int deptId = ((ComboItem)cmbDepartment.SelectedItem).Id;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    if (editingId == 0)
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO instructors (first_name, last_name, email, department_id) VALUES (@fn,@ln,@em,@d)", conn))
                        {
                            cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim());
                            cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim());
                            cmd.Parameters.AddWithValue("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                            cmd.Parameters.AddWithValue("@d",  deptId == 0 ? (object)DBNull.Value : deptId);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Instructor registered successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand(
                            "UPDATE instructors SET first_name=@fn, last_name=@ln, email=@em, department_id=@d WHERE instructor_id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@fn", txtFirstName.Text.Trim());
                            cmd.Parameters.AddWithValue("@ln", txtLastName.Text.Trim());
                            cmd.Parameters.AddWithValue("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                            cmd.Parameters.AddWithValue("@d",  deptId == 0 ? (object)DBNull.Value : deptId);
                            cmd.Parameters.AddWithValue("@id", editingId);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Instructor updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                ClearForm();
                LoadInstructors();
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; }
        }

        private void DeleteInstructor(int instructorId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var chk = new MySqlCommand("SELECT COUNT(*) FROM courses WHERE instructor_id=@id", conn))
                {
                    chk.Parameters.AddWithValue("@id", instructorId);
                    if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                    { MessageBox.Show("Cannot delete: this instructor is assigned to one or more courses.", "Delete Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                }
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; return; }

            if (MessageBox.Show("Are you sure you want to delete this instructor?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("DELETE FROM instructors WHERE instructor_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", instructorId);
                    cmd.ExecuteNonQuery();
                }
                LoadInstructors();
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; }
        }
    }
}
