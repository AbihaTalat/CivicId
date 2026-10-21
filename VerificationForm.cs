
using System;
using System.Drawing;
using System.Windows.Forms;
using CivicID.Data;

namespace CivicID;

public class VerificationForm : Form
{
    private readonly TextBox cnicBox = new();
    private readonly Label messageLabel = new();
    private readonly TableLayoutPanel resultLayout = new();
    private readonly ToolTip valueToolTip = new();

    private readonly Color backgroundColor = Color.FromArgb(247, 246, 241);
    private readonly Color inkColor = Color.FromArgb(42, 48, 43);
    private readonly Color mutedColor = Color.FromArgb(108, 116, 107);
    private readonly Color accentColor = Color.FromArgb(104, 128, 105);

    private Panel page = null!;
    private Panel searchPanel = null!;
    private Panel resultsPanel = null!;
    private Panel resultsViewport = null!;
    private Label heading = null!;
    private Label subtitle = null!;
    private Label resultsHeading = null!;
    private Label disclaimer = null!;

    public VerificationForm()
    {
        Text = "CivicID | Verification";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(850, 700);
        MinimumSize = new Size(620, 480);
        BackColor = backgroundColor;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildInterface();
        Resize += (_, _) => AdjustLayout();
        AdjustLayout();
    }

