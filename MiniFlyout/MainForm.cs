using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UI;

public class MainForm : Form
{
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

    private Point dragStart;

    public MainForm()
    {
        InitUI();
        EnableDrag();
    }

    private void InitUI()
    {
        Width = 280;
        Height = 70;
        TopMost = true;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.FromArgb(20, 20, 20);

        // Rounded corners
        Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        // Position near taskbar
        var screen = Screen.PrimaryScreen.WorkingArea;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(screen.Width - 300, screen.Height - 90);

        var panel = new FlowLayoutPanel()
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            Padding = new Padding(10),
        };

        var prev = new StyledButton("⏮");
        var play = new StyledButton("▶/⏸");
        var next = new StyledButton("⏭");

        prev.Click += (s, e) => MediaController.Previous();
        play.Click += (s, e) => MediaController.PlayPause();
        next.Click += (s, e) => MediaController.Next();

        panel.Controls.Add(prev);
        panel.Controls.Add(play);
        panel.Controls.Add(next);

        Controls.Add(panel);
    }

    private void EnableDrag()
    {
        MouseDown += (s, e) => dragStart = e.Location;

        MouseMove += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                Location = new Point(
                    Location.X + e.X - dragStart.X,
                    Location.Y + e.Y - dragStart.Y
                );
            }
        };
    }
}