using System;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private LinkLabel lnkForgotPassword;
        private Label lblError;

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Login";
            this.Size = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            // ── LEFT PANEL ──────────────────────────────────────────
            var left = new Panel { Size = new Size(380, 560), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96) };

            var c1 = new Panel { Size = new Size(180, 180), Location = new Point(260, -60), BackColor = Color.Transparent };
            c1.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(30, 80, 140)), 0, 0, 180, 180); };
            var c2 = new Panel { Size = new Size(120, 120), Location = new Point(-30, 400), BackColor = Color.Transparent };
            c2.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(30, 80, 140)), 0, 0, 120, 120); };

            var logo = new Panel { Size = new Size(70, 70), Location = new Point(40, 80), BackColor = Color.Transparent };
            logo.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(52, 152, 219)), 0, 10, 50, 50); e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(41, 128, 185)), 20, 0, 50, 50); e.Graphics.FillRectangle(Brushes.White, 28, 18, 14, 14); };

            left.Controls.Add(new Label { Text = "AcadSystem", Font = new Font("Segoe UI", 26f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(40, 165), BackColor = Color.Transparent });
            left.Controls.Add(new Label { Text = "Academic Information System", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(180, 210, 240), AutoSize = true, Location = new Point(40, 210), BackColor = Color.Transparent });
            left.Controls.Add(new Panel { Size = new Size(50, 3), Location = new Point(40, 245), BackColor = Color.FromArgb(52, 152, 219) });
            left.Controls.Add(new Label { Text = "Manage students, courses, and\nenrollments all in one place.", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(160, 200, 235), Location = new Point(40, 268), Size = new Size(300, 50), BackColor = Color.Transparent });

            string[] features = { "✔  Student & Enrollment Tracking", "✔  Course & Instructor Management", "✔  Academic Report Generation" };
            int fy = 340;
            foreach (var f in features) { left.Controls.Add(new Label { Text = f, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(140, 190, 230), AutoSize = true, Location = new Point(40, fy), BackColor = Color.Transparent }); fy += 28; }

            left.Controls.Add(new Label { Text = "v1.0.0  |  © 2024 AcadSystem", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(100, 150, 200), AutoSize = true, Location = new Point(40, 510), BackColor = Color.Transparent });
            left.Controls.AddRange(new Control[] { c1, c2, logo });

            // ── RIGHT PANEL ─────────────────────────────────────────
            var right = new Panel { Size = new Size(520, 560), Location = new Point(380, 0), BackColor = Color.White };

            right.Controls.Add(new Label { Text = "Welcome Back", Font = new Font("Segoe UI", 22f, FontStyle.Bold), ForeColor = Color.FromArgb(30, 40, 60), AutoSize = true, Location = new Point(70, 75) });
            right.Controls.Add(new Label { Text = "Sign in to your academic account", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(130, 140, 160), AutoSize = true, Location = new Point(70, 115) });

            right.Controls.Add(new Label { Text = "USERNAME", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 110, 130), AutoSize = true, Location = new Point(70, 180) });
            txtUsername = new TextBox { Location = new Point(70, 200), Size = new Size(370, 36), Font = new Font("Segoe UI", 11f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252), Text = "admin" };
            right.Controls.Add(txtUsername);

            right.Controls.Add(new Label { Text = "PASSWORD", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 110, 130), AutoSize = true, Location = new Point(70, 258) });
            txtPassword = new TextBox { Location = new Point(70, 278), Size = new Size(370, 36), Font = new Font("Segoe UI", 11f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252), PasswordChar = '●' };
            right.Controls.Add(txtPassword);

            lnkForgotPassword = new LinkLabel { Text = "Forgot Password?", Font = new Font("Segoe UI", 9f), Location = new Point(70, 328), AutoSize = true, LinkColor = Color.FromArgb(52, 152, 219) };
            lnkForgotPassword.Click += (s, e) => new PasswordRecoveryForm(showBackButton: true).Show();
            right.Controls.Add(lnkForgotPassword);

            // Error label
            lblError = new Label { Text = "", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(192, 57, 43), AutoSize = true, Location = new Point(70, 355), BackColor = Color.Transparent };
            right.Controls.Add(lblError);

            btnLogin = new Button { Text = "SIGN IN", Location = new Point(70, 375), Size = new Size(370, 48), Font = new Font("Segoe UI", 11f, FontStyle.Bold), BackColor = Color.FromArgb(15, 52, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(25, 72, 126);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(15, 52, 96);
            right.Controls.Add(btnLogin);

            right.Controls.Add(new Label { Text = "Need help? Contact your system administrator.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(160, 165, 175), AutoSize = true, Location = new Point(70, 440) });

            this.Controls.AddRange(new Control[] { left, right });

            // Allow Enter key to submit
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Please enter your username and password.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Signing in...";

            try
            {
                string hash = HashPassword(password);

                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand(
                    "SELECT user_id, full_name, role FROM users WHERE username = @u AND password_hash = @p AND is_active = 1", conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", hash);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Store session info
                            Session.UserId   = reader.GetInt32("user_id");
                            Session.FullName = reader.GetString("full_name");
                            Session.Role     = reader.GetString("role");
                            Session.Username = username;

                            this.Hide();
                            new DashboardForm().Show();
                        }
                        else
                        {
                            lblError.Text = "Invalid username or password, or account is inactive.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Database error: " + ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "SIGN IN";
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