    private void BuildInterface()
    {
        AutoScroll = true;
        AutoScrollMinSize = new Size(0, 820);

        page = new Panel
        {
            Location = Point.Empty,
            Size = new Size(780, 820),
            BackColor = backgroundColor,
            Padding = new Padding(24)
        };

        Controls.Add(page);

        heading = new Label
        {
            Text = "Identity verification",
            Font = new Font("Georgia", 23F),
            ForeColor = inkColor,
            Location = new Point(24, 22),
            Size = new Size(700, 44),
            AutoSize = false
        };

        page.Controls.Add(heading);

        subtitle = new Label
        {
            Text = "Check an application using its registered CNIC.",
            Font = new Font("Segoe UI", 10F),
            ForeColor = mutedColor,
            Location = new Point(26, 72),
            Size = new Size(690, 28),
            AutoSize = false
        };

        page.Controls.Add(subtitle);

        searchPanel = new Panel
        {
            Location = new Point(24, 120),
            Size = new Size(700, 175),
            BackColor = Color.White,
            Padding = new Padding(18)
        };

        page.Controls.Add(searchPanel);

        var cnicLabel = new Label
        {
            Text = "CNIC number",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = inkColor,
            Location = new Point(18, 16),
            Size = new Size(650, 25)
        };

        searchPanel.Controls.Add(cnicLabel);

        cnicBox.Location = new Point(18, 48);
        cnicBox.Size = new Size(450, 35);
        cnicBox.Font = new Font("Segoe UI", 11F);
        cnicBox.MaxLength = 15;
        cnicBox.PlaceholderText = "e.g. 12345-1234567-1";
        cnicBox.Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right;

        cnicBox.TextChanged += (_, _) => messageLabel.Text = "";
        searchPanel.Controls.Add(cnicBox);

        var verifyButton = new Button
        {
            Text = "Verify identity",
            Location = new Point(18, 96),
            Size = new Size(160, 38),
            BackColor = accentColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        verifyButton.FlatAppearance.BorderSize = 0;
        verifyButton.Click += (_, _) => VerifyIdentity();
        searchPanel.Controls.Add(verifyButton);

        var clearButton = new Button
        {
            Text = "Clear",
            Location = new Point(190, 96),
            Size = new Size(95, 38),
            BackColor = Color.FromArgb(236, 239, 231),
            ForeColor = inkColor,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        clearButton.FlatAppearance.BorderSize = 0;
        clearButton.Click += (_, _) => ClearResults();
        searchPanel.Controls.Add(clearButton);

        messageLabel.Location = new Point(24, 310);
        messageLabel.Size = new Size(700, 36);
        messageLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        messageLabel.ForeColor = mutedColor;
        messageLabel.AutoSize = false;
        page.Controls.Add(messageLabel);

        resultsHeading = new Label
        {
            Text = "Verification details",
            Font = new Font("Georgia", 17F),
            ForeColor = inkColor,
            Location = new Point(24, 356),
            Size = new Size(700, 40),
            AutoSize = false
        };

        page.Controls.Add(resultsHeading);

        resultsViewport = new Panel
        {
            Location = new Point(24, 402),
            Size = new Size(700, 260),
            BackColor = Color.White,
            AutoScroll = true,
            BorderStyle = BorderStyle.None
        };

        page.Controls.Add(resultsViewport);

        resultsPanel = new Panel
        {
            Location = Point.Empty,
            Size = new Size(660, 260),
            BackColor = Color.White,
            Padding = new Padding(18)
        };

        resultsViewport.Controls.Add(resultsPanel);

        resultLayout.Dock = DockStyle.Top;
        resultLayout.AutoSize = true;
        resultLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        resultLayout.ColumnCount = 2;
        resultLayout.RowCount = 7;
        resultLayout.BackColor = Color.White;
        resultLayout.Margin = Padding.Empty;
        resultLayout.Padding = Padding.Empty;

        resultLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 190));
        resultLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < 7; i++)
        {
            resultLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 34));
        }

        resultsPanel.Controls.Add(resultLayout);

        string[] labels =
        {
            "Application reference",
            "Full name",
            "CNIC",
            "Date of birth",
            "Gender",
            "Application status",
            "Date submitted"
        };

        for (int i = 0; i < labels.Length; i++)
        {
            var fieldLabel = new Label
            {
                Text = labels[i],
                Dock = DockStyle.Fill,
                ForeColor = mutedColor,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            var valueLabel = new Label
            {
                Name = "resultValue" + i,
                Text = "—",
                Dock = DockStyle.Fill,
                ForeColor = inkColor,
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                AutoSize = false
            };

            resultLayout.Controls.Add(fieldLabel, 0, i);
            resultLayout.Controls.Add(valueLabel, 1, i);
        }

        disclaimer = new Label
        {
            Text = "Academic prototype only. Verification checks records in the local application database; it is not connected to NADRA.",
            Location = new Point(24, 678),
            Size = new Size(700, 48),
            ForeColor = mutedColor,
            Font = new Font("Segoe UI", 9F),
            AutoSize = false
        };

        page.Controls.Add(disclaimer);
    }

    private void AdjustLayout()
    {
        if (page == null || IsDisposed)
            return;

        int width = Math.Max(580, ClientSize.Width - 40);

        page.Width = width;
        page.Height = 820;

        int innerWidth = width - 70;

        heading.Width = innerWidth;
        subtitle.Width = innerWidth;
        searchPanel.Width = innerWidth;
        messageLabel.Width = innerWidth;
        resultsHeading.Width = innerWidth;
        resultsViewport.Width = innerWidth;
        disclaimer.Width = innerWidth;

        cnicBox.Width = Math.Max(250, searchPanel.ClientSize.Width - 45);

        resultsPanel.Width = Math.Max(
            resultsViewport.ClientSize.Width - 20,
            620);

        resultLayout.Width = resultsPanel.ClientSize.Width - 36;
        resultLayout.ColumnStyles[1].SizeType = SizeType.Percent;
        resultLayout.ColumnStyles[1].Width = 100F;

        resultsViewport.Height = Math.Max(
            170,
            ClientSize.Height - 480);
    }

    private void VerifyIdentity()
    {
        string cnic = cnicBox.Text.Trim()
            .Replace("-", "")
            .Replace(" ", "");

        if (cnic.Length != 13 || !IsDigitsOnly(cnic))
        {
            messageLabel.ForeColor = Color.FromArgb(170, 65, 55);
            messageLabel.Text = "Enter a valid 13-digit CNIC number.";
            ClearDetailValues();
            return;
        }

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
                    ApplicationStatus,
                    CreatedAt
                FROM Citizens
                WHERE REPLACE(REPLACE(Cnic, '-', ''), ' ', '') = $cnic
                LIMIT 1;";

            command.Parameters.AddWithValue("$cnic", cnic);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                ClearDetailValues();
                messageLabel.ForeColor = Color.FromArgb(170, 65, 55);
                messageLabel.Text =
                    "No matching record was found in the local database.";
                return;
            }

            SetValue(0, reader["ApplicationReference"]?.ToString() ?? "—");
            SetValue(1, reader["FullName"]?.ToString() ?? "—");
            SetValue(2, reader["Cnic"]?.ToString() ?? "—");
            SetValue(3, FormatDate(reader["DateOfBirth"], false));
            SetValue(4, reader["Gender"]?.ToString() ?? "—");
            SetValue(5, reader["ApplicationStatus"]?.ToString() ?? "—");
            SetValue(6, FormatDate(reader["CreatedAt"], true));

            messageLabel.ForeColor = accentColor;
            messageLabel.Text = "A matching record was found.";
        }
        catch (Exception ex)
        {
            ClearDetailValues();
            messageLabel.ForeColor = Color.FromArgb(170, 65, 55);
            messageLabel.Text = "Verification could not be completed.";

            MessageBox.Show(
                "An error occurred while checking the record.\n\n" + ex.Message,
                "CivicID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static bool IsDigitsOnly(string value)
    {
        foreach (char character in value)
        {
            if (!char.IsDigit(character))
                return false;
        }

        return true;
    }

    private static string FormatDate(object value, bool includeTime)
    {
        if (value == DBNull.Value ||
            !DateTime.TryParse(value.ToString(), out DateTime date))
            return "—";

        return includeTime
            ? date.ToString("dd MMM yyyy, HH:mm")
            : date.ToString("dd MMM yyyy");
    }

    private void SetValue(int index, string value)
    {
        foreach (Control control in resultLayout.Controls)
        {
            if (control is Label label &&
                label.Name == "resultValue" + index)
            {
                label.Text = value;
                label.Tag = value;
                label.AutoEllipsis = true;

                valueToolTip.SetToolTip(label, value);
                break;
            }
        }
    }

    private void ClearDetailValues()
    {
        for (int i = 0; i < 7; i++)
            SetValue(i, "—");
    }

    private void ClearResults()
    {
        cnicBox.Clear();
        messageLabel.Text = "";
        ClearDetailValues();
        cnicBox.Focus();
    }
}
