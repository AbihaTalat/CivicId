
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CivicID.Data;

namespace CivicID;

public class ReportsForm : Form
{
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private readonly DataGridView reportsGrid = new();

    private readonly Label totalLabel = new();
    private readonly Label pendingLabel = new();
    private readonly Label approvedLabel = new();
    private readonly Label rejectedLabel = new();

    private DataTable reportData = new();

    private readonly Color backgroundColor = Color.FromArgb(247, 246, 241);
    private readonly Color inkColor = Color.FromArgb(42, 48, 43);
    private readonly Color mutedColor = Color.FromArgb(108, 116, 107);

    public ReportsForm()
    {
        Text = "CivicID | Reports";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1200, 780);
        MinimumSize = new Size(900, 620);
        BackColor = backgroundColor;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        LoadApplications();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(24),
            BackColor = backgroundColor
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill };
        header.Controls.Add(new Label
        {
            Text = "Reports",
            Font = new Font("Georgia", 23F),
            ForeColor = inkColor,
            AutoSize = true,
            Location = new Point(0, 0)
        });
        header.Controls.Add(new Label
        {
            Text = "Review application activity and export records.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = mutedColor,
            AutoSize = true,
            Location = new Point(2, 43)
        });
        root.Controls.Add(header, 0, 0);

        var stats = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 8)
        };

        for (int i = 0; i < 4; i++)
            stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        stats.Controls.Add(CreateStat("TOTAL RECORDS", totalLabel), 0, 0);
        stats.Controls.Add(CreateStat("PENDING", pendingLabel), 1, 0);
        stats.Controls.Add(CreateStat("APPROVED", approvedLabel), 2, 0);
        stats.Controls.Add(CreateStat("REJECTED", rejectedLabel), 3, 0);

        root.Controls.Add(stats, 0, 1);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 8, 0, 4),
            Margin = Padding.Empty
        };

        searchBox.Width = 300;
        searchBox.Height = 32;
        searchBox.PlaceholderText = "Search name, CNIC, or reference...";
        searchBox.Margin = new Padding(0, 2, 10, 2);
        searchBox.TextChanged += (_, _) => ApplyFilters();

        statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        statusFilter.Width = 155;
        statusFilter.Height = 32;
        statusFilter.Margin = new Padding(0, 2, 10, 2);
        statusFilter.Items.AddRange(new object[]
        {
            "All statuses", "Pending", "Approved", "Rejected"
        });
        statusFilter.SelectedIndex = 0;
        statusFilter.SelectedIndexChanged += (_, _) => ApplyFilters();

        var refreshButton = MakeButton("Refresh", 95);
        refreshButton.Click += (_, _) => LoadApplications();

        var exportButton = MakeButton("Export CSV", 110);
        exportButton.Click += (_, _) => ExportCsv();

        toolbar.Controls.Add(searchBox);
        toolbar.Controls.Add(statusFilter);
        toolbar.Controls.Add(refreshButton);
        toolbar.Controls.Add(exportButton);
        root.Controls.Add(toolbar, 0, 2);

        ConfigureGrid();
        root.Controls.Add(reportsGrid, 0, 3);

        var footer = new Label
        {
            Text = "Exported files contain the records currently matching your filters.",
            Dock = DockStyle.Fill,
            ForeColor = mutedColor,
            Font = new Font("Segoe UI", 9F),
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(footer, 0, 4);
    }

    private Control CreateStat(string title, Label valueLabel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 10, 0),
            Padding = new Padding(12, 8, 8, 8)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(layout);

        layout.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            ForeColor = mutedColor,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false
        }, 0, 0);

        valueLabel.Text = "0";
        valueLabel.Dock = DockStyle.Fill;
        valueLabel.ForeColor = inkColor;
        valueLabel.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        valueLabel.AutoSize = false;
        layout.Controls.Add(valueLabel, 0, 1);

        return panel;
    }

    private Button MakeButton(string text, int width)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(104, 128, 105),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 1, 10, 2)
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void ConfigureGrid()
    {
        reportsGrid.Dock = DockStyle.Fill;
        reportsGrid.BackgroundColor = Color.White;
        reportsGrid.BorderStyle = BorderStyle.None;
        reportsGrid.GridColor = Color.FromArgb(230, 232, 225);
        reportsGrid.EnableHeadersVisualStyles = false;
        reportsGrid.ColumnHeadersHeight = 48;
        reportsGrid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        reportsGrid.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(236, 239, 231);
        reportsGrid.ColumnHeadersDefaultCellStyle.ForeColor = inkColor;
        reportsGrid.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 9F, FontStyle.Bold);
        reportsGrid.ColumnHeadersDefaultCellStyle.WrapMode =
            DataGridViewTriState.True;
        reportsGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        reportsGrid.DefaultCellStyle.ForeColor = inkColor;
        reportsGrid.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(222, 232, 220);
        reportsGrid.DefaultCellStyle.SelectionForeColor = inkColor;
        reportsGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        reportsGrid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(250, 250, 247);
        reportsGrid.RowHeadersVisible = false;
        reportsGrid.AllowUserToAddRows = false;
        reportsGrid.AllowUserToDeleteRows = false;
        reportsGrid.AllowUserToResizeColumns = true;
        reportsGrid.AllowUserToResizeRows = false;
        reportsGrid.AllowUserToOrderColumns = false;
        reportsGrid.AutoGenerateColumns = true;
        reportsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        reportsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        reportsGrid.ScrollBars = ScrollBars.Both;
        reportsGrid.ReadOnly = true;
        reportsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        reportsGrid.MultiSelect = false;
        reportsGrid.ShowCellToolTips = true;
        reportsGrid.CellToolTipTextNeeded += ShowCellTooltip;
        reportsGrid.CellFormatting += FormatDateCell;
    }

    private void LoadApplications()
    {
        try
        {
            using var connection = DatabaseService.GetConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    ApplicationReference,
                    FullName,
                    Cnic,
                    DateOfBirth,
                    Gender,
                    Address,
                    Phone,
                    ApplicationStatus,
                    CreatedAt
                FROM Citizens
                ORDER BY Id DESC;";

            reportData = new DataTable();
            using var reader = command.ExecuteReader();
            reportData.Load(reader);

            UpdateSummary();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not load reports.\n\n" + ex.Message,
                "CivicID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateSummary()
    {
        int total = reportData.Rows.Count;
        int pending = 0;
        int approved = 0;
        int rejected = 0;

        foreach (DataRow row in reportData.Rows)
        {
            string status = row["ApplicationStatus"]?.ToString() ?? "";

            if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                pending++;
            else if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                approved++;
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                rejected++;
        }

        totalLabel.Text = total.ToString();
        pendingLabel.Text = pending.ToString();
        approvedLabel.Text = approved.ToString();
        rejectedLabel.Text = rejected.ToString();
    }

    private void ApplyFilters()
    {
        if (reportData.Columns.Count == 0)
            return;

        var filtered = reportData.Clone();
        string search = searchBox.Text.Trim();
        string selectedStatus = statusFilter.SelectedItem?.ToString()
                                ?? "All statuses";

        foreach (DataRow row in reportData.Rows)
        {
            string name = row["FullName"]?.ToString() ?? "";
            string cnic = row["Cnic"]?.ToString() ?? "";
            string reference = row["ApplicationReference"]?.ToString() ?? "";
            string status = row["ApplicationStatus"]?.ToString() ?? "";

            bool matchesSearch =
                name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                cnic.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                reference.Contains(search, StringComparison.OrdinalIgnoreCase);

            bool matchesStatus =
                selectedStatus == "All statuses" ||
                status.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase);

            if (matchesSearch && matchesStatus)
                filtered.ImportRow(row);
        }

        reportsGrid.DataSource = filtered;

        SetColumn("ApplicationReference", "Application reference", 230);
        SetColumn("FullName", "Full name", 210);
        SetColumn("Cnic", "CNIC", 155);
        SetColumn("DateOfBirth", "Date of birth", 145);
        SetColumn("Gender", "Gender", 110);
        SetColumn("Address", "Address", 240);
        SetColumn("Phone", "Phone", 150);
        SetColumn("ApplicationStatus", "Status", 125);
        SetColumn("CreatedAt", "Date submitted", 190);
    }

    private void SetColumn(string name, string header, int width)
    {
        if (reportsGrid.Columns[name] is DataGridViewColumn column)
        {
           column.HeaderText = header;
           column.Width = width;
           column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    private void ShowCellTooltip(
        object? sender,
        DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (sender is not DataGridView grid ||
            e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        e.ToolTipText = grid.Rows[e.RowIndex]
            .Cells[e.ColumnIndex].FormattedValue?.ToString() ?? "";
    }

    private void FormatDateCell(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid ||
            e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        string column = grid.Columns[e.ColumnIndex].Name;

        if ((column == "CreatedAt" || column == "DateOfBirth") &&
            e.Value != null &&
            DateTime.TryParse(e.Value.ToString(), out DateTime date))
        {
            e.Value = column == "CreatedAt"
                ? date.ToString("dd MMM yyyy, HH:mm")
                : date.ToString("dd MMM yyyy");

            e.FormattingApplied = true;
        }
    }

    private void ExportCsv()
    {
        if (reportsGrid.DataSource is not DataTable table ||
            table.Rows.Count == 0)
        {
            MessageBox.Show(
                "There are no matching records to export.",
                "CivicID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"CivicID_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            Title = "Export report"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var csv = new StringBuilder();

            for (int column = 0; column < table.Columns.Count; column++)
            {
                if (column > 0)
                    csv.Append(',');

                csv.Append(EscapeCsv(table.Columns[column].ColumnName));
            }

            csv.AppendLine();

            foreach (DataRow row in table.Rows)
            {
                for (int column = 0; column < table.Columns.Count; column++)
                {
                    if (column > 0)
                        csv.Append(',');

                    string value = row[column] == DBNull.Value
                        ? ""
                        : row[column].ToString() ?? "";

                    string name = table.Columns[column].ColumnName;

                    if (name == "CreatedAt" &&
                        DateTime.TryParse(value, out DateTime submitted))
                        value = submitted.ToString("dd MMM yyyy, HH:mm");

                    if (name == "DateOfBirth" &&
                        DateTime.TryParse(value, out DateTime dob))
                        value = dob.ToString("dd MMM yyyy");

                    csv.Append(EscapeCsv(value));
                }

                csv.AppendLine();
            }

            File.WriteAllText(
                dialog.FileName,
                csv.ToString(),
                new UTF8Encoding(true));

            MessageBox.Show(
                "Report exported successfully.",
                "CivicID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not export the report.\n\n" + ex.Message,
                "CivicID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string EscapeCsv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
