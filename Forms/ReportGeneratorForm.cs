using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace InfoSystem
{
    public class ReportGeneratorForm : Form
    {
        private ListBox lstReports;
        private DataGridView dgvReport;
        private ComboBox cmbDepartment, cmbDateRange;
        private DateTimePicker dtFrom, dtTo;
        private Button btnGenerate, btnExport, btnClear;
        private Label lblStatus;
        private ProgressBar progressBar;
        private DataTable currentData;

        public ReportGeneratorForm()
        {
            InitializeComponents();
            LoadDepartments();
        }

        private void InitializeComponents()
        {
            this.Text = "AcadSystem - Report Generator";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 600);
            this.BackColor = Color.FromArgb(240, 243, 248);
            this.Font = new Font("Segoe UI", 9f);

            // ── Top bar ──
            var topBar = new Panel { Size = new Size(1100, 60), Location = new Point(0, 0), BackColor = Color.FromArgb(15, 52, 96), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            topBar.Controls.Add(new Label { Text = "📊  Report Generator", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(25, 16), BackColor = Color.Transparent });
            var btnClose = new Button { Text = "✕", Size = new Size(40, 40), Location = new Point(1046, 10), Font = new Font("Segoe UI", 12f), ForeColor = Color.FromArgb(180, 200, 225), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Click += (s, e) => this.Close();
            topBar.Controls.Add(btnClose);

            // ── Left filter panel ──
            var left = new Panel { Location = new Point(0, 60), Size = new Size(300, 640), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            left.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 299, 0, 299, 640);

            SLbl(left, "REPORT TYPE", 20);
            lstReports = new ListBox { Location = new Point(20, 42), Size = new Size(260, 100), Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(247, 250, 254), SelectionMode = SelectionMode.One };
            lstReports.Items.AddRange(new object[] { "📋  Student Course Report", "📈  Course Enrollment Report", "🎓  Instructor Course Report" });
            lstReports.SelectedIndex = 0;
            left.Controls.Add(lstReports);

            SLbl(left, "FILTER BY DEPARTMENT", 158);
            cmbDepartment = new ComboBox { Location = new Point(20, 180), Size = new Size(260, 30), Font = new Font("Segoe UI", 9.5f), BackColor = Color.FromArgb(247, 250, 254), DropDownStyle = ComboBoxStyle.DropDownList };
            left.Controls.Add(cmbDepartment);

            SLbl(left, "DATE RANGE PRESET", 225);
            cmbDateRange = new ComboBox { Location = new Point(20, 247), Size = new Size(260, 30), Font = new Font("Segoe UI", 9.5f), BackColor = Color.FromArgb(247, 250, 254), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDateRange.Items.AddRange(new object[] { "All Records", "Last 30 Days", "Last 6 Months", "This Year", "Custom Range" });
            cmbDateRange.SelectedIndex = 0;
            cmbDateRange.SelectedIndexChanged += CmbDateRange_Changed;
            left.Controls.Add(cmbDateRange);

            left.Controls.Add(new Label { Text = "From:", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 120, 150), AutoSize = true, Location = new Point(20, 292), BackColor = Color.Transparent });
            dtFrom = new DateTimePicker { Location = new Point(20, 312), Size = new Size(120, 28), Font = new Font("Segoe UI", 9f), Value = new DateTime(2000, 1, 1) };
            left.Controls.Add(dtFrom);
            left.Controls.Add(new Label { Text = "To:", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(100, 120, 150), AutoSize = true, Location = new Point(155, 292), BackColor = Color.Transparent });
            dtTo = new DateTimePicker { Location = new Point(155, 312), Size = new Size(120, 28), Font = new Font("Segoe UI", 9f), Value = DateTime.Now };
            left.Controls.Add(dtTo);

            btnGenerate = new Button { Text = "GENERATE REPORT", Location = new Point(20, 575), Size = new Size(260, 42), Font = new Font("Segoe UI", 10f, FontStyle.Bold), BackColor = Color.FromArgb(15, 52, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.MouseEnter += (s, e) => btnGenerate.BackColor = Color.FromArgb(25, 72, 126);
            btnGenerate.MouseLeave += (s, e) => btnGenerate.BackColor = Color.FromArgb(15, 52, 96);
            btnGenerate.Click += BtnGenerate_Click;
            left.Controls.Add(btnGenerate);

            // ── Right content panel ──
            var right = new Panel { Location = new Point(300, 60), Size = new Size(800, 640), BackColor = Color.FromArgb(240, 243, 248), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };

            var tb2 = new Panel { Size = new Size(800, 50), Location = new Point(0, 0), BackColor = Color.White };
            tb2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tb2.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 49, tb2.Width, 49);
            tb2.Controls.Add(new Label { Text = "Report Preview", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.FromArgb(25, 40, 70), AutoSize = true, Location = new Point(20, 14), BackColor = Color.Transparent });

            btnExport = new Button { Text = "⬇ Export to Excel", Size = new Size(130, 32), Location = new Point(540, 9), Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Enabled = false };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExport.Click += BtnExport_Click;

            btnClear = new Button { Text = "✕ Clear", Size = new Size(80, 32), Location = new Point(678, 9), Font = new Font("Segoe UI", 9f), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Enabled = false };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Click += (s, e) => { dgvReport.DataSource = null; currentData = null; btnExport.Enabled = false; btnClear.Enabled = false; lblStatus.Text = "Ready"; };
            tb2.Controls.AddRange(new Control[] { btnExport, btnClear });

            progressBar = new ProgressBar { Location = new Point(0, 50), Size = new Size(800, 5), Style = ProgressBarStyle.Continuous };
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            dgvReport = new DataGridView
            {
                Location = new Point(10, 65),
                Size = new Size(780, 520),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f),
                GridColor = Color.FromArgb(230, 235, 245),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            EnrollmentForm.ApplyGridStyle(dgvReport);

            var sb2 = new Panel { Size = new Size(800, 30), Location = new Point(0, 610), BackColor = Color.FromArgb(248, 250, 253), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            sb2.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 228, 238)), 0, 0, sb2.Width, 0);
            lblStatus = new Label { Text = "Ready", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(120, 140, 165), AutoSize = true, Location = new Point(15, 8), BackColor = Color.Transparent };
            sb2.Controls.Add(lblStatus);

            right.Controls.AddRange(new Control[] { tb2, progressBar, dgvReport, sb2 });
            this.Controls.AddRange(new Control[] { topBar, left, right });
        }

        private void SLbl(Panel p, string t, int y) =>
            p.Controls.Add(new Label { Text = t, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 120, 155), AutoSize = true, Location = new Point(20, y), BackColor = Color.Transparent });

        private void CmbDateRange_Changed(object sender, EventArgs e)
        {
            string sel = cmbDateRange.SelectedItem?.ToString() ?? "";
            DateTime from = new DateTime(2000, 1, 1);
            DateTime to = DateTime.Now;
            switch (sel)
            {
                case "Last 30 Days":   from = DateTime.Now.AddDays(-30); break;
                case "Last 6 Months":  from = DateTime.Now.AddMonths(-6); break;
                case "This Year":      from = new DateTime(DateTime.Now.Year, 1, 1); break;
            }
            dtFrom.Value = from;
            dtTo.Value = to;
        }

        private void LoadDepartments()
        {
            cmbDepartment.Items.Clear();
            cmbDepartment.Items.Add(new ComboItem(0, "All Departments"));
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new MySqlCommand("SELECT department_id, department_name FROM departments ORDER BY department_name", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) cmbDepartment.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
            }
            catch { /* leave with just "All Departments" */ }
            cmbDepartment.SelectedIndex = 0;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            btnGenerate.Enabled = false;
            lblStatus.Text = "Generating...";
            progressBar.Value = 0;
            var t = new Timer { Interval = 40 };
            t.Tick += (ts, te) =>
            {
                progressBar.Value = Math.Min(progressBar.Value + 10, 100);
                if (progressBar.Value >= 100)
                {
                    t.Stop();
                    btnGenerate.Enabled = true;
                    PopulateGrid();
                }
            };
            t.Start();
        }

        private void PopulateGrid()
        {
            int sel = lstReports.SelectedIndex;
            int deptId = cmbDepartment.SelectedItem is ComboItem ci ? ci.Id : 0;
            DateTime from = dtFrom.Value.Date;
            DateTime to   = dtTo.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                DataTable dt = sel == 0 ? QueryStudentCourse(deptId, from, to) :
                               sel == 1 ? QueryCourseEnrollment(deptId, from, to) :
                                          QueryInstructorCourse(deptId, from, to);
                currentData = dt;
                dgvReport.DataSource = dt;
                btnExport.Enabled = dt.Rows.Count > 0;
                btnClear.Enabled  = true;
                lblStatus.Text = $"Generated: {DateTime.Now:hh:mm tt}  —  {dt.Rows.Count} row(s)";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                btnGenerate.Enabled = true;
            }
        }

        private static DataTable QueryStudentCourse(int deptId, DateTime from, DateTime to)
        {
            string sql = @"SELECT CONCAT(s.first_name,' ',s.last_name) AS Student,
                                  c.course_name AS Course,
                                  c.credits AS Credits,
                                  IFNULL(d.department_name,'-') AS Department,
                                  IFNULL(e.grade,'-') AS Grade,
                                  DATE_FORMAT(e.enrollment_date,'%b %d, %Y') AS 'Enrollment Date'
                           FROM enrollments e
                           JOIN students s ON e.student_id = s.student_id
                           JOIN courses c ON e.course_id = c.course_id
                           LEFT JOIN departments d ON c.department_id = d.department_id
                           WHERE e.enrollment_date BETWEEN @f AND @t";
            if (deptId > 0) sql += " AND c.department_id = @dept";
            sql += " ORDER BY s.last_name, s.first_name";
            return RunQuery(sql, deptId, from, to);
        }

        private static DataTable QueryCourseEnrollment(int deptId, DateTime from, DateTime to)
        {
            string sql = @"SELECT c.course_name AS Course,
                                  c.credits AS Credits,
                                  IFNULL(d.department_name,'-') AS Department,
                                  COUNT(e.enrollment_id) AS Enrolled
                           FROM courses c
                           LEFT JOIN departments d ON c.department_id = d.department_id
                           LEFT JOIN enrollments e ON c.course_id = e.course_id
                               AND e.enrollment_date BETWEEN @f AND @t
                           WHERE 1=1";
            if (deptId > 0) sql += " AND c.department_id = @dept";
            sql += " GROUP BY c.course_id, c.course_name, c.credits, d.department_name ORDER BY Enrolled DESC";
            return RunQuery(sql, deptId, from, to);
        }

        private static DataTable QueryInstructorCourse(int deptId, DateTime from, DateTime to)
        {
            string sql = @"SELECT CONCAT(i.first_name,' ',i.last_name) AS Instructor,
                                  IFNULL(d.department_name,'-') AS Department,
                                  c.course_name AS Course,
                                  COUNT(e.enrollment_id) AS Students
                           FROM instructors i
                           JOIN courses c ON c.instructor_id = i.instructor_id
                           LEFT JOIN departments d ON i.department_id = d.department_id
                           LEFT JOIN enrollments e ON c.course_id = e.course_id
                               AND e.enrollment_date BETWEEN @f AND @t
                           WHERE 1=1";
            if (deptId > 0) sql += " AND i.department_id = @dept";
            sql += " GROUP BY i.instructor_id, c.course_id ORDER BY i.last_name, i.first_name";
            return RunQuery(sql, deptId, from, to);
        }

        private static DataTable RunQuery(string sql, int deptId, DateTime from, DateTime to)
        {
            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@f", from);
                cmd.Parameters.AddWithValue("@t", to);
                if (deptId > 0) cmd.Parameters.AddWithValue("@dept", deptId);
                var dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        // ── Excel Export ──────────────────────────────────────────────────

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (currentData == null || currentData.Rows.Count == 0)
            { MessageBox.Show("No data to export. Generate a report first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string[] titles = { "Student_Course_Report", "Course_Enrollment_Report", "Instructor_Course_Report" };
            string baseName = titles[lstReports.SelectedIndex] + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm");

            using (var dlg = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = baseName, Title = "Save Report As" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    BuildExcelFile(dlg.FileName);
                    if (MessageBox.Show($"Report exported successfully.\n\nOpen file now?", "Export Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                }
                catch (Exception ex) { MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void BuildExcelFile(string path)
        {
            int sel = lstReports.SelectedIndex;
            string[] reportTitles = { "Student Course Report", "Course Enrollment Report", "Instructor Course Report" };
            string reportTitle = reportTitles[sel];
            string dept = (cmbDepartment.SelectedItem as ComboItem)?.ToString() ?? "All Departments";

            using (var pkg = new ExcelPackage())
            {
                var ws1 = pkg.Workbook.Worksheets.Add("Report");
                int lastRow = BuildReportSheet(ws1, reportTitle, dept);
                var ws2 = pkg.Workbook.Worksheets.Add("Chart");
                BuildChartSheet(ws2, sel, reportTitle);
                pkg.SaveAs(new FileInfo(path));
            }
        }

        private int BuildReportSheet(ExcelWorksheet ws, string reportTitle, string dept)
        {
            int colCount = currentData.Columns.Count;

            // ── Header block (rows 1–4) ──
            ws.Row(1).Height = 28;
            ws.Row(2).Height = 18;
            ws.Row(3).Height = 22;
            ws.Row(4).Height = 16;

            var headerRange = ws.Cells[1, 1, 4, colCount];
            headerRange.Merge = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 52, 96));

            ws.Cells[1, 1].Value = "ACADSYSTEM";
            ws.Cells[1, 1].Style.Font.Size = 20;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Color.SetColor(Color.White);
            ws.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Cells[1, 1].Style.Indent = 2;

            // Subtitle row
            var sub = ws.Cells[5, 1, 5, colCount];
            sub.Merge = true;
            sub.Value = "Academic Information System  |  " + reportTitle;
            sub.Style.Font.Size = 12;
            sub.Style.Font.Bold = true;
            sub.Style.Font.Color.SetColor(Color.FromArgb(15, 52, 96));
            sub.Style.Fill.PatternType = ExcelFillStyle.Solid;
            sub.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 232, 248));
            sub.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sub.Style.Indent = 2;
            ws.Row(5).Height = 20;

            // Meta row
            var meta = ws.Cells[6, 1, 6, colCount];
            meta.Merge = true;
            meta.Value = $"Generated: {DateTime.Now:MMMM dd, yyyy  hh:mm tt}     Department: {dept}     Generated by: {Session.FullName}";
            meta.Style.Font.Size = 9;
            meta.Style.Font.Italic = true;
            meta.Style.Font.Color.SetColor(Color.FromArgb(100, 120, 150));
            meta.Style.Fill.PatternType = ExcelFillStyle.Solid;
            meta.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 245, 252));
            meta.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            meta.Style.Indent = 2;
            ws.Row(6).Height = 15;

            // Spacer
            ws.Row(7).Height = 6;

            // ── Column headers (row 8) ──
            int headerRow = 8;
            for (int c = 0; c < currentData.Columns.Count; c++)
            {
                var cell = ws.Cells[headerRow, c + 1];
                cell.Value = currentData.Columns[c].ColumnName.ToUpper();
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 9;
                cell.Style.Font.Color.SetColor(Color.FromArgb(30, 50, 90));
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(210, 225, 245));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(15, 52, 96));
            }
            ws.Row(headerRow).Height = 18;

            // ── Data rows ──
            int dataStartRow = headerRow + 1;
            for (int r = 0; r < currentData.Rows.Count; r++)
            {
                bool alt = r % 2 == 1;
                Color rowBg = alt ? Color.FromArgb(242, 247, 255) : Color.White;
                for (int c = 0; c < currentData.Columns.Count; c++)
                {
                    var cell = ws.Cells[dataStartRow + r, c + 1];
                    cell.Value = currentData.Rows[r][c];
                    cell.Style.Font.Size = 9;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(rowBg);
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                    cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(220, 228, 238));
                }
                ws.Row(dataStartRow + r).Height = 15;
            }

            // Auto-fit columns
            for (int c = 1; c <= currentData.Columns.Count; c++)
                ws.Column(c).AutoFit(10, 50);

            // Outer border around data
            int lastDataRow = dataStartRow + currentData.Rows.Count - 1;
            if (lastDataRow >= dataStartRow)
            {
                var tableRange = ws.Cells[headerRow, 1, lastDataRow, colCount];
                tableRange.Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(15, 52, 96));
            }

            // ── Signature block ──
            int sigRow = lastDataRow + 3;
            ws.Row(sigRow).Height = 14;
            ws.Cells[sigRow, 1].Value = "Prepared by:";
            ws.Cells[sigRow, 1].Style.Font.Bold = true;
            ws.Cells[sigRow, 1].Style.Font.Size = 9;

            ws.Row(sigRow + 1).Height = 18;
            ws.Cells[sigRow + 1, 1].Value = Session.FullName;
            ws.Cells[sigRow + 1, 1].Style.Font.Bold = true;
            ws.Cells[sigRow + 1, 1].Style.Font.Size = 10;
            ws.Cells[sigRow + 1, 1].Style.Font.Color.SetColor(Color.FromArgb(15, 52, 96));

            ws.Row(sigRow + 2).Height = 14;
            ws.Cells[sigRow + 2, 1].Value = "________________________";
            ws.Cells[sigRow + 2, 1].Style.Font.Size = 9;

            ws.Row(sigRow + 3).Height = 12;
            ws.Cells[sigRow + 3, 1].Value = "Signature over printed name";
            ws.Cells[sigRow + 3, 1].Style.Font.Size = 8;
            ws.Cells[sigRow + 3, 1].Style.Font.Italic = true;
            ws.Cells[sigRow + 3, 1].Style.Font.Color.SetColor(Color.FromArgb(130, 140, 155));

            ws.Row(sigRow + 5).Height = 14;
            ws.Cells[sigRow + 5, 1].Value = "Date Signed:  ________________________";
            ws.Cells[sigRow + 5, 1].Style.Font.Size = 9;

            return sigRow + 5;
        }

        private void BuildChartSheet(ExcelWorksheet ws2, int reportType, string reportTitle)
        {
            // Write chart summary data to cells, then reference for chart
            ws2.Cells["A1"].Value = "Category";
            ws2.Cells["B1"].Value = "Value";
            ws2.Cells["A1"].Style.Font.Bold = true;
            ws2.Cells["B1"].Style.Font.Bold = true;

            var chartData = GetChartData(reportType);
            int row = 2;
            foreach (var kv in chartData)
            {
                ws2.Cells[row, 1].Value = kv.Key;
                ws2.Cells[row, 2].Value = kv.Value;
                row++;
            }

            if (row <= 2) return; // no data, skip chart

            // Auto-fit
            ws2.Column(1).AutoFit(10, 40);
            ws2.Column(2).AutoFit(8, 20);

            // ── Create chart ──
            eChartType chartType = reportType == 1 ? eChartType.BarClustered : eChartType.ColumnClustered;
            var chart = ws2.Drawings.AddChart("Chart1", chartType) as ExcelBarChart;
            if (chart == null) return;

            chart.Title.Text = reportTitle + " — Summary Chart";
            chart.Title.Font.Size = 13;
            chart.Legend.Position = eLegendPosition.Bottom;

            var series = chart.Series.Add(ws2.Cells[2, 2, row - 1, 2], ws2.Cells[2, 1, row - 1, 1]);
            series.Header = "Count";

            chart.SetPosition(1, 0, 3, 0);
            chart.SetSize(700, 400);
        }

        private Dictionary<string, int> GetChartData(int reportType)
        {
            var dict = new Dictionary<string, int>();
            if (currentData == null) return dict;

            if (reportType == 0)
            {
                // Grade distribution
                foreach (DataRow r in currentData.Rows)
                {
                    string g = r["Grade"]?.ToString() ?? "-";
                    if (g == "-") g = "No Grade";
                    if (dict.ContainsKey(g)) dict[g]++;
                    else dict[g] = 1;
                }
            }
            else if (reportType == 1)
            {
                // Enrollments per course (already in data)
                foreach (DataRow r in currentData.Rows)
                {
                    string course = r["Course"]?.ToString() ?? "Unknown";
                    if (course.Length > 25) course = course.Substring(0, 23) + "..";
                    int enrolled = Convert.ToInt32(r["Enrolled"]);
                    dict[course] = enrolled;
                }
            }
            else
            {
                // Students per instructor (aggregate)
                foreach (DataRow r in currentData.Rows)
                {
                    string instructor = r["Instructor"]?.ToString() ?? "Unknown";
                    int students = Convert.ToInt32(r["Students"]);
                    if (dict.ContainsKey(instructor)) dict[instructor] += students;
                    else dict[instructor] = students;
                }
            }
            return dict;
        }
    }
}
