using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class CourseManagementForm : Form
    {
        private DataGridView dgv;
        private TextBox txtCourseName;
        private NumericUpDown numCredits;
        private ComboBox cmbDept, cmbInstructor;
        private Button btnSave, btnCancelEdit, btnRefresh;
        private Label lblStatus, lblError, lblFormMode;
        private int editingId = 0;

        public CourseManagementForm()
        {
            InitializeComponents();
            LoadComboBoxes();
            LoadCourses();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Course Management";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(850, 600);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);

            // ── Top bar ──
            var topBar = new Panel { Size = new Size(1000, 60), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            topBar.Controls.Add(new Label { Text = "📚  Course Management Transaction", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 16), BackColor = Color.Transparent });
            var btnClose = new Button { Text = "✕", Size = new Size(40, 40), Location = new Point(946, 10), Font = new Font("Segoe UI", 12f), ForeColor = Color.FromArgb(180, 200, 225), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            // ── Entry panel ──
            var entry = new Panel { Location = new Point(0, 60), Size = new Size(1000, 140), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            entry.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 139, entry.Width, 139);

            lblFormMode = new Label { Text = "ADD NEW COURSE", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(15, 52, 96), AutoSize = true, Location = new Point(20, 12), BackColor = Color.Transparent };
            entry.Controls.Add(lblFormMode);

            EnrollmentForm.FLbl(entry, "COURSE NAME", 20, 32);
            txtCourseName = new TextBox { Location = new Point(20, 52), Size = new Size(280, 28), Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(txtCourseName);

            EnrollmentForm.FLbl(entry, "CREDITS", 320, 32);
            numCredits = new NumericUpDown { Location = new Point(320, 52), Size = new Size(70, 28), Font = new Font("Segoe UI", 9.5f), Minimum = 1, Maximum = 12, Value = 3 };
            entry.Controls.Add(numCredits);

            EnrollmentForm.FLbl(entry, "DEPARTMENT", 410, 32);
            cmbDept = new ComboBox { Location = new Point(410, 52), Size = new Size(220, 28), Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(cmbDept);

            EnrollmentForm.FLbl(entry, "INSTRUCTOR", 650, 32);
            cmbInstructor = new ComboBox { Location = new Point(650, 52), Size = new Size(220, 28), Font = new Font("Segoe UI", 9.5f), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 249, 252) };
            entry.Controls.Add(cmbInstructor);

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(192, 57, 43), AutoSize = true, Location = new Point(20, 96), BackColor = Color.Transparent };
            entry.Controls.Add(lblError);

            btnSave = EnrollmentForm.Btn("✔ Save Course", Color.FromArgb(39, 174, 96), 130, 34);
            btnSave.Location = new Point(650, 90);
            btnSave.Click += BtnSave_Click;
            entry.Controls.Add(btnSave);

            btnCancelEdit = EnrollmentForm.Btn("✕ Cancel Edit", Color.FromArgb(150, 165, 185), 110, 34);
            btnCancelEdit.Location = new Point(790, 90);
            btnCancelEdit.Visible = false;
            btnCancelEdit.Click += (s, e) => CancelEdit();
            entry.Controls.Add(btnCancelEdit);

            // ── Grid header ──
            var gh = new Panel { Location = new Point(15, 210), Size = new Size(970, 44), BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            gh.Controls.Add(new Label { Text = "Course Records", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(0, 12), BackColor = Color.Transparent });
            btnRefresh = EnrollmentForm.Btn("↺ Refresh", Color.FromArgb(100, 120, 150), 90, 30);
            btnRefresh.Location = new Point(875, 7);
            btnRefresh.Click += (s, e) => { CancelEdit(); LoadCourses(); };
            gh.Controls.Add(btnRefresh);

            // ── DataGridView ──
            dgv = new DataGridView
            {
                Location = new Point(15, 257),
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
            dgv.CellClick += Dgv_CellClick;

            lblStatus = new Label { Text = "Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 140, 165), AutoSize = true, Location = new Point(20, 658), BackColor = Color.Transparent, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            this.Controls.AddRange(new Control[] { topBar, entry, gh, dgv, lblStatus });
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            Color bg = col == "Edit"   ? Color.FromArgb(52, 152, 219) :
                       col == "Delete" ? Color.FromArgb(231, 76, 60)   : Color.Empty;
            if (bg == Color.Empty) return;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.CellBounds);
            string txt = e.Value?.ToString() ?? "";
            using (var sf = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center })
            using (var f = new Font("Segoe UI", 8.5f))
                e.Graphics.DrawString(txt, f, Brushes.White, e.CellBounds, sf);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
            e.Handled = true;
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ID"].Value);

            if (col == "Edit")   PopulateEditForm(id);
            else if (col == "Delete") DeleteCourse(id);
        }

        private void LoadComboBoxes()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    cmbDept.Items.Clear();
                    cmbDept.Items.Add(new ComboItem(0, "(No Department)"));
                    using (var cmd = new MySqlCommand("SELECT department_id, department_name FROM departments ORDER BY department_name", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) cmbDept.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
                    cmbDept.SelectedIndex = 0;

                    cmbInstructor.Items.Clear();
                    cmbInstructor.Items.Add(new ComboItem(0, "(No Instructor)"));
                    using (var cmd = new MySqlCommand("SELECT instructor_id, CONCAT(first_name,' ',last_name) FROM instructors ORDER BY last_name, first_name", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) cmbInstructor.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
                    cmbInstructor.SelectedIndex = 0;
                }
            }
            catch (Exception ex) { lblError.Text = "Error loading dropdowns: " + ex.Message; }
        }

        public void LoadCourses()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand(@"
                    SELECT c.course_id AS ID,
                           c.course_name AS Course,
                           c.credits AS Credits,
                           IFNULL(d.department_name,'-') AS Department,
                           IFNULL(CONCAT(i.first_name,' ',i.last_name),'-') AS Instructor
                    FROM courses c
                    LEFT JOIN departments d ON c.department_id = d.department_id
                    LEFT JOIN instructors i ON c.instructor_id = i.instructor_id
                    ORDER BY c.course_name", conn))
                {
                    var dt = new DataTable();
                    new MySqlDataAdapter(cmd).Fill(dt);
                    dgv.DataSource = dt;
                    if (dgv.Columns["ID"] != null) dgv.Columns["ID"].Visible = false;

                    if (dgv.Columns["Edit"] == null)
                    {
                        var editCol = new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "✏ Edit", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 80 };
                        dgv.Columns.Add(editCol);
                    }
                    if (dgv.Columns["Delete"] == null)
                    {
                        var delCol = new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "🗑 Delete", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 90 };
                        dgv.Columns.Add(delCol);
                    }
                    lblStatus.Text = $"{dt.Rows.Count} course(s) found.";
                }
            }
            catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
        }

        private void PopulateEditForm(int courseId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("SELECT course_name, credits, department_id, instructor_id FROM courses WHERE course_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", courseId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        txtCourseName.Text = r["course_name"].ToString();
                        numCredits.Value = Convert.ToDecimal(r["credits"]);
                        SelectComboById(cmbDept, r.IsDBNull(r.GetOrdinal("department_id")) ? 0 : r.GetInt32("department_id"));
                        SelectComboById(cmbInstructor, r.IsDBNull(r.GetOrdinal("instructor_id")) ? 0 : r.GetInt32("instructor_id"));
                    }
                }
                editingId = courseId;
                lblFormMode.Text = "EDIT COURSE";
                btnSave.Text = "✔ Update Course";
                btnCancelEdit.Visible = true;
                lblError.Text = "";
            }
            catch (Exception ex) { lblError.Text = "Error loading course: " + ex.Message; }
        }

        private static void SelectComboById(ComboBox cmb, int id)
        {
            for (int i = 0; i < cmb.Items.Count; i++)
                if (((ComboItem)cmb.Items[i]).Id == id) { cmb.SelectedIndex = i; return; }
            cmb.SelectedIndex = 0;
        }

        private void CancelEdit()
        {
            editingId = 0;
            txtCourseName.Text = "";
            numCredits.Value = 3;
            cmbDept.SelectedIndex = 0;
            cmbInstructor.SelectedIndex = 0;
            lblFormMode.Text = "ADD NEW COURSE";
            btnSave.Text = "✔ Save Course";
            btnCancelEdit.Visible = false;
            lblError.Text = "";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtCourseName.Text))
            { lblError.Text = "Course name is required."; return; }

            int deptId       = ((ComboItem)cmbDept.SelectedItem).Id;
            int instructorId = ((ComboItem)cmbInstructor.SelectedItem).Id;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    if (editingId == 0)
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO courses (course_name, credits, department_id, instructor_id) VALUES (@n,@cr,@d,@i)", conn))
                        {
                            cmd.Parameters.AddWithValue("@n",  txtCourseName.Text.Trim());
                            cmd.Parameters.AddWithValue("@cr", (int)numCredits.Value);
                            cmd.Parameters.AddWithValue("@d",  deptId == 0 ? (object)DBNull.Value : deptId);
                            cmd.Parameters.AddWithValue("@i",  instructorId == 0 ? (object)DBNull.Value : instructorId);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Course added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand(
                            "UPDATE courses SET course_name=@n, credits=@cr, department_id=@d, instructor_id=@i WHERE course_id=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@n",  txtCourseName.Text.Trim());
                            cmd.Parameters.AddWithValue("@cr", (int)numCredits.Value);
                            cmd.Parameters.AddWithValue("@d",  deptId == 0 ? (object)DBNull.Value : deptId);
                            cmd.Parameters.AddWithValue("@i",  instructorId == 0 ? (object)DBNull.Value : instructorId);
                            cmd.Parameters.AddWithValue("@id", editingId);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Course updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                CancelEdit();
                LoadCourses();
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; }
        }

        private void DeleteCourse(int courseId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    using (var chk = new MySqlCommand("SELECT COUNT(*) FROM enrollments WHERE course_id=@id", conn))
                    {
                        chk.Parameters.AddWithValue("@id", courseId);
                        if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                        { MessageBox.Show("Cannot delete: this course has existing enrollments.", "Delete Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                    }
                }
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; return; }

            if (MessageBox.Show("Are you sure you want to delete this course?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("DELETE FROM courses WHERE course_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", courseId);
                    cmd.ExecuteNonQuery();
                }
                LoadCourses();
            }
            catch (Exception ex) { lblError.Text = "Error: " + ex.Message; }
        }
    }
}
