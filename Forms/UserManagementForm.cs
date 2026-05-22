using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class UserManagementForm : Form
    {
        private DataGridView dgvUsers;
        private TextBox txtSearch;
        private Button btnSearch, btnAdd, btnRefresh;
        private Label lblStatus;

        public UserManagementForm()
        {
            InitializeComponents();
            LoadUsers();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - User Management";
            this.Size = new Size(950, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);

            // ── TOP BAR ──────────────────────────────────────────────
            var topBar = new Panel { Size = new Size(this.ClientSize.Width, 60), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96), Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right) };
            topBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            topBar.Controls.Add(new Label { Text = "👤  User Management", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 16), BackColor = Color.Transparent });

            var btnClose = new Button { Text = "✕", Size = new Size(40, 40), Location = new Point(896, 10), Font = new Font("Segoe UI", 12f), ForeColor = Color.FromArgb(180, 200, 225), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            // ── TOOLBAR ──────────────────────────────────────────────
            var toolbar = new Panel { Size = new Size(this.ClientSize.Width, 55), Location = new Point(0, 60), BackColor = Color.White, Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right) };
            toolbar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            toolbar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 54, toolbar.Width, 54);

            txtSearch = new TextBox { Location = new Point(20, 15), Size = new Size(260, 28), Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252) };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadUsers(txtSearch.Text.Trim()); };

            btnSearch = MakeBtn("🔍 Search", Color.FromArgb(52, 152, 219), 90, 28);
            btnSearch.Location = new Point(288, 15);
            btnSearch.Click += (s, e) => LoadUsers(txtSearch.Text.Trim());

            btnRefresh = MakeBtn("↺ Refresh", Color.FromArgb(100, 120, 150), 90, 28);
            btnRefresh.Location = new Point(386, 15);
            btnRefresh.Click += (s, e) => { txtSearch.Text = ""; LoadUsers(); };

            btnAdd = MakeBtn("+ Add Account", Color.FromArgb(39, 174, 96), 120, 28);
            btnAdd.Location = new Point(800, 15);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += BtnAdd_Click;

            toolbar.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh, btnAdd });

            // ── DATA GRID ────────────────────────────────────────────
            dgvUsers = new DataGridView
            {
                Location = new Point(15, 130),
                Size = new Size(this.ClientSize.Width - 30, this.ClientSize.Height - 175),
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
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(80, 100, 130);
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvUsers.ColumnHeadersHeight = 36;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 255);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 40, 70);
            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 254);

            // Fix white buttons: manually paint button columns
            dgvUsers.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                string colName = dgvUsers.Columns[e.ColumnIndex].Name;
                if (colName == "Edit" || colName == "Toggle")
                {
                    Color bg = colName == "Edit" ? Color.FromArgb(52, 152, 219) : Color.FromArgb(230, 126, 34);
                    e.Graphics.FillRectangle(new SolidBrush(bg), e.CellBounds);
                    string txt = e.Value?.ToString() ?? "";
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var font = new Font("Segoe UI", 8.5f))
                        e.Graphics.DrawString(txt, font, Brushes.White, e.CellBounds, sf);
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                    e.Handled = true;
                }
            };

            // Status bar
            var statusBar = new Panel { Size = new Size(950, 30), Location = new Point(0, 558), BackColor = Color.FromArgb(248, 250, 253), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            statusBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 0, statusBar.Width, 0);
            lblStatus = new Label { Text = "Loading...", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 140, 165), AutoSize = true, Location = new Point(20, 8), BackColor = Color.Transparent };
            statusBar.Controls.Add(lblStatus);

            this.Controls.AddRange(new Control[] { topBar, toolbar, dgvUsers, statusBar });
        }

        // ── Load / Search users ──────────────────────────────────────
        public void LoadUsers(string search = "")
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    string sql = @"SELECT user_id AS 'ID', username AS 'Username', full_name AS 'Full Name',
                                          email AS 'Email', role AS 'Role',
                                          CASE WHEN is_active=1 THEN 'Active' ELSE 'Inactive' END AS 'Status',
                                          DATE_FORMAT(created_at,'%b %d, %Y') AS 'Created'
                                   FROM users";
                    if (!string.IsNullOrEmpty(search))
                        sql += " WHERE username LIKE @s OR full_name LIKE @s OR email LIKE @s OR role LIKE @s";
                    sql += " ORDER BY user_id";

                    using (var adapter = new MySqlDataAdapter(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(search))
                            adapter.SelectCommand.Parameters.AddWithValue("@s", $"%{search}%");

                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUsers.DataSource = dt;

                        // Hide ID column from view but keep it for reference
                        if (dgvUsers.Columns["ID"] != null)
                        { dgvUsers.Columns["ID"].Visible = false; }

                        // Color status column
                        foreach (DataGridViewRow row in dgvUsers.Rows)
                        {
                            if (row.Cells["Status"].Value?.ToString() == "Inactive")
                                row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 50, 50);
                        }

                        // Add action buttons column
                        SetupActionColumns();

                        lblStatus.Text = $"{dt.Rows.Count} account(s) found.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
            }
        }

        private void SetupActionColumns()
        {
            if (dgvUsers.Columns["Edit"] == null)
            {
                var editCol = new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "✏ Edit", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, Width = 75 };
                editCol.DefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
                editCol.DefaultCellStyle.ForeColor = Color.White;
                dgvUsers.Columns.Add(editCol);
            }
            if (dgvUsers.Columns["Toggle"] == null)
            {
                var togCol = new DataGridViewButtonColumn { Name = "Toggle", HeaderText = "Active/Inactive", Text = "Toggle", UseColumnTextForButtonValue = false, FlatStyle = FlatStyle.Flat, Width = 110 };
                togCol.DefaultCellStyle.BackColor = Color.FromArgb(230, 126, 34);
                togCol.DefaultCellStyle.ForeColor = Color.White;
                dgvUsers.Columns.Add(togCol);
            }

            // Update toggle button text per row
            foreach (DataGridViewRow row in dgvUsers.Rows)
            {
                if (row.Cells["Status"].Value?.ToString() == "Active")
                    row.Cells["Toggle"].Value = "⛔ Deactivate";
                else
                    row.Cells["Toggle"].Value = "✔ Activate";
            }

            dgvUsers.CellClick -= DgvUsers_CellClick;
            dgvUsers.CellClick += DgvUsers_CellClick;

            // Force repaint so button text is visible immediately after toggle
            dgvUsers.Refresh();
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvUsers.Rows[e.RowIndex];
            int userId = Convert.ToInt32(row.Cells["ID"].Value);

            if (dgvUsers.Columns[e.ColumnIndex].Name == "Edit")
            {
                OpenEditForm(userId);
            }
            else if (dgvUsers.Columns[e.ColumnIndex].Name == "Toggle")
            {
                string status = row.Cells["Status"].Value?.ToString();
                string action = status == "Active" ? "deactivate" : "activate";
                if (MessageBox.Show($"Are you sure you want to {action} this account?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    ToggleStatus(userId, status == "Active" ? 0 : 1);
            }
        }

        private void ToggleStatus(int userId, int newStatus)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("UPDATE users SET is_active=@s WHERE user_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@s", newStatus);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
                LoadUsers(txtSearch.Text.Trim());
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AccountForm(0);
            form.FormClosed += (s, ev) => LoadUsers();
            form.ShowDialog();
        }

        private void OpenEditForm(int userId)
        {
            var form = new AccountForm(userId);
            form.FormClosed += (s, ev) => LoadUsers();
            form.ShowDialog();
        }

        // ── Helper ───────────────────────────────────────────────────
        private Button MakeBtn(string text, Color color, int w, int h)
        {
            var btn = new Button { Text = text, Size = new Size(w, h), Font = new Font("Segoe UI", 9f), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
