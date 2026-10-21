
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CivicID.Data;

namespace CivicID;

public class CitizenRecordsForm : Form
{
    private readonly TextBox searchBox = new();
    private readonly DataGridView recordsGrid = new();
    private readonly Label countLabel = new();
    private readonly ToolTip cellToolTip = new();

    private readonly Color backgroundColor = Color.FromArgb(247, 246, 241);
    private readonly Color inkColor = Color.FromArgb(42, 48, 43);
    private readonly Color mutedColor = Color.FromArgb(108, 116, 107);

    public CitizenRecordsForm()
    {
        Text = "CivicID | Citizen Records";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1160, 740);
        MinimumSize = new Size(850, 580);
        BackColor = backgroundColor;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        LoadRecords();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(24),
            BackColor = backgroundColor
        };

        root.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 100));

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 58));

        root.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 34));

        root.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        Controls.Add(root);

        // Header
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = backgroundColor
        };

        var heading = new Label
        {
            Text = "Citizen records",
            Font = new Font("Georgia", 23F),
            ForeColor = inkColor,
            AutoSize = false,
            Location = new Point(0, 0),
            Size = new Size(800, 40)
        };

        var subtitle = new Label
        {
            Text = "Search registered citizens by name, CNIC, or application reference.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = mutedColor,
            AutoSize = false,
            Location = new Point(2, 48),
            Size = new Size(900, 26)
        };

        header.Controls.Add(heading);
        header.Controls.Add(subtitle);

        root.Controls.Add(header, 0, 0);

        // Search toolbar
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 6, 0, 4),
            Margin = Padding.Empty,
            BackColor = backgroundColor
        };

        searchBox.Width = 350;
        searchBox.Height = 32;
        searchBox.Font = new Font("Segoe UI", 10F);
        searchBox.PlaceholderText = "Search name, CNIC, or reference...";
        searchBox.Margin = new Padding(0, 2, 10, 2);
        searchBox.TextChanged += (_, _) => LoadRecords();

        var refreshButton = new Button
        {
            Text = "Refresh",
            Width = 100,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(104, 128, 105),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 1, 10, 2)
        };

        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += (_, _) => LoadRecords();

        toolbar.Controls.Add(searchBox);
        toolbar.Controls.Add(refreshButton);

        root.Controls.Add(toolbar, 0, 1);

        // Record count
        countLabel.Text = "0 records";
        countLabel.Dock = DockStyle.Fill;
        countLabel.ForeColor = mutedColor;
        countLabel.Font = new Font("Segoe UI", 9F);
        countLabel.TextAlign = ContentAlignment.MiddleLeft;

        root.Controls.Add(countLabel, 0, 2);

        // Records table
        ConfigureGrid();
        root.Controls.Add(recordsGrid, 0, 3);

        // Keep header text within the available width.
        root.Resize += (_, _) =>
        {
            int availableWidth = Math.Max(300, root.ClientSize.Width - 48);

            heading.Width = availableWidth;
            subtitle.Width = availableWidth;
        };
    }

    private void ConfigureGrid()
    {
        recordsGrid.Dock = DockStyle.Fill;
        recordsGrid.BackgroundColor = Color.White;
        recordsGrid.BorderStyle = BorderStyle.None;
        recordsGrid.GridColor = Color.FromArgb(230, 232, 225);

        recordsGrid.EnableHeadersVisualStyles = false;
        recordsGrid.ColumnHeadersHeight = 48;
        recordsGrid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        recordsGrid.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(236, 239, 231);

        recordsGrid.ColumnHeadersDefaultCellStyle.ForeColor = inkColor;

        recordsGrid.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 9F, FontStyle.Bold);

        recordsGrid.ColumnHeadersDefaultCellStyle.WrapMode =
            DataGridViewTriState.True;

        recordsGrid.DefaultCellStyle.Font =
            new Font("Segoe UI", 9F);

        recordsGrid.DefaultCellStyle.ForeColor = inkColor;

        recordsGrid.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(222, 232, 220);

        recordsGrid.DefaultCellStyle.SelectionForeColor = inkColor;

        recordsGrid.DefaultCellStyle.WrapMode =
            DataGridViewTriState.False;

        recordsGrid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(250, 250, 247);

        recordsGrid.RowHeadersVisible = false;
        recordsGrid.AllowUserToAddRows = false;
        recordsGrid.AllowUserToDeleteRows = false;
        recordsGrid.AllowUserToResizeColumns = true;
        recordsGrid.AllowUserToResizeRows = false;
        recordsGrid.AllowUserToOrderColumns = false;

        recordsGrid.AutoGenerateColumns = true;
        recordsGrid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.None;

        recordsGrid.AutoSizeRowsMode =
            DataGridViewAutoSizeRowsMode.None;

        recordsGrid.RowTemplate.Height = 36;
        recordsGrid.ScrollBars = ScrollBars.Both;
        recordsGrid.ReadOnly = true;

        recordsGrid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        recordsGrid.MultiSelect = false;
        recordsGrid.ShowCellToolTips = true;
        recordsGrid.CellToolTipTextNeeded += ShowCellTooltip;
        recordsGrid.CellFormatting += FormatDateCell;
    }

    private void LoadRecords()
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
                WHERE FullName LIKE $search
                   OR Cnic LIKE $search
                   OR ApplicationReference LIKE $search
                ORDER BY Id DESC;";

            command.Parameters.AddWithValue(
                "$search", "%" + searchBox.Text.Trim() + "%");

            var table = new DataTable();

            using var reader = command.ExecuteReader();
            table.Load(reader);

            recordsGrid.DataSource = table;

            SetColumn("ApplicationReference", "Application reference", 230);
            SetColumn("FullName", "Full name", 210);
            SetColumn("Cnic", "CNIC", 155);
            SetColumn("DateOfBirth", "Date of birth", 145);
            SetColumn("Gender", "Gender", 110);
            SetColumn("Address", "Address", 240);
            SetColumn("Phone", "Phone", 150);
            SetColumn("ApplicationStatus", "Status", 125);
            SetColumn("CreatedAt", "Date submitted", 190);

            countLabel.Text = table.Rows.Count == 1
                ? "1 record"
                : $"{table.Rows.Count} records";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not load citizen records.\n\n" + ex.Message,
                "CivicID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SetColumn(string name, string header, int width)
    {
        if (recordsGrid.Columns[name] is not DataGridViewColumn column)
            return;

        column.HeaderText = header;
        column.Width = width;
        column.MinimumWidth = 80;
        column.SortMode = DataGridViewColumnSortMode.NotSortable;
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
}
