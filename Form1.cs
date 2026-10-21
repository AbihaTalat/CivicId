
using System;
using System.Drawing;
using System.Windows.Forms;
using CivicID.Data;

namespace CivicID
{
    public partial class Form1 : Form
    {
        private readonly Color paper = Color.FromArgb(245, 243, 237);
        private readonly Color sidebarColor = Color.FromArgb(237, 235, 227);
        private readonly Color white = Color.FromArgb(251, 250, 247);
        private readonly Color ink = Color.FromArgb(52, 56, 50);
        private readonly Color muted = Color.FromArgb(108, 116, 107);
        private readonly Color sage = Color.FromArgb(120, 136, 117);
        private readonly Color line = Color.FromArgb(221, 219, 210);

        private readonly Font bodyFont = new Font("Segoe UI", 9F);
        private readonly Font smallFont = new Font("Segoe UI", 8F);
        private readonly Font headingFont =
            new Font("Georgia", 25F, FontStyle.Regular);

        private Panel sidebar = null!;
        private Panel content = null!;
        private Panel header = null!;
        private Label totalValue = null!;
        private Label pendingValue = null!;
        private Label approvedValue = null!;
        private Label recentCount = null!;
        private DataGridView recentGrid = null!;

        public Form1()
        {
            Text = "CivicID — Citizen Services";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(950, 650);
            Size = new Size(1240, 800);
            BackColor = paper;
            ForeColor = ink;
            Font = bodyFont;
            AutoScaleMode = AutoScaleMode.Dpi;

            BuildLayout();
            LoadOverview();
        }

        private void BuildLayout()
        {
            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = paper,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 218));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(shell);

