using System;
using System.Drawing;
using System.Windows.Forms;

namespace InfoSystem
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "About AcadSystem";
            this.Size = new Size(560, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            var header = new Panel { Size = new Size(560, 180), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96) };
            header.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, 65, 115)), 380, -60, 200, 200); e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(20, 65, 115)), -40, 80, 140, 140); };

            var logo = new Panel { Size = new Size(75, 75), Location = new Point(240, 30), BackColor = Color.Transparent };
            logo.Paint += (s, e) => { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(52, 152, 219)), 0, 12, 55, 55); e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(41, 128, 185)), 20, 0, 55, 55); e.Graphics.FillRectangle(Brushes.White, 28, 18, 20, 20); };

            var lblName = new Label { Text = "AcadSystem", Font = new Font("Segoe UI", 20f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent };
            lblName.Location = new Point((560 - lblName.PreferredWidth) / 2, 118);
            var lblSub = new Label { Text = "Academic Information System", Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(150, 195, 235), AutoSize = true, BackColor = Color.Transparent };
            lblSub.Location = new Point((560 - lblSub.PreferredWidth) / 2, 150);
            header.Controls.AddRange(new Control[] { logo, lblName, lblSub });

            var badge = new Panel { Size = new Size(120, 30), Location = new Point(220, 185), BackColor = Color.FromArgb(52, 152, 219) };
            badge.Paint += (s, e) => { using (var f = new Font("Segoe UI", 9f, FontStyle.Bold)) e.Graphics.DrawString("Version 1.0.0", f, Brushes.White, 15, 7); };

            var info = new (string l, string v)[] {
                ("Build Date",  "January 2024"),
                ("Framework",   ".NET 6 / Windows Forms"),
                ("Platform",    "Windows 10 / 11 (64-bit)"),
                ("Database",    "MySQL (XAMPP / MySQL Workbench)"),
                ("DB Package",  "MySql.Data 8.3.0"),
                ("License",     "Academic / Institutional License"),
                ("Developer",   "Information Systems Department"),
                ("Contact",     "support@acadsystem.edu"),
            };

            int iy = 240; bool alt = false;
            foreach (var (l, v) in info)
            {
                var row = new Panel { Size = new Size(560, 38), Location = new Point(0, iy), BackColor = alt ? Color.FromArgb(247, 250, 254) : Color.White };
                row.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 240, 248)), 0, 37, 560, 37);
                row.Controls.Add(new Label { Text = l, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Color.FromArgb(80, 100, 130), Size = new Size(170, 38), Location = new Point(40, 0), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent });
                row.Controls.Add(new Label { Text = v, Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(35, 55, 85), AutoSize = true, Location = new Point(215, 11), BackColor = Color.Transparent });
                this.Controls.Add(row); iy += 38; alt = !alt;
            }

            var btnClose = new Button { Text = "CLOSE", Size = new Size(120, 38), Location = new Point(220, iy + 18), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), BackColor = Color.FromArgb(15, 52, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(25, 72, 126);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(15, 52, 96);

            this.Controls.AddRange(new Control[] { header, badge, btnClose });
            this.ClientSize = new Size(560, iy + 75);
        }
    }
}
