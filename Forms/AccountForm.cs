using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    /// <summary>
    /// Used for both Add (userId=0) and Edit (userId>0).
    /// </summary>
    public class AccountForm : Form
    {
        private readonly int userId;
        private TextBox txtUsername, txtFullName, txtEmail, txtPassword, txtConfirm;
        private ComboBox cmbRole;
        private CheckBox chkActive;
        private Button btnSave, btnCancel;
        private Label lblError;

        public AccountForm(int userId)
        {
            this.userId = userId;
            InitializeComponents();
            if (userId > 0) LoadUser();
        }

        private void InitializeComponents()
        {
            bool isEdit = userId > 0;
            this.Text = isEdit ? "Edit Account" : "Add New Account";
            this.Size = new Size(460, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            // Header strip
            var header = new Panel { Size = new Size(460, 55), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96) };
            header.Controls.Add(new Label { Text = isEdit ? "✏  Edit Account" : "➕  Add New Account", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(20, 14), BackColor = Color.Transparent });

            int y = 75;
            AddField("FULL NAME",  ref y, out txtFullName);
            AddField("USERNAME",   ref y, out txtUsername);
            AddField("EMAIL",      ref y, out txtEmail);

            // Role
            AddLabel("ROLE", y);
            cmbRole = new ComboBox { Location = new Point(30, y + 20), Size = new Size(390, 28), Font = new Font("Segoe UI", 10f), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(247, 249, 252) };
            cmbRole.Items.AddRange(new object[] { "Admin", "Staff", "Instructor" });
            cmbRole.SelectedIndex = 1;
            this.Controls.Add(cmbRole);
            y += 60;

            // Password (show always for add; optional hint for edit)
            string pwLabel = isEdit ? "NEW PASSWORD  (leave blank to keep current)" : "PASSWORD";
            AddLabel(pwLabel, y);
            txtPassword = MakeField(y + 20, true);
            y += 60;

            if (!isEdit)
            {
                AddLabel("CONFIRM PASSWORD", y);
                txtConfirm = MakeField(y + 20, true);
                y += 60;
            }
            else txtConfirm = new TextBox(); // dummy

            // Active checkbox
            chkActive = new CheckBox { Text = "Account is Active", Location = new Point(30, y), Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(30, 50, 80), Checked = true, AutoSize = true };
            this.Controls.Add(chkActive);
            y += 40;

            // Error
            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(192, 57, 43), Location = new Point(30, y), Size = new Size(390, 20), BackColor = Color.Transparent };
            this.Controls.Add(lblError);
            y += 24;

            // Buttons
            btnSave = new Button { Text = isEdit ? "SAVE CHANGES" : "CREATE ACCOUNT", Location = new Point(30, y), Size = new Size(195, 40), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(15, 52, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            btnSave.MouseEnter += (s, e) => btnSave.BackColor = Color.FromArgb(25, 72, 126);
            btnSave.MouseLeave += (s, e) => btnSave.BackColor = Color.FromArgb(15, 52, 96);

            btnCancel = new Button { Text = "CANCEL", Location = new Point(235, y), Size = new Size(185, 40), Font = new Font("Segoe UI", 10f), BackColor = Color.FromArgb(200, 210, 220), ForeColor = Color.FromArgb(50, 60, 80), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { header, btnSave, btnCancel });
            this.ClientSize = new Size(460, y + 60);
        }

        private void AddLabel(string text, int y) =>
            this.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 115, 135), AutoSize = true, Location = new Point(30, y), BackColor = Color.Transparent });

        private void AddField(string label, ref int y, out TextBox txt)
        {
            AddLabel(label, y);
            txt = MakeField(y + 20);
            y += 60;
        }

        private TextBox MakeField(int y, bool pw = false)
        {
            var t = new TextBox { Location = new Point(30, y), Size = new Size(390, 32), Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252), PasswordChar = pw ? '●' : '\0' };
            this.Controls.Add(t);
            return t;
        }

        private void LoadUser()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("SELECT username, full_name, email, role, is_active FROM users WHERE user_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtUsername.Text  = r["username"].ToString();
                            txtFullName.Text  = r["full_name"].ToString();
                            txtEmail.Text     = r["email"].ToString();
                            cmbRole.Text      = r["role"].ToString();
                            chkActive.Checked = Convert.ToBoolean(r["is_active"]);
                        }
                    }
                }
            }
            catch (Exception ex) { lblError.Text = "Error loading user: " + ex.Message; }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            bool isEdit = userId > 0;

            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            { lblError.Text = "Full name, username, and email are required."; return; }

            if (!isEdit)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text)) { lblError.Text = "Password is required."; return; }
                if (txtPassword.Text.Length < 6) { lblError.Text = "Password must be at least 6 characters."; return; }
                if (txtPassword.Text != txtConfirm.Text) { lblError.Text = "Passwords do not match."; return; }
            }

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    if (isEdit)
                    {
                        string sql = "UPDATE users SET full_name=@fn, username=@u, email=@em, role=@r, is_active=@a";
                        if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            if (txtPassword.Text.Length < 6) { lblError.Text = "Password must be at least 6 characters."; return; }
                            sql += ", password_hash=@ph";
                        }
                        sql += " WHERE user_id=@id";

                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@fn", txtFullName.Text.Trim());
                            cmd.Parameters.AddWithValue("@u",  txtUsername.Text.Trim());
                            cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                            cmd.Parameters.AddWithValue("@r",  cmbRole.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@a",  chkActive.Checked ? 1 : 0);
                            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                                cmd.Parameters.AddWithValue("@ph", LoginForm.HashPassword(txtPassword.Text));
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Account updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        using (var cmd = new MySqlCommand(
                            "INSERT INTO users (username, password_hash, full_name, email, role, is_active) VALUES (@u,@ph,@fn,@em,@r,@a)", conn))
                        {
                            cmd.Parameters.AddWithValue("@u",  txtUsername.Text.Trim());
                            cmd.Parameters.AddWithValue("@ph", LoginForm.HashPassword(txtPassword.Text));
                            cmd.Parameters.AddWithValue("@fn", txtFullName.Text.Trim());
                            cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
                            cmd.Parameters.AddWithValue("@r",  cmbRole.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@a",  chkActive.Checked ? 1 : 0);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Account created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                this.Close();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                lblError.Text = "Username or email already exists.";
            }
            catch (Exception ex)
            {
                lblError.Text = "Error: " + ex.Message;
            }
        }
    }
}
