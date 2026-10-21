
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace CivicID
{
    public class OpenCvStudioForm : Form
    {
        private readonly Color paper = Color.FromArgb(245, 243, 237);
        private readonly Color white = Color.FromArgb(251, 250, 247);
        private readonly Color ink = Color.FromArgb(52, 56, 50);
        private readonly Color muted = Color.FromArgb(116, 117, 107);
        private readonly Color sage = Color.FromArgb(120, 136, 117);
        private readonly Color line = Color.FromArgb(221, 219, 210);

        private readonly PictureBox pictureOriginal = new();
        private readonly PictureBox pictureProcessed = new();
        private readonly Label lblStatus = new();

        private Mat? originalImage;
        private Mat? processedImage;

        private TableLayoutPanel mainLayout = null!;
        private Panel header = null!;
        private Label title = null!;
        private Label description = null!;
        private Label eyebrow = null!;

        public OpenCvStudioForm()
        {
            Text = "CivicID — OpenCV Studio";
            Size = new Size(1100, 760);
            MinimumSize = new Size(850, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = paper;
            ForeColor = ink;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;

            BuildInterface();

            Resize += (_, _) => AdjustLayout();
            FormClosed += (_, _) => ReleaseImages();

            AdjustLayout();
        }

        private void BuildInterface()
        {
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 22, 28, 15),
                ColumnCount = 1,
                RowCount = 4,
                BackColor = paper
            };

            mainLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 115));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 66));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            mainLayout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 32));

            Controls.Add(mainLayout);

            // Header
            header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = paper
            };

            eyebrow = MakeLabel(
                "CITIZEN SERVICES  /  IMAGE PROCESSING",
                8.5F, FontStyle.Bold, muted);

            eyebrow.Location = new Point(0, 0);
            eyebrow.AutoSize = false;
            eyebrow.Size = new Size(700, 20);

            title = MakeLabel(
                "OpenCV Studio",
                26F, FontStyle.Regular, ink);

            title.Font = new Font("Georgia", 26F);
            title.Location = new Point(0, 23);
            title.AutoSize = false;
            title.Size = new Size(700, 40);

            description = MakeLabel(
                "A small computer vision laboratory.",
                10F, FontStyle.Regular, muted);

            description.Location = new Point(2, 70);
            description.AutoSize = false;
            description.Size = new Size(700, 26);

            header.Controls.Add(eyebrow);
            header.Controls.Add(title);
            header.Controls.Add(description);

            mainLayout.Controls.Add(header, 0, 0);

            // Toolbar
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = paper,
                Padding = new Padding(0, 5, 0, 5)
            };

            toolbar.Controls.Add(MakeButton(
                "Load image", true, LoadImage));

            toolbar.Controls.Add(MakeButton(
                "Grayscale", false, ApplyGrayscale));

            toolbar.Controls.Add(MakeButton(
                "Gaussian blur", false, ApplyBlur));

            toolbar.Controls.Add(MakeButton(
                "Edge detection", false, ApplyEdgeDetection));

            toolbar.Controls.Add(MakeButton(
                "Reset", false, ResetImage));

            toolbar.Controls.Add(MakeButton(
                "Save result", false, SaveResult));

            mainLayout.Controls.Add(toolbar, 0, 1);

            // Original and processed image previews
            var previews = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = paper,
                Padding = new Padding(0, 8, 0, 8)
            };

            previews.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            previews.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            previews.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            var originalPanel = CreatePreviewPanel(
                "01  /  ORIGINAL IMAGE", pictureOriginal);

            var processedPanel = CreatePreviewPanel(
                "02  /  PROCESSED OUTPUT", pictureProcessed);

            originalPanel.Margin = new Padding(0, 0, 8, 0);
            processedPanel.Margin = new Padding(8, 0, 0, 0);

            previews.Controls.Add(originalPanel, 0, 0);
            previews.Controls.Add(processedPanel, 1, 0);

            mainLayout.Controls.Add(previews, 0, 2);

            // Footer / status
            lblStatus.Text = "Ready. Load an image to begin.";
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = muted;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.AutoEllipsis = true;

            mainLayout.Controls.Add(lblStatus, 0, 3);
        }

        private void AdjustLayout()
        {
            if (mainLayout == null || header == null)
                return;

            int availableWidth = Math.Max(
                300, mainLayout.ClientSize.Width - 56);

            eyebrow.Width = availableWidth;
            title.Width = availableWidth;
            description.Width = availableWidth;
        }

        private Panel CreatePreviewPanel(
            string heading,
            PictureBox pictureBox)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = white,
                Padding = new Padding(12)
            };

            var titleLabel = MakeLabel(
                heading, 8.5F, FontStyle.Bold, muted);

            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 30;
            titleLabel.AutoSize = false;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            titleLabel.AutoEllipsis = true;

            pictureBox.Dock = DockStyle.Fill;
            pictureBox.BackColor = Color.FromArgb(238, 236, 229);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BorderStyle = BorderStyle.None;

            panel.Controls.Add(pictureBox);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Button MakeButton(
            string text,
            bool primary,
            Action action)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 38,
                Padding = new Padding(10, 0, 10, 0),
                Margin = new Padding(0, 0, 8, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? sage : white,
                ForeColor = primary ? Color.White : ink,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = line;
            button.Click += (_, _) => action();

            return button;
        }

        private Label MakeLabel(
            string text,
            float size,
            FontStyle style,
            Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent
            };
        }

        private void LoadImage()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Choose an image for OpenCV Studio",
                Filter =
                    "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|" +
                    "All files|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            Mat? loaded = null;

            try
            {
                loaded = Cv2.ImRead(
                    dialog.FileName, ImreadModes.Color);

                if (loaded.Empty())
                {
                    loaded.Dispose();

                    MessageBox.Show(
                        this,
                        "OpenCV could not read this image. Try a JPG or PNG file.",
                        "Unable to load image",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                ReleaseImages();

                originalImage = loaded;
                loaded = null;

                ShowBitmap(
                    pictureOriginal,
                    BitmapConverter.ToBitmap(originalImage));

                SetProcessedImage(originalImage.Clone());

                lblStatus.Text =
                    $"Loaded: {Path.GetFileName(dialog.FileName)}  ·  " +
                    $"{originalImage.Width} × {originalImage.Height}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "Image loading error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                loaded?.Dispose();
            }
        }

        private bool HasImage()
        {
            if (originalImage != null && !originalImage.Empty())
                return true;

            MessageBox.Show(
                this,
                "Load an image before applying an effect.",
                "No image loaded",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return false;
        }

        private void ApplyGrayscale()
        {
            if (!HasImage())
                return;

            try
            {
                var output = new Mat();

                Cv2.CvtColor(
                    originalImage!,
                    output,
                    ColorConversionCodes.BGR2GRAY);

                SetProcessedImage(output);

                lblStatus.Text =
                    "Applied grayscale conversion using OpenCV.";
            }
            catch (Exception ex)
            {
                ShowProcessingError(ex);
            }
        }

        private void ApplyBlur()
        {
            if (!HasImage())
                return;

            try
            {
                var output = new Mat();

                Cv2.GaussianBlur(
                    originalImage!,
                    output,
                    new OpenCvSharp.Size(15, 15),
                    0);

                SetProcessedImage(output);

                lblStatus.Text =
                    "Applied Gaussian blur using a 15 × 15 kernel.";
            }
            catch (Exception ex)
            {
                ShowProcessingError(ex);
            }
        }

        private void ApplyEdgeDetection()
        {
            if (!HasImage())
                return;

            try
            {
                using var gray = new Mat();
                var edges = new Mat();

                Cv2.CvtColor(
                    originalImage!,
                    gray,
                    ColorConversionCodes.BGR2GRAY);

                Cv2.Canny(gray, edges, 80, 160);

                SetProcessedImage(edges);

                lblStatus.Text =
                    "Applied Canny edge detection (thresholds: 80 and 160).";
            }
            catch (Exception ex)
            {
                ShowProcessingError(ex);
            }
        }

        private void ResetImage()
        {
            if (!HasImage())
                return;

            SetProcessedImage(originalImage!.Clone());

            lblStatus.Text =
                "Reset the output to the original image.";
        }

        private void SaveResult()
        {
            if (processedImage == null || processedImage.Empty())
            {
                MessageBox.Show(
                    this,
                    "Load an image before saving a result.",
                    "Nothing to save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Save processed image",
                Filter =
                    "PNG image|*.png|" +
                    "JPEG image|*.jpg|" +
                    "Bitmap image|*.bmp",
                FileName = "civicid-processed.png",
                DefaultExt = "png",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                if (!Cv2.ImWrite(dialog.FileName, processedImage))
                {
                    throw new InvalidOperationException(
                        "OpenCV could not write the output image.");
                }

                lblStatus.Text =
                    $"Saved result: {Path.GetFileName(dialog.FileName)}";

                MessageBox.Show(
                    this,
                    "The processed image was saved successfully.",
                    "Image saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowProcessingError(ex);
            }
        }

        private void SetProcessedImage(Mat image)
        {
            var oldImage = processedImage;
            processedImage = image;

            ShowBitmap(
                pictureProcessed,
                BitmapConverter.ToBitmap(processedImage));

            oldImage?.Dispose();
        }

        private void ShowBitmap(PictureBox box, Bitmap bitmap)
        {
            var previous = box.Image;
            box.Image = bitmap;
            previous?.Dispose();
        }

        private void ShowProcessingError(Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "OpenCV processing error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            lblStatus.Text =
                "The image operation could not be completed.";
        }

        private void ReleaseImages()
        {
            originalImage?.Dispose();
            originalImage = null;

            processedImage?.Dispose();
            processedImage = null;

            pictureOriginal.Image?.Dispose();
            pictureOriginal.Image = null;

            pictureProcessed.Image?.Dispose();
            pictureProcessed.Image = null;
        }
    }
}
