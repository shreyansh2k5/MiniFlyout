// File: UI/MainForm.cs
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniFlyout.Core;

namespace MiniFlyout.UI
{
    public class MainForm : Form
    {
        private readonly IMediaService _mediaService;
        private Point _dragStart;
        private System.Windows.Forms.Timer _topMostEnforcer = null!;
        private System.Windows.Forms.Timer _uiUpdater = null!;
        
        private StyledButton _playBtn = null!;
        private Label _titleLabel = null!;
        private Label _artistLabel = null!;
        private PictureBox _thumbnailBox = null!;
        
        private string _lastTitle = "";
        private bool _isVisible = true;

        public MainForm(IMediaService mediaService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            
            InitUI();
            EnableDrag();
            EnableTopMostEnforcer();
            EnableDynamicUpdater();
        }

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
            // 15% narrower and 5% taller
            Width = 290; 
            Height = 52;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(10, 10, 10); 
            DoubleBuffered = true; 

            var screen = Screen.PrimaryScreen!;
            var bounds = screen.Bounds;
            var workingArea = screen.WorkingArea;
            int taskbarHeight = bounds.Height - workingArea.Height;
            int yPos = taskbarHeight > 0 
                ? workingArea.Height + (taskbarHeight - Height) / 2 
                : bounds.Height - Height - 2;

            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, yPos);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                RowCount = 1,
                ColumnCount = 3,
                // Reduced top/bottom padding to maximize vertical space for the thumbnail
                Padding = new Padding(8, 2, 8, 2)
            };
            
            // Increased width from 36f to 50f so the thumbnail isn't horizontally squished
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50f)); 
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); 
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86f)); // Scaled down for smaller width
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); 

            _thumbnailBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 8, 0)
            };

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
                Margin = new Padding(0, 0, 0, 1)
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

            // Scaled buttons down slightly (26x26) so they don't clip in the narrower UI
            var prev = new StyledButton("\uE892") { Anchor = AnchorStyles.None, Margin = new Padding(0), Size = new Size(26, 26), Font = new Font("Segoe Fluent Icons", 10.5f) };
            _playBtn = new StyledButton("\uE768") { Anchor = AnchorStyles.None, Margin = new Padding(0), Size = new Size(26, 26), Font = new Font("Segoe Fluent Icons", 10.5f) }; 
            var next = new StyledButton("\uE893") { Anchor = AnchorStyles.None, Margin = new Padding(0), Size = new Size(26, 26), Font = new Font("Segoe Fluent Icons", 10.5f) };

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
            WindowEffects.ApplyNativeRoundedCorners(this.Handle);
            WindowEffects.EnableAmbientAcrylic(this.Handle, Color.Black, 0x40);
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
                Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
        }

        private void EnableTopMostEnforcer()
        {
            _topMostEnforcer = new System.Windows.Forms.Timer { Interval = 2000 };
            _topMostEnforcer.Tick += (s, e) => WindowEffects.EnforceTopMost(this.Handle);
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
                // OPTIMIZATION: Check frequently if actively playing, check slowly if paused
                _uiUpdater.Interval = track.IsPlaying ? 1000 : 3000;

                // Make sure widget is visible
                if (!_isVisible)
                {
                    WindowEffects.ShowOverlay(this.Handle);
                    _isVisible = true;
                }

                string currentIcon = track.IsPlaying ? "\uE769" : "\uE768";
                if (_playBtn.Text != currentIcon) _playBtn.Text = currentIcon;

                // Only perform heavy UI/Image work if the track actually changes
                if (_lastTitle != track.Title)
                {
                    _lastTitle = track.Title;
                    _titleLabel.Text = track.Title;
                    _artistLabel.Text = track.Artist;

                    if (track.ThumbnailData != null)
                    {
                        using (var ms = new System.IO.MemoryStream(track.ThumbnailData))
                        using (var tempImage = Image.FromStream(ms))
                        {
                            var oldImage = _thumbnailBox.Image;
                            
                            // OPTIMIZATION: Clone the image so the memory stream can safely close
                            _thumbnailBox.Image = new Bitmap(tempImage); 
                            oldImage?.Dispose(); // Free memory of previous artwork instantly

                            // Generate the ambient background tint
                            var ambientColor = ImageUtils.GetAmbientGlowColor(_thumbnailBox.Image);
                            
                            // Boosted opacity to 0x99 (approx 60%) so the glow is heavily visible
                            WindowEffects.EnableAmbientAcrylic(this.Handle, ambientColor, 0x99); 
                        }
                    }
                    else
                    {
                        ClearMediaData();
                    }
                }
            }
            else
            {
                // No active media session
                _uiUpdater.Interval = 3000; // Drop CPU usage to minimum
                
                if (_isVisible)
                {
                    WindowEffects.HideOverlay(this.Handle); // Hide smoothly without breaking focus
                    _isVisible = false;
                    ClearMediaData();
                }
            }
        }

        private void ClearMediaData()
        {
            _lastTitle = "";
            _playBtn.Text = "\uE768";
            _titleLabel.Text = "No Media";
            _artistLabel.Text = "";
            
            _thumbnailBox.Image?.Dispose();
            _thumbnailBox.Image = null;
            
            WindowEffects.EnableAmbientAcrylic(this.Handle, Color.Black, 0x40);
        }
    }
}