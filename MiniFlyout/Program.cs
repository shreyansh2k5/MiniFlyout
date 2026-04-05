// File: Program.cs
using System;
using System.Windows.Forms;
using MiniFlyout.Core;
using MiniFlyout.Infrastructure;
using MiniFlyout.UI;

namespace MiniFlyout
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Composition Root: Setup Dependency Injection manually
            IMediaService mediaService = new WindowsMediaService();
            
            // Pass the service into the UI layer
            var mainForm = new MainForm(mediaService);

            Application.Run(mainForm);
        }
    }
}