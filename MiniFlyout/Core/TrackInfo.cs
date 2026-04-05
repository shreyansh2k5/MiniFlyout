// File: Core/TrackInfo.cs
namespace MiniFlyout.Core
{
    public class TrackInfo
    {
        public string Title { get; set; } = "Unknown";
        public string Artist { get; set; } = "Unknown";
        
        // New properties for dynamic UI
        public bool IsPlaying { get; set; } = false;
        public byte[]? ThumbnailData { get; set; }
    }
}