// File: Infrastructure/WindowsMediaService.cs
using System;
using System.Threading.Tasks;
using MiniFlyout.Core;
using Windows.Media.Control;

namespace MiniFlyout.Infrastructure
{
    public class WindowsMediaService : IMediaService
    {
        private void Press(byte key)
        {
            NativeMethods.keybd_event(key, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY, 0);
            NativeMethods.keybd_event(key, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, 0);
        }

        public void PlayPause() => Press(0xB3); // VK_MEDIA_PLAY_PAUSE
        public void Next() => Press(0xB0);      // VK_MEDIA_NEXT_TRACK
        public void Previous() => Press(0xB1);  // VK_MEDIA_PREV_TRACK

        public async Task<TrackInfo?> GetCurrentTrackAsync()
        {
            try
            {
                // Uses Windows 10/11 native media transport controls API
                var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = manager.GetCurrentSession();
                
                if (session != null)
                {
                    var mediaProperties = await session.TryGetMediaPropertiesAsync();
                    return new TrackInfo
                    {
                        Title = string.IsNullOrWhiteSpace(mediaProperties.Title) ? "Unknown Title" : mediaProperties.Title,
                        Artist = string.IsNullOrWhiteSpace(mediaProperties.Artist) ? "Unknown Artist" : mediaProperties.Artist
                    };
                }
            }
            catch (Exception)
            {
                // Fallback if API fails or is denied by OS privacy settings
            }
            return null;
        }
    }
}