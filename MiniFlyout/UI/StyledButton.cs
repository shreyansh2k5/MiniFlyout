using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    public class StyledButton : Button
    {
        public StyledButton(string text)
        {
            Text = text;
            Width = 60;
            Height = 40;
            FlatStyle = FlatStyle.Flat;
            ForeColor = Color.White;
            BackColor = Color.FromArgb(30, 30, 30);
            FlatAppearance.BorderSize = 0;

            MouseEnter += (s, e) => BackColor = Color.FromArgb(50, 50, 50);
            MouseLeave += (s, e) => BackColor = Color.FromArgb(30, 30, 30);
        }
    }
}