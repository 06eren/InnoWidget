using System;
using System.Threading.Tasks;

namespace InnoWidget.Core.Services.Media;

public interface IMediaSessionService : IDisposable
{
    event EventHandler? MediaChanged;

    Task InitializeAsync();

    Task<MediaSnapshot?> GetCurrentAsync();

    Task PlayPauseAsync();
    Task NextAsync();
    Task PreviousAsync();
}
