// File: UI/ImageUtils.cs
using System.Drawing;

namespace MiniFlyout.UI
{
    public static class ImageUtils
    {
        public static Color GetAmbientGlowColor(Image? image)
        {
            if (image == null) return Color.Black;

            try
            {
                // Resize the image to 1x1 pixel to instantly get the average color mathematically
                using var bitmap = new Bitmap(1, 1);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, 1, 1);
                var dominantColor = bitmap.GetPixel(0, 0);

                // Boost the brightness so the ambient glow is vibrant and visible like YouTube
                // (Increased from 0.25 to 0.75 multiplier)
                int r = Math.Min((int)(dominantColor.R * 0.75), 255);
                int g = Math.Min((int)(dominantColor.G * 0.75), 255);
                int b = Math.Min((int)(dominantColor.B * 0.75), 255);

                return Color.FromArgb(r, g, b);
            }
            catch
            {
                return Color.Black;
            }
        }
    }
}