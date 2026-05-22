using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InfoSystem
{
    public class DashboardForm : Form
    {
        private Panel sideNav;
        private Panel mainContent;
        private Panel topBar;
        private Button activeNavBtn;

        public DashboardForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Dashboard";
            this.Size = new Size(1150, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 600);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);
            this.FormClosing += (s, e) => Application.Exit();

            // ── SIDE NAV ─────────────────────────────────────────────
            sideNav = new Panel { Size = new Size(220, 700), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96) };
            sideNav.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            var logoArea = new Panel { Size = new Size(220, 75), Location = new Point(0, 0), BackColor = Color.FromArgb(10, 40, 78) };
            logoArea.Controls.Add(new Label { Text = "AcadSystem", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(20, 15), BackColor = Color.Transparent });
            logoArea.Controls.Add(new Label { Text = "Academic Info System", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(140, 185, 225), AutoSize = true, Location = new Point(20, 45), BackColor = Color.Transparent });

            sideNav.Controls.Add(new Label { Text = "MAIN MENU", Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 155, 200), AutoSize = true, Location = new Point(20, 95), BackColor = Color.Transparent });

            var navItems = new (string text, string icon, string action)[]
            {
                ("Dashboard",        "⊞",  "dashboard"),
                ("Grade Recording",  "📝", "students"),
                ("Instructors",      "🎓", "instructors"),
                ("Courses",          "📚", "courses"),
                ("Enrollments",      "📋", "enrollments"),
            };

            int navY = 118;
            Button firstBtn = null;
            foreach (var item in navItems)
            {
                var btn = MakeNavBtn(item.text, item.icon, item.action);
                btn.Location = new Point(0, navY);
                if (firstBtn == null) firstBtn = btn;
                sideNav.Controls.Add(btn);
                navY += 46;
            }

            sideNav.Controls.Add(new Panel { Size = new Size(180, 1), Location = new Point(20, navY + 4), BackColor = Color.FromArgb(35, 75, 115) });
            navY += 14;

            var navItems2 = new (string text, string icon, string action)[]
            {
                ("User Management",    "🔧", "users"),
                ("Reports",            "📊", "reports"),
                ("About Program",      "ℹ",  "about"),
                ("Password Recovery",  "🔑", "pwrecovery"),
            };
            foreach (var item in navItems2)
            {
                var btn = MakeNavBtn(item.text, item.icon, item.action);
                btn.Location = new Point(0, navY);
                sideNav.Controls.Add(btn);
                navY += 46;
            }

            var btnLogout = new Button { Text = "  ⏻  Logout", Size = new Size(220, 50), Location = new Point(0, 620), Font = new Font("Segoe UI", 10f), ForeColor = Color.FromArgb(200, 80, 80), BackColor = Color.FromArgb(10, 40, 78), FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLogout.Click += (s, e) => { Session.Clear(); this.Hide(); new LoginForm().Show(); };

            sideNav.Controls.AddRange(new Control[] { logoArea, btnLogout });

            // ── TOP BAR ──────────────────────────────────────────────
            topBar = new Panel { Size = new Size(930, 65), Location = new Point(220, 0), BackColor = Color.White };
            topBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(225, 229, 238)), 0, 64, topBar.Width, 64);
            topBar.Controls.Add(new Label { Text = "Dashboard", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.FromArgb(20, 40, 70), AutoSize = true, Location = new Point(30, 18), BackColor = Color.Transparent });
            topBar.Controls.Add(new Label { Text = DateTime.Now.ToString("dddd, MMMM dd yyyy  •  hh:mm tt"), Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(140, 155, 175), AutoSize = true, Location = new Point(30, 42), BackColor = Color.Transparent });

            // Show logged-in user
            var lblUser = new Label { Text = Session.FullName + "  (" + Session.Role + ")", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(60, 80, 110), AutoSize = true, Location = new Point(580, 25), BackColor = Color.Transparent };
            lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            topBar.Controls.Add(lblUser);

            var avatarPanel = new Panel { Size = new Size(40, 40), Location = new Point(870, 12), BackColor = Color.Transparent };
            avatarPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            avatarPanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(52, 152, 219)), 0, 0, 40, 40);
                string initial = Session.FullName?.Length > 0 ? Session.FullName[0].ToString().ToUpper() : "?";
                using (var f = new Font("Segoe UI", 13f, FontStyle.Bold))
                    e.Graphics.DrawString(initial, f, Brushes.White, 11, 9);
            };
            topBar.Controls.Add(avatarPanel);

            // ── MAIN CONTENT ─────────────────────────────────────────
            mainContent = new Panel { Location = new Point(220, 65), Size = new Size(930, 635), BackColor = Color.FromArgb(240, 243, 248), AutoScroll = true };
            mainContent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LoadDashboardContent();

            if (firstBtn != null) { firstBtn.BackColor = Color.FromArgb(25, 72, 126); activeNavBtn = firstBtn; }
            this.Controls.AddRange(new Control[] { sideNav, topBar, mainContent });
        }

        private Button MakeNavBtn(string text, string icon, string action)
        {
            var btn = new Button { Text = $"  {icon}   {text}", Size = new Size(220, 46), Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(180, 210, 240), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Tag = action };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => { if (btn != activeNavBtn) btn.BackColor = Color.FromArgb(20, 62, 108); };
            btn.MouseLeave += (s, e) => { if (btn != activeNavBtn) btn.BackColor = Color.Transparent; };
            btn.Click += (s, e) => {
                if (activeNavBtn != null) activeNavBtn.BackColor = Color.Transparent;
                btn.BackColor = Color.FromArgb(25, 72, 126);
                activeNavBtn = btn;
                switch (btn.Tag?.ToString())
                {
                    case "dashboard":    LoadDashboardContent(); break;
                    case "students":    new GradeRecordingForm().Show(); break;
                    case "instructors": new InstructorManagementForm().Show(); break;
                    case "courses":     new CourseManagementForm().Show(); break;
                    case "enrollments": new EnrollmentForm().Show(); break;
                    case "users":       new UserManagementForm().Show(); break;
                    case "reports":     new ReportGeneratorForm().Show(); break;
                    case "about":       new AboutForm().Show(); break;
                    case "pwrecovery":  new PasswordRecoveryForm().Show(); break;
                }
            };
            return btn;
        }

        private void LoadDashboardContent()
        {
            mainContent.Controls.Clear();

            // ── Pull live counts from DB ──────────────────────────────
            int cntStudents = 0, cntCourses = 0, cntInstructors = 0, cntEnrollments = 0;
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    string[] queries = {
                        "SELECT COUNT(*) FROM students",
                        "SELECT COUNT(*) FROM courses",
                        "SELECT COUNT(*) FROM instructors",
                        "SELECT COUNT(*) FROM enrollments"
                    };
                    int[] results = new int[4];
                    for (int i = 0; i < queries.Length; i++)
                        using (var cmd = new MySqlCommand(queries[i], conn))
                            results[i] = Convert.ToInt32(cmd.ExecuteScalar());
                    cntStudents = results[0]; cntCourses = results[1];
                    cntInstructors = results[2]; cntEnrollments = results[3];
                }
            }
            catch { /* show zeros if DB unavailable */ }

            var cards = new (string title, string value, string sub, Color accent)[]
            {
                ("Total Students",  cntStudents.ToString("N0"),    "Registered in the system",   Color.FromArgb(52, 152, 219)),
                ("Active Courses",  cntCourses.ToString("N0"),     "Available this semester",    Color.FromArgb(39, 174, 96)),
                ("Instructors",     cntInstructors.ToString("N0"), "Teaching this semester",     Color.FromArgb(230, 126, 34)),
                ("Enrollments",     cntEnrollments.ToString("N0"), "Total enrollment records",   Color.FromArgb(155, 89, 182)),
            };

            int cx = 25;
            foreach (var c in cards) { var card = MakeStatCard(c.title, c.value, c.sub, c.accent); card.Location = new Point(cx, 25); mainContent.Controls.Add(card); cx += 215; }

            // ── Recent Enrollments from DB ────────────────────────────
            var tp = new Panel { Location = new Point(25, 175), Size = new Size(580, 330), BackColor = Color.White };
            tp.Paint += (s, e) => { e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 228, 238)), 0, 0, 579, 329); e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(248, 250, 253)), 0, 0, 580, 48); };
            tp.Controls.Add(new Label { Text = "Recent Enrollments", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(20, 14), BackColor = Color.Transparent });

            string[] headers = { "Student", "Course", "Dept", "Grade", "Date" };
            int[] colX = { 15, 140, 290, 390, 455 };
            for (int i = 0; i < headers.Length; i++)
                tp.Controls.Add(new Label { Text = headers[i].ToUpper(), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(130, 145, 165), AutoSize = true, Location = new Point(colX[i], 58), BackColor = Color.Transparent });

            int ry = 82; bool alt = false;
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand(@"
                    SELECT CONCAT(s.first_name, ' ', s.last_name) AS student,
                           c.course_name AS course,
                           IFNULL(d.department_name, '-') AS dept,
                           IFNULL(e.grade, '-') AS grade,
                           DATE_FORMAT(e.enrollment_date, '%b %d') AS edate
                    FROM enrollments e
                    JOIN students s ON e.student_id = s.student_id
                    JOIN courses c ON e.course_id = c.course_id
                    LEFT JOIN departments d ON c.department_id = d.department_id
                    ORDER BY e.enrollment_id DESC
                    LIMIT 6", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string student = reader["student"].ToString();
                        string course  = reader["course"].ToString();
                        if (course.Length > 20) course = course.Substring(0, 18) + "...";
                        string dept  = reader["dept"].ToString();
                        if (dept.Length > 10) dept = dept.Substring(0, 8) + "...";
                        string grade = reader["grade"].ToString();
                        string date  = reader["edate"].ToString();

                        var row = new Panel { Size = new Size(580, 36), Location = new Point(0, ry), BackColor = alt ? Color.FromArgb(249, 251, 254) : Color.White };
                        row.Paint += (s2, e2) => e2.Graphics.DrawLine(new Pen(Color.FromArgb(237, 241, 248)), 0, 35, 580, 35);
                        string[] vals = { student, course, dept, grade, date };
                        for (int i = 0; i < vals.Length; i++)
                        {
                            var lbl = new Label { Text = vals[i], Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(45, 60, 85), AutoSize = true, Location = new Point(colX[i], 10), BackColor = Color.Transparent };
                            if (i == 3 && grade.Length > 0 && grade != "-") lbl.ForeColor = grade.StartsWith("A") ? Color.FromArgb(39, 174, 96) : Color.FromArgb(52, 152, 219);
                            row.Controls.Add(lbl);
                        }
                        tp.Controls.Add(row);
                        ry += 36; alt = !alt;
                    }
                }
            }
            catch
            {
                tp.Controls.Add(new Label { Text = "Could not load enrollment data.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(180, 50, 50), AutoSize = true, Location = new Point(15, 90), BackColor = Color.Transparent });
            }
            mainContent.Controls.Add(tp);

            // Quick actions
            var qp = new Panel { Location = new Point(620, 175), Size = new Size(285, 330), BackColor = Color.White };
            qp.Paint += (s, e) => { e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 228, 238)), 0, 0, 284, 329); e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(248, 250, 253)), 0, 0, 285, 48); };
            qp.Controls.Add(new Label { Text = "Quick Actions", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(20, 14), BackColor = Color.Transparent });

            var actions = new (string text, Color c)[] {
                ("Enroll a Student",    Color.FromArgb(52, 152, 219)),
                ("Add New Course",      Color.FromArgb(39, 174, 96)),
                ("Register Instructor", Color.FromArgb(230, 126, 34)),
                ("User Management",     Color.FromArgb(155, 89, 182)),
                ("Generate Report",     Color.FromArgb(231, 76, 60)),
            };
            int qy = 62;
            foreach (var a in actions)
            {
                var qbtn = new Button { Text = a.text, Size = new Size(245, 42), Location = new Point(20, qy), Font = new Font("Segoe UI", 9.5f), BackColor = a.c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };
                qbtn.FlatAppearance.BorderSize = 0;
                if (a.text == "Enroll a Student")    qbtn.Click += (s, e) => new EnrollmentForm().Show();
                if (a.text == "Add New Course")      qbtn.Click += (s, e) => new CourseManagementForm().Show();
                if (a.text == "Register Instructor") qbtn.Click += (s, e) => new InstructorManagementForm().Show();
                if (a.text == "User Management")     qbtn.Click += (s, e) => new UserManagementForm().Show();
                if (a.text == "Generate Report")     qbtn.Click += (s, e) => new ReportGeneratorForm().Show();
                qp.Controls.Add(qbtn);
                qy += 50;
            }
            mainContent.Controls.Add(qp);
        }

        private Panel MakeStatCard(string title, string value, string sub, Color accent)
        {
            var card = new Panel { Size = new Size(205, 135), BackColor = Color.White };
            card.Paint += (s, e) => { e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 228, 238)), 0, 0, 204, 134); e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, 135); };
            card.Controls.Add(new Label { Text = title.ToUpper(), Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(130, 145, 165), AutoSize = true, Location = new Point(20, 22), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 28f, FontStyle.Bold), ForeColor = Color.FromArgb(20, 40, 70), AutoSize = true, Location = new Point(18, 42), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = sub, Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 135, 155), AutoSize = true, Location = new Point(20, 100), BackColor = Color.Transparent });
            return card;
        }
    }
}
