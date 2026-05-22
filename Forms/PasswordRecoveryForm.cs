using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class PasswordRecoveryForm : Form
    {
        private Panel formPanel;
        private Panel stepPanel;
        private int currentStep = 1;
        private string verifiedUsername = "";
        private string verifiedEmail    = "";
        private bool showBackButton;

        // showBackButton: true when opened from Login, false when opened from inside the app
        public PasswordRecoveryForm(bool showBackButton = false)
        {
            this.showBackButton = showBackButton;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Password Recovery";
            this.Size = new Size(540, 615);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(247, 249, 252);
            this.Font = new Font("Segoe UI", 9f);

            var topBar = new Panel { Size = new Size(540, 65), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96) };
            topBar.Controls.Add(new Label { Text = "AcadSystem", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 20), BackColor = Color.Transparent });
            topBar.Controls.Add(new Label { Text = "Password Recovery", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(160, 200, 235), AutoSize = true, Location = new Point(148, 25), BackColor = Color.Transparent });

            var iconPanel = new Panel { Size = new Size(80, 80), Location = new Point(230, 90), BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(230, 240, 255)), 0, 0, 80, 80);
                using (var f = new Font("Segoe UI Symbol", 26f))
                    e.Graphics.DrawString("🔐", f, new SolidBrush(Color.FromArgb(15, 52, 96)), 12, 16);
            };

            this.Controls.Add(new Label { Text = "Reset Your Password", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = Color.FromArgb(20, 35, 60), AutoSize = true, Location = new Point(140, 185) });
            this.Controls.Add(new Label { Text = "Enter your username and registered email to continue.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 140, 160), AutoSize = true, Location = new Point(80, 218) });

            stepPanel = new Panel { Size = new Size(480, 50), Location = new Point(30, 250), BackColor = Color.Transparent };
            DrawSteps();

            formPanel = new Panel { Size = new Size(480, 230), Location = new Point(30, 308), BackColor = Color.White };
            formPanel.Paint += (s, e) => e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 225, 235)), 0, 0, 479, 229);

            var btnBack = new Button { Text = "← Back to Login", Location = new Point(30, 548), AutoSize = true, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(52, 152, 219), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Visible = showBackButton };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { topBar, iconPanel, stepPanel, formPanel, btnBack });
            LoadStep1();
        }

        private void DrawSteps()
        {
            stepPanel.Controls.Clear();
            string[] labels = { "Verify Identity", "New Password", "Done" };
            Color done = Color.FromArgb(39, 174, 96), active = Color.FromArgb(15, 52, 96), inactive = Color.FromArgb(200, 210, 225);
            for (int i = 0; i < 3; i++)
            {
                bool isActive = (i + 1) == currentStep, isDone = (i + 1) < currentStep;
                Color cc = isDone ? done : isActive ? active : inactive;
                int x = 30 + i * 150, ci = i + 1; bool iD = isDone;
                var circle = new Panel { Size = new Size(34, 34), Location = new Point(x, 0), BackColor = Color.Transparent };
                circle.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(cc), 0, 0, 34, 34);
                    string t = iD ? "✓" : ci.ToString();
                    using (var f = new Font("Segoe UI", iD ? 12f : 11f, FontStyle.Bold)) {
                        var sz = e.Graphics.MeasureString(t, f);
                        e.Graphics.DrawString(t, f, Brushes.White, (34 - sz.Width) / 2, (34 - sz.Height) / 2);
                    }
                };
                stepPanel.Controls.Add(circle);
                stepPanel.Controls.Add(new Label { Text = labels[i], Font = new Font("Segoe UI", 8.5f, isActive ? FontStyle.Bold : FontStyle.Regular), ForeColor = isActive || isDone ? Color.FromArgb(30, 45, 70) : Color.FromArgb(160, 170, 185), AutoSize = true, Location = new Point(x - 12, 40), BackColor = Color.Transparent });
                if (i < 2) stepPanel.Controls.Add(new Panel { Size = new Size(110, 2), Location = new Point(x + 38, 16), BackColor = isDone ? done : inactive });
            }
        }

        // ── Step helpers ─────────────────────────────────────────────
        private TextBox MakeField(string placeholder = "", bool pw = false)
        {
            var t = new TextBox { Font = new Font("Segoe UI", 11f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 249, 252), Size = new Size(390, 36), PasswordChar = pw ? '●' : '\0', Text = placeholder };
            return t;
        }
        private Label MakeLbl(string text) => new Label { Text = text, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 115, 135), AutoSize = true, BackColor = Color.Transparent };
        private Label MakeError() => new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(192, 57, 43), AutoSize = true, BackColor = Color.Transparent };
        private Button MakeBtn(string text)
        {
            var btn = new Button { Text = text, Size = new Size(410, 44), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(15, 52, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(25, 72, 126);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(15, 52, 96);
            return btn;
        }

        // ── Step 1: verify username + email ─────────────────────────
        private void LoadStep1()
        {
            formPanel.Controls.Clear(); currentStep = 1; DrawSteps();
            var lblU = MakeLbl("USERNAME"); lblU.Location = new Point(35, 25);
            var txtU = MakeField(""); txtU.Location = new Point(35, 45);
            var lblE = MakeLbl("REGISTERED EMAIL"); lblE.Location = new Point(35, 92);
            var txtE = MakeField(""); txtE.Location = new Point(35, 112);
            var err = MakeError(); err.Location = new Point(35, 158);
            var btn = MakeBtn("VERIFY IDENTITY"); btn.Location = new Point(35, 175);
            btn.Click += (s, e) =>
            {
                err.Text = "";
                if (string.IsNullOrWhiteSpace(txtU.Text) || string.IsNullOrWhiteSpace(txtE.Text))
                { err.Text = "Please fill in all fields."; return; }

                try
                {
                    using (var conn = DatabaseConnection.GetConnection())
                    using (var cmd = new MySqlCommand("SELECT username FROM users WHERE username=@u AND email=@e AND is_active=1", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", txtU.Text.Trim());
                        cmd.Parameters.AddWithValue("@e", txtE.Text.Trim());
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            verifiedUsername = txtU.Text.Trim();
                            verifiedEmail    = txtE.Text.Trim();
                            LoadStep2();
                        }
                        else err.Text = "Username and email do not match any active account.";
                    }
                }
                catch (Exception ex) { err.Text = "DB Error: " + ex.Message; }
            };
            formPanel.Controls.AddRange(new Control[] { lblU, txtU, lblE, txtE, err, btn });
        }

        // ── Step 2: enter new password ───────────────────────────────
        private void LoadStep2()
        {
            formPanel.Controls.Clear(); currentStep = 2; DrawSteps();
            var lbl1 = MakeLbl("NEW PASSWORD"); lbl1.Location = new Point(35, 25);
            var txtP = MakeField("", true); txtP.Location = new Point(35, 45);
            var lbl2 = MakeLbl("CONFIRM PASSWORD"); lbl2.Location = new Point(35, 92);
            var txtC = MakeField("", true); txtC.Location = new Point(35, 112);
            var err = MakeError(); err.Location = new Point(35, 158);
            var btn = MakeBtn("RESET PASSWORD"); btn.Location = new Point(35, 175);
            btn.Click += (s, e) =>
            {
                err.Text = "";
                if (txtP.Text.Length < 6) { err.Text = "Password must be at least 6 characters."; return; }
                if (txtP.Text != txtC.Text) { err.Text = "Passwords do not match."; return; }

                try
                {
                    string hash = LoginForm.HashPassword(txtP.Text);
                    using (var conn = DatabaseConnection.GetConnection())
                    using (var cmd = new MySqlCommand("UPDATE users SET password_hash=@h WHERE username=@u", conn))
                    {
                        cmd.Parameters.AddWithValue("@h", hash);
                        cmd.Parameters.AddWithValue("@u", verifiedUsername);
                        cmd.ExecuteNonQuery();
                    }
                    LoadStep3();
                }
                catch (Exception ex) { err.Text = "DB Error: " + ex.Message; }
            };
            formPanel.Controls.AddRange(new Control[] { lbl1, txtP, lbl2, txtC, err, btn });
        }

        // ── Step 3: success ──────────────────────────────────────────
        private void LoadStep3()
        {
            formPanel.Controls.Clear(); currentStep = 3; DrawSteps();
            var success = new Panel { Size = new Size(460, 230), Location = new Point(0, 0), BackColor = Color.Transparent };
            var icon = new Label { Text = "✔", Font = new Font("Segoe UI", 36f, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96), AutoSize = true, Location = new Point(195, 30), BackColor = Color.Transparent };
            var msg1 = new Label { Text = "Password Reset Successfully!", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(110, 95), BackColor = Color.Transparent };
            var msg2 = new Label { Text = "You can now log in with your new password.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(130, 140, 160), AutoSize = true, Location = new Point(90, 122), BackColor = Color.Transparent };
            var btnLogin = MakeBtn("BACK TO LOGIN"); btnLogin.Location = new Point(35, 165);
            btnLogin.Click += (s, e) => this.Close();
            formPanel.Controls.AddRange(new Control[] { icon, msg1, msg2, btnLogin });
        }
    }
}
