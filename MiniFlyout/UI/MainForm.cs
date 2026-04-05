// File: UI/MainForm.cs
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniFlyout.Core;
using MiniFlyout.Infrastructure;

namespace MiniFlyout.UI
{
    public class MainForm : Form
    {
        private readonly IMediaService _mediaService;
        private Point _dragStart;
        private System.Windows.Forms.Timer _topMostEnforcer = null!;
        private System.Windows.Forms.Timer _uiUpdater = null!;
        
        // UI Components
        private StyledButton _playBtn = null!;
        private Label _titleLabel = null!;
        private Label _artistLabel = null!;
        private PictureBox _thumbnailBox = null!;
        
        // State tracking
        private string _lastTitle = "";

        // Native Windows 11 API for smooth, built-in rounded corners
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public MainForm(IMediaService mediaService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            
            InitUI();
            EnableDrag();
            EnableTopMostEnforcer();
            EnableDynamicUpdater();
        }

        // Prevent the overlay from stealing focus when clicked or updated
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return cp;
            }
        }

        private void InitUI()
        {
            // Ultra-slim layout to fit perfectly INSIDE the Windows 11 taskbar
            Width = 340; 
            Height = 44; // Taskbar is 48px, this gives a sleek 2px margin top and bottom
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(10, 10, 10); 
            DoubleBuffered = true; 

            // Calculate precise center INSIDE the taskbar bounds
            var screen = Screen.PrimaryScreen!;
            var bounds = screen.Bounds;
            var workingArea = screen.WorkingArea;
            int taskbarHeight = bounds.Height - workingArea.Height;
            int yPos = taskbarHeight > 0 
                ? workingArea.Height + (taskbarHeight - Height) / 2 
                : bounds.Height - Height - 2;

            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, yPos); // Flush to the left edge

            // 1. Master Grid Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                RowCount = 1,
                ColumnCount = 3,
                Padding = new Padding(8, 4, 8, 4) // Tight, native-feeling padding
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36f)); // Thumbnail Area
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // Text Area
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96f)); // Buttons Area
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); 

            // 2. Thumbnail PictureBox
            _thumbnailBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 8, 0)
            };

            // 3. Text Container
            var textLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(0)
            };
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            _titleLabel = new Label
            {
                Text = "No Media",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 1) // Nudge to center visually
            };
            
            _artistLabel = new Label
            {
                Text = "Waiting for playback...",
                ForeColor = Color.FromArgb(180, 255, 255, 255),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = true,
                Margin = new Padding(0, 1, 0, 0)
            };
            textLayout.Controls.Add(_titleLabel, 0, 0);
            textLayout.Controls.Add(_artistLabel, 0, 1);

            // 4. Buttons Container
            var buttonLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Override button defaults to scale flawlessly into the new slim taskbar height
            var prev = new StyledButton("\uE892") { Anchor = AnchorStyles.None, Margin = new Padding(0), Size = new Size(28, 28), Font = new Font("Segoe Fluent Icons", 11f) };
            _playBtn = new StyledButton("\uE768") { Anchor = AnchorStyles.None, Margin = new Padding(0), Size = new Size(28, 28), Font = new Font("Segoe Fluent Icons", 11f) }; 
            var next = new StyledButton("\uE893") { Anchor = AnchorStyles.None, Margin = new Padding(0), Size = new Size(28, 28), Font = new Font("Segoe Fluent Icons", 11f) };

            prev.Click += (s, e) => _mediaService.Previous();
            _playBtn.Click += (s, e) => _mediaService.PlayPause();
            next.Click += (s, e) => _mediaService.Next();

            buttonLayout.Controls.Add(prev, 0, 0);
            buttonLayout.Controls.Add(_playBtn, 1, 0);
            buttonLayout.Controls.Add(next, 2, 0);

            mainLayout.Controls.Add(_thumbnailBox, 0, 0);
            mainLayout.Controls.Add(textLayout, 1, 0);
            mainLayout.Controls.Add(buttonLayout, 2, 0);

            Controls.Add(mainLayout);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyAcrylicBlur();

            // Apply Native Windows 11 Smooth Corners (DWMWCP_ROUNDSMALL = 3)
            // This replaces the jagged Region method with flawless native UI rendering
            int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
            int cornerPref = 3; 
            DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));
        }

        private void ApplyAcrylicBlur()
        {
            var accent = new NativeMethods.AccentPolicy
            {
                AccentState = NativeMethods.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                // Softened the darkness (0x40 instead of 0x99) so it blends seamlessly into the taskbar
                GradientColor = unchecked((int)0x40000000)
            };

            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new NativeMethods.WindowCompositionAttributeData
            {
                Attribute = NativeMethods.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            NativeMethods.SetWindowCompositionAttribute(this.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        private void EnableDrag()
        {
            MouseDown += HandleMouseDown;
            MouseMove += HandleMouseMove;
        }

        private void HandleMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _dragStart = e.Location;
        }

        private void HandleMouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Location = new Point(
                    Location.X + e.X - _dragStart.X,
                    Location.Y + e.Y - _dragStart.Y
                );
            }
        }

        private void EnableTopMostEnforcer()
        {
            _topMostEnforcer = new System.Windows.Forms.Timer { Interval = 2000 };
            _topMostEnforcer.Tick += (s, e) => 
            {
                const uint SWP_NOACTIVATE = 0x0010;
                NativeMethods.SetWindowPos(this.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | SWP_NOACTIVATE);
            };
            _topMostEnforcer.Start();
        }

        private void EnableDynamicUpdater()
        {
            _uiUpdater = new System.Windows.Forms.Timer { Interval = 1000 };
            _uiUpdater.Tick += async (s, e) => await UpdateUIStateAsync();
            _uiUpdater.Start();
            _ = UpdateUIStateAsync();
        }

        private async Task UpdateUIStateAsync()
        {
            var track = await _mediaService.GetCurrentTrackAsync();
            if (track != null)
            {
                string currentIcon = track.IsPlaying ? "\uE769" : "\uE768";
                if (_playBtn.Text != currentIcon) _playBtn.Text = currentIcon;

                if (_lastTitle != track.Title)
                {
                    _lastTitle = track.Title;
                    _titleLabel.Text = track.Title;
                    _artistLabel.Text = track.Artist;

                    if (track.ThumbnailData != null)
                    {
                        var oldImage = _thumbnailBox.Image;
                        var ms = new System.IO.MemoryStream(track.ThumbnailData);
                        _thumbnailBox.Image = Image.FromStream(ms);
                        oldImage?.Dispose();
                    }
                    else
                    {
                        _thumbnailBox.Image = null;
                    }
                }
            }
            else
            {
                _playBtn.Text = "\uE768";
                _titleLabel.Text = "No Media";
                _artistLabel.Text = "";
                _thumbnailBox.Image = null;
                _lastTitle = "";
            }
        }
    }
}