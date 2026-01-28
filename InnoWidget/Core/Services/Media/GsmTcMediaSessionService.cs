using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace InnoWidget.Core.Services.Media;

public sealed class GsmTcMediaSessionService : IMediaSessionService
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private GlobalSystemMediaTransportControlsSession? _session;

    public event EventHandler? MediaChanged;

    public async Task InitializeAsync()
    {
        _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        _manager.CurrentSessionChanged += Manager_CurrentSessionChanged;
        AttachSession(_manager.GetCurrentSession());
    }

    public async Task<MediaSnapshot?> GetCurrentAsync()
    {
        var s = _session;
        if (s is null)
            return null;

        var mediaProps = await s.TryGetMediaPropertiesAsync();
        var playback = s.GetPlaybackInfo();
        bool? isPlaying = null;
        if (playback is not null)
        {
            if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                isPlaying = true;
            else if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                isPlaying = false;
        }

        byte[]? png = null;
        try
        {
            if (mediaProps?.Thumbnail is not null)
                png = await ReadThumbnailAsync(mediaProps.Thumbnail);
        }
        catch
        {
        }

        return new MediaSnapshot(
            SourceAppId: s.SourceAppUserModelId,
            Title: mediaProps?.Title,
            Artist: mediaProps?.Artist,
            IsPlaying: isPlaying,
            ThumbnailPng: png);
    }

    public Task PlayPauseAsync() => _session?.TryTogglePlayPauseAsync().AsTask() ?? Task.CompletedTask;
    public Task NextAsync() => _session?.TrySkipNextAsync().AsTask() ?? Task.CompletedTask;
    public Task PreviousAsync() => _session?.TrySkipPreviousAsync().AsTask() ?? Task.CompletedTask;

    private void Manager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
    {
        AttachSession(sender.GetCurrentSession());
        MediaChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AttachSession(GlobalSystemMediaTransportControlsSession? newSession)
    {
        if (_session is not null)
        {
            _session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
            _session.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
        }

        _session = newSession;

        if (_session is not null)
        {
            _session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
            _session.PlaybackInfoChanged += Session_PlaybackInfoChanged;
        }
    }

    private void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        => MediaChanged?.Invoke(this, EventArgs.Empty);

    private void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        => MediaChanged?.Invoke(this, EventArgs.Empty);

    private static async Task<byte[]?> ReadThumbnailAsync(IRandomAccessStreamReference thumbnail)
    {
        using var stream = await thumbnail.OpenReadAsync();
        using var ms = new MemoryStream();
        using var s = stream.AsStreamForRead();
        await s.CopyToAsync(ms);
        return ms.ToArray();
    }

    public void Dispose()
    {
        if (_manager is not null)
            _manager.CurrentSessionChanged -= Manager_CurrentSessionChanged;

        if (_session is not null)
        {
            _session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
            _session.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
        }

        _session = null;
        _manager = null;
    }
}
