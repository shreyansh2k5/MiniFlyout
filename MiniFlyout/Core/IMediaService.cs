// File: Core/IMediaService.cs
using System.Threading.Tasks;

namespace MiniFlyout.Core
{
    public interface IMediaService
    {
        void PlayPause();
        void Next();
        void Previous();
        Task<TrackInfo?> GetCurrentTrackAsync();
    }
}