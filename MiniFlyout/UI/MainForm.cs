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

        public MainForm(IMediaService mediaService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            
            InitUI();
            EnableDrag();
            EnableTopMostEnforcer();
            EnableDynamicUpdater();
        }

        private void InitUI()
        {
            // Wider, horizontal layout
            Width = 340; 
            Height = 64;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(15, 15, 15); 

            // Smooth rounded corners
            Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 18, 18));

            var screen = Screen.PrimaryScreen!;
            var bounds = screen.Bounds;
            var workingArea = screen.WorkingArea;
            int taskbarHeight = bounds.Height - workingArea.Height;
            int yPos = taskbarHeight > 0 
                ? workingArea.Height + (taskbarHeight - Height) / 2 
                : bounds.Height - Height - 5;

            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, yPos); // Leftmost edge

            // 1. Master Grid Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                RowCount = 1,
                ColumnCount = 3,
                Padding = new Padding(5)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54f)); // Thumbnail Area
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // Text Area
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f)); // Buttons Area

            // 2. Thumbnail PictureBox
            _thumbnailBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(25, 25, 25), // Slight highlight behind image
                Margin = new Padding(0, 0, 5, 0)
            };

            // 3. Text Container
            var textLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(0)
            };
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));

            _titleLabel = new Label
            {
                Text = "No Media",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true
            };
            
            _artistLabel = new Label
            {
                Text = "Waiting for playback...",
                ForeColor = Color.FromArgb(180, 255, 255, 255), // Light Gray
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                AutoEllipsis = true
            };
            textLayout.Controls.Add(_titleLabel, 0, 0);
            textLayout.Controls.Add(_artistLabel, 0, 1);

            // 4. Buttons Container
            var buttonLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };

            var prev = new StyledButton("\uE892");
            _playBtn = new StyledButton("\uE768"); // Default to Play
            var next = new StyledButton("\uE893");

            prev.Click += (s, e) => _mediaService.Previous();
            _playBtn.Click += (s, e) => _mediaService.PlayPause();
            next.Click += (s, e) => _mediaService.Next();

            buttonLayout.Controls.Add(prev);
            buttonLayout.Controls.Add(_playBtn);
            buttonLayout.Controls.Add(next);

            mainLayout.Controls.Add(_thumbnailBox, 0, 0);
            mainLayout.Controls.Add(textLayout, 1, 0);
            mainLayout.Controls.Add(buttonLayout, 2, 0);

            Controls.Add(mainLayout);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyAcrylicBlur();
        }

        private void ApplyAcrylicBlur()
        {
            var accent = new NativeMethods.AccentPolicy
            {
                AccentState = NativeMethods.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = unchecked((int)0x99000000)
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
            // Allow dragging from any empty space
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
                NativeMethods.SetWindowPos(this.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
            };
            _topMostEnforcer.Start();
        }

        private void EnableDynamicUpdater()
        {
            // Updates UI every 1 second continuously 
            _uiUpdater = new System.Windows.Forms.Timer { Interval = 1000 };
            _uiUpdater.Tick += async (s, e) => await UpdateUIStateAsync();
            _uiUpdater.Start();
            
            // Initial fetch
            _ = UpdateUIStateAsync();
        }

        private async Task UpdateUIStateAsync()
        {
            var track = await _mediaService.GetCurrentTrackAsync();
            if (track != null)
            {
                // Update Play/Pause Icon dynamically
                string currentIcon = track.IsPlaying ? "\uE769" : "\uE768";
                if (_playBtn.Text != currentIcon) _playBtn.Text = currentIcon;

                // Update text and image ONLY if the song changes to save memory
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