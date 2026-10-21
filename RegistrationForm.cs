
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using CivicID.Data;

namespace CivicID
{
    public class RegistrationForm : Form
    {
        private readonly TextBox txtCnic = new();
        private readonly TextBox txtName = new();
        private readonly TextBox txtAddress = new();
        private readonly TextBox txtPhone = new();

        private readonly ComboBox cmbGender = new();
        private readonly DateTimePicker dtpDob = new();

        private readonly ErrorProvider errors = new();

        private readonly Color paper = Color.FromArgb(245, 243, 237);
        private readonly Color ink = Color.FromArgb(52, 56, 50);
        private readonly Color sage = Color.FromArgb(120, 136, 117);

        public RegistrationForm()
        {
            Text = "CivicID — Citizen Registration";
            Size = new Size(850, 760);
            MinimumSize = new Size(760, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = paper;
            ForeColor = ink;
            Font = new Font("Segoe UI", 10);
            AutoScroll = true;

            errors.ContainerControl = this;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            BuildForm();
        }

        private void BuildForm()
        {
            var heading = new Label
            {
                Text = "Citizen registration",
                Font = new Font("Georgia", 24),
                AutoSize = true,
                Location = new Point(36, 28)
            };

            var subtitle = new Label
            {
                Text = "Enter the applicant's details below.",
                ForeColor = Color.FromArgb(116, 117, 107),
                AutoSize = true,
                Location = new Point(39, 76)
            };

            Controls.Add(heading);
            Controls.Add(subtitle);

            var identityGroup = new GroupBox
            {
                Text = "Identity details",
                Location = new Point(36, 120),
                Size = new Size(760, 205),
                ForeColor = ink
            };

            var contactGroup = new GroupBox
            {
                Text = "Contact details",
                Location = new Point(36, 345),
                Size = new Size(760, 175),
                ForeColor = ink
            };

            Controls.Add(identityGroup);
            Controls.Add(contactGroup);

            AddField(identityGroup, "Full name", txtName, 24, 32, 320);
            AddField(identityGroup, "CNIC (13 digits)", txtCnic, 390, 32, 320);

            AddField(identityGroup, "Date of birth", dtpDob, 24, 112, 320);
            AddField(identityGroup, "Gender", cmbGender, 390, 112, 320);

            dtpDob.Format = DateTimePickerFormat.Custom;
            dtpDob.CustomFormat = "dd MMM yyyy";
            dtpDob.MaxDate = DateTime.Today;
            dtpDob.Value = new DateTime(2000, 1, 1);

            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.Items.AddRange(
                new object[]
                {
                    "Female",
                    "Male",
                    "Other",
                    "Prefer not to say"
                }
            );
            cmbGender.SelectedIndex = 0;

            AddField(contactGroup, "Phone number", txtPhone, 24, 32, 320);
            AddField(contactGroup, "Residential address", txtAddress, 390, 32, 320);

            txtCnic.MaxLength = 15;
            txtPhone.MaxLength = 20;
            txtName.MaxLength = 100;
            txtAddress.MaxLength = 250;

            var note = new Label
            {
                Text = "Use fictional details for testing. This is a student-built prototype.",
                AutoSize = true,
                ForeColor = Color.FromArgb(116, 117, 107),
                Location = new Point(39, 540)
            };

            var saveButton = new Button
            {
                Text = "Save application",
                Location = new Point(36, 580),
                Size = new Size(180, 44),
                BackColor = sage,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveApplication;

            var clearButton = new Button
            {
                Text = "Clear form",
                Location = new Point(230, 580),
                Size = new Size(130, 44),
                BackColor = paper,
                ForeColor = ink,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            clearButton.Click += (_, _) => ClearForm();

            Controls.Add(note);
            Controls.Add(saveButton);
            Controls.Add(clearButton);

            AcceptButton = saveButton;
        }

        private void AddField(
            Control parent,
            string labelText,
            Control input,
            int x,
            int y,
            int width)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = ink
            };

            input.Location = new Point(x, y + 25);
            input.Size = new Size(width, 32);
            input.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            input.Font = new Font("Segoe UI", 10);

            if (input is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }

            parent.Controls.Add(label);
            parent.Controls.Add(input);
        }

        private void SaveApplication(object? sender, EventArgs e)
        {
            errors.Clear();

            string name = txtName.Text.Trim();
            string rawCnic = txtCnic.Text.Trim();
            string cnic = rawCnic.Replace("-", "").Replace(" ", "");
            string phone = txtPhone.Text.Trim();
            string address = txtAddress.Text.Trim();

            bool valid = true;

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.SetError(
                    txtName,
                    "Enter the applicant's full name."
                );
                valid = false;
            }

            if (cnic.Length != 13 || !long.TryParse(cnic, out _))
            {
                errors.SetError(
                    txtCnic,
                    "Enter exactly 13 digits."
                );
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                errors.SetError(
                    txtPhone,
                    "Enter a phone number."
                );
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                errors.SetError(
                    txtAddress,
                    "Enter a residential address."
                );
                valid = false;
            }

            if (dtpDob.Value.Date > DateTime.Today)
            {
                errors.SetError(
                    dtpDob,
                    "Date of birth cannot be in the future."
                );
                valid = false;
            }

            if (!valid)
            {
                MessageBox.Show(
                    "Please correct the highlighted fields.",
                    "Check your entries",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            string reference = "CID-" +
                Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            try
            {
                using var connection = DatabaseService.GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = """
                    INSERT INTO Citizens
                    (
                        ApplicationReference,
                        Cnic,
                        FullName,
                        DateOfBirth,
                        Gender,
                        Address,
                        Phone,
                        ApplicationStatus,
                        CreatedAt
                    )
                    VALUES
                    (
                        $reference,
                        $cnic,
                        $name,
                        $dob,
                        $gender,
                        $address,
                        $phone,
                        $status,
                        $createdAt
                    );
                    """;

                command.Parameters.AddWithValue("$reference", reference);
                command.Parameters.AddWithValue("$cnic", cnic);
                command.Parameters.AddWithValue("$name", name);

                command.Parameters.AddWithValue(
                    "$dob",
                    dtpDob.Value.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture
                    )
                );

                command.Parameters.AddWithValue(
                    "$gender",
                    cmbGender.SelectedItem?.ToString() ?? "Other"
                );

                command.Parameters.AddWithValue("$address", address);
                command.Parameters.AddWithValue("$phone", phone);
                command.Parameters.AddWithValue("$status", "Pending");

                command.Parameters.AddWithValue(
                    "$createdAt",
                    DateTime.Now.ToString(
                        "s",
                        CultureInfo.InvariantCulture
                    )
                );

                command.ExecuteNonQuery();

                MessageBox.Show(
                    $"Application saved successfully!\n\nReference: {reference}",
                    "Registration complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ClearForm();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show(
                    "This CNIC already exists in the database.",
                    "Duplicate record",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not save the application.\n\n{ex.Message}",
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtCnic.Clear();
            txtPhone.Clear();
            txtAddress.Clear();

            cmbGender.SelectedIndex = 0;
            dtpDob.Value = new DateTime(2000, 1, 1);

            errors.Clear();
            txtName.Focus();
        }
    }
}