            sidebar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = sidebarColor,
                Padding = new Padding(23, 25, 18, 20)
            };

            shell.Controls.Add(sidebar, 0, 0);
            BuildSidebar();

            content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = paper,
                Padding = new Padding(34, 0, 34, 24)
            };

            shell.Controls.Add(content, 1, 0);

            header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = white
            };

            content.Controls.Add(header);

            var headerLabel = new Label
            {
                Text = "CITIZEN SERVICES  /  ADMINISTRATION",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = muted,
                AutoSize = true,
                Location = new Point(25, 29)
            };

            header.Controls.Add(headerLabel);

            var overview = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = paper,
                Padding = new Padding(0, 26, 0, 0)
            };

            content.Controls.Add(overview);
            overview.BringToFront();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = paper,
                ColumnCount = 1,
                RowCount = 5,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // Give the heading, statistics and section title enough space.
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            overview.Controls.Add(layout);

            var intro = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = paper
            };

            var eyebrow = new Label
            {
                Text = "OVERVIEW",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = sage,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            intro.Controls.Add(eyebrow);

            var title = new Label
            {
                Text = "A clearer view of citizen records.",
                Font = headingFont,
                ForeColor = ink,
                AutoSize = true,
                Location = new Point(0, 23)
            };

            intro.Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Application activity and citizen records, in one place.",
                Font = bodyFont,
                ForeColor = muted,
                AutoSize = true,
                Location = new Point(2, 72)
            };

            intro.Controls.Add(subtitle);
            layout.Controls.Add(intro, 0, 0);

            var stats = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = paper,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            for (int i = 0; i < 3; i++)
            {
                stats.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 33.333F));
            }

            totalValue = AddStat(stats, 0, "TOTAL CITIZENS");
            pendingValue = AddStat(stats, 1, "PENDING APPLICATIONS");
            approvedValue = AddStat(stats, 2, "APPROVED APPLICATIONS");

            layout.Controls.Add(stats, 0, 1);

            var sectionHeader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = paper
            };

            AddRule(sectionHeader, 0);

            var recentTitle = new Label
            {
                Text = "Recently submitted",
                Font = new Font("Georgia", 15F),
                ForeColor = ink,
                AutoSize = true,
                Location = new Point(0, 15)
            };

            sectionHeader.Controls.Add(recentTitle);

            recentCount = new Label
            {
                Text = "",
                Font = smallFont,
                ForeColor = muted,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            sectionHeader.Controls.Add(recentCount);

            sectionHeader.Resize += (_, _) =>
            {
                recentCount.Location = new Point(
                    Math.Max(0, sectionHeader.ClientSize.Width - recentCount.Width),
                    20);
            };

            layout.Controls.Add(sectionHeader, 0, 2);

            recentGrid = CreateGrid();
            layout.Controls.Add(recentGrid, 0, 3);

            var footer = new Label
            {
                Text = "CIVICID  ·  CITIZEN RECORDS SYSTEM",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = muted,
                TextAlign = ContentAlignment.BottomLeft,
                Dock = DockStyle.Fill
            };

            layout.Controls.Add(footer, 0, 4);
        }

        private void BuildSidebar()
        {
            var brand = new Label
            {
                Text = "civicid.",
                Font = new Font("Georgia", 25F),
                ForeColor = ink,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            sidebar.Controls.Add(brand);

            var tagline = new Label
            {
                Text = "CITIZEN SERVICES",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = sage,
                AutoSize = true,
                Location = new Point(2, 40)
            };

            sidebar.Controls.Add(tagline);

            var divider = new Panel
            {
                BackColor = line,
                Height = 1,
                Location = new Point(0, 76),
                Width = 170
            };

            sidebar.Controls.Add(divider);

            var section = new Label
            {
                Text = "WORKSPACE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = muted,
                AutoSize = true,
                Location = new Point(2, 98)
            };

            sidebar.Controls.Add(section);

            string[] pages =
            {
                "Overview",
                "Registration",
                "Citizen records",
                "Verification",
                "OpenCV studio",
                "Reports"
            };

            for (int i = 0; i < pages.Length; i++)
            {
                int index = i;

                var button = new Button
                {
                    Text = pages[i],
                    Tag = pages[i],
                    Location = new Point(0, 127 + i * 45),
                    Size = new Size(174, 37),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(11, 0, 0, 0),
                    Font = bodyFont,
                    ForeColor = i == 0 ? ink : muted,
                    BackColor = i == 0
                        ? Color.FromArgb(221, 226, 215)
                        : sidebarColor,
                    Cursor = Cursors.Hand
                };

                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor =
                    Color.FromArgb(225, 227, 218);

                button.Click += (_, _) => OpenPage(index);
                sidebar.Controls.Add(button);
            }

            var bottomRule = new Panel
            {
                BackColor = line,
                Height = 1,
                Dock = DockStyle.Bottom
            };

            sidebar.Controls.Add(bottomRule);

            var bottomLabel = new Label
            {
                Text = "CITIZEN RECORDS SYSTEM",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = muted,
                Dock = DockStyle.Bottom,
                Height = 27,
                TextAlign = ContentAlignment.BottomLeft
            };

            sidebar.Controls.Add(bottomLabel);
        }

        private Label AddStat(
            TableLayoutPanel parent,
            int column,
            string caption)
        {
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = paper,
                Padding = new Padding(0, 12, 16, 0),
                Margin = Padding.Empty
            };

            if (column > 0)
            {
                var separator = new Panel
                {
                    BackColor = line,
                    Width = 1,
                    Dock = DockStyle.Left
                };

                wrapper.Controls.Add(separator);
            }

            var captionLabel = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = muted,
                AutoSize = true,
                Location = new Point(column > 0 ? 18 : 0, 12)
            };

            wrapper.Controls.Add(captionLabel);

            var value = new Label
            {
                Text = "—",
                Font = new Font("Georgia", 27F),
                ForeColor = ink,
                AutoSize = true,
                Location = new Point(column > 0 ? 18 : 0, 35)
            };

            wrapper.Controls.Add(value);
            parent.Controls.Add(wrapper, column, 0);

            return value;
        }

        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = white,
                BorderStyle = BorderStyle.None,
                GridColor = line,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = true,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both,
                ShowCellToolTips = true,
                Font = bodyFont
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = white;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = muted;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 8F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding =
                new Padding(9, 7, 9, 7);
            grid.ColumnHeadersHeight = 39;

            grid.DefaultCellStyle.BackColor = white;
            grid.DefaultCellStyle.ForeColor = ink;
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(226, 231, 222);
            grid.DefaultCellStyle.SelectionForeColor = ink;
            grid.DefaultCellStyle.Padding = new Padding(9, 5, 9, 5);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 247, 242);
            grid.RowTemplate.Height = 39;

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ApplicationReference",
                HeaderText = "REFERENCE",
                DataPropertyName = "ApplicationReference",
                Width = 190,
                MinimumWidth = 150
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                HeaderText = "CITIZEN NAME",
                DataPropertyName = "FullName",
                Width = 210,
                MinimumWidth = 160
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cnic",
                HeaderText = "CNIC",
                DataPropertyName = "Cnic",
                Width = 155,
                MinimumWidth = 135
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ApplicationStatus",
                HeaderText = "STATUS",
                DataPropertyName = "ApplicationStatus",
                Width = 125,
                MinimumWidth = 100
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedAt",
                HeaderText = "DATE SUBMITTED",
                DataPropertyName = "CreatedAt",
                Width = 190,
                MinimumWidth = 160
            });

            grid.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                e.ToolTipText = Convert.ToString(
                    grid.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue)
                    ?? "";
            };

            grid.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                if (grid.Columns[e.ColumnIndex].Name == "CreatedAt"
                    && e.Value != null
                    && DateTime.TryParse(e.Value.ToString(), out var date))
                {
                    e.Value = date.ToString("dd MMM yyyy, HH:mm");
                    e.FormattingApplied = true;
                }
            };

            return grid;
        }

        private void AddRule(Control parent, int y)
        {
            parent.Controls.Add(new Panel
            {
                BackColor = line,
                Height = 1,
                Dock = DockStyle.Top
            });
        }

        private void LoadOverview()
        {
            try
            {
                using var connection = DatabaseService.GetConnection();

                using var summary = connection.CreateCommand();
                summary.CommandText = @"
                    SELECT
                        COUNT(*),
                        SUM(CASE WHEN ApplicationStatus = 'Pending'
                            THEN 1 ELSE 0 END),
                        SUM(CASE WHEN ApplicationStatus = 'Approved'
                            THEN 1 ELSE 0 END)
                    FROM Citizens;";

                using var reader = summary.ExecuteReader();

                if (reader.Read())
                {
                    totalValue.Text =
                        reader.IsDBNull(0) ? "0" : reader.GetInt64(0).ToString();

                    pendingValue.Text =
                        reader.IsDBNull(1) ? "0" : reader.GetInt64(1).ToString();

                    approvedValue.Text =
                        reader.IsDBNull(2) ? "0" : reader.GetInt64(2).ToString();
                }

                reader.Close();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT ApplicationReference, FullName, Cnic,
                           ApplicationStatus, CreatedAt
                    FROM Citizens
                    ORDER BY Id DESC
                    LIMIT 100;";

                using var records = command.ExecuteReader();

                var table = new System.Data.DataTable();
                table.Load(records);

                recentGrid.DataSource = table;
                recentCount.Text = $"{table.Rows.Count} recent records";

                recentCount.Location = new Point(
                    Math.Max(0,
                        recentCount.Parent!.ClientSize.Width - recentCount.Width),
                    20);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load citizen records.\n\n{ex.Message}",
                    "CivicID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void OpenPage(int index)
        {
            if (index == 0)
            {
                LoadOverview();
                return;
            }

            Form? page = index switch
            {
                1 => new RegistrationForm(),
                2 => new CitizenRecordsForm(),
                3 => new VerificationForm(),
                4 => new OpenCvStudioForm(),
                5 => new ReportsForm(),
                _ => null
            };

            if (page == null)
                return;

            using (page)
            {
                page.StartPosition = FormStartPosition.CenterParent;
                page.ShowDialog(this);
            }

            LoadOverview();
        }
    }
}
