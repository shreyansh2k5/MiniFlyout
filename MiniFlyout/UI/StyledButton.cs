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
            Font = new Font("Segoe Fluent Icons", 14f, FontStyle.Regular);
            Text = fluentIcon;
            
            Size = new Size(32, 32);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            
            // Centered vertically relative to the new layout
            Margin = new Padding(2, 11, 2, 0); 

            // Crisp Native Hover Effect (Solid White Background, Black Text)
            MouseEnter += (s, e) => 
            {
                BackColor = Color.White;
                ForeColor = Color.Black;
            };
            
            MouseLeave += (s, e) => 
            {
                BackColor = Color.Transparent;
                ForeColor = Color.White;
            };
        }
    }
}