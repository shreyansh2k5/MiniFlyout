// File: UI/StyledButton.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniFlyout.UI
{
    public class StyledButton : Button
    {
        public StyledButton(string fluentIcon)
        {
            // Use Windows 11 native icons
            Font = new Font("Segoe Fluent Icons", 14f, FontStyle.Regular);
            Text = fluentIcon;
            
            Size = new Size(35, 35);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            Margin = new Padding(2);

            // Subtle hover effect mimicking native behavior
            MouseEnter += (s, e) => BackColor = Color.FromArgb(40, 255, 255, 255);
            MouseLeave += (s, e) => BackColor = Color.Transparent;
        }
    }
}