using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using InnoWidget.Core.Mvvm;
using InnoWidget.Core.Services.Media;

namespace InnoWidget.Widgets.Media;

public sealed class MediaWidgetViewModel : ObservableObject, IDisposable
{
    private readonly IMediaSessionService _media;

    public string Title { get; } = "Spotify/Medya Kontrolcü";

    private string _trackTitle = "";
    public string TrackTitle
    {
        get => _trackTitle;
        private set => SetProperty(ref _trackTitle, value);
    }

    private string _artist = "";
    public string Artist
    {
        get => _artist;
        private set => SetProperty(ref _artist, value);
    }

    private bool _hasMedia;
    public bool HasMedia
    {
        get => _hasMedia;
        private set => SetProperty(ref _hasMedia, value);
    }

    private BitmapSource? _cover;
    public BitmapSource? Cover
    {
        get => _cover;
        private set => SetProperty(ref _cover, value);
    }

    private bool? _isPlaying;
    public bool? IsPlaying
    {
        get => _isPlaying;
        private set => SetProperty(ref _isPlaying, value);
    }

    public RelayCommand PlayPauseCommand { get; }
    public RelayCommand NextCommand { get; }
    public RelayCommand PrevCommand { get; }

    public MediaWidgetViewModel(IMediaSessionService media)
    {
        _media = media;

        PlayPauseCommand = new RelayCommand(() => _ = _media.PlayPauseAsync());
        NextCommand = new RelayCommand(() => _ = _media.NextAsync());
        PrevCommand = new RelayCommand(() => _ = _media.PreviousAsync());

        _media.MediaChanged += Media_MediaChanged;
        _ = InitializeAndRefreshAsync();
    }

    private void Media_MediaChanged(object? sender, EventArgs e)
    {
        _ = RefreshAsync();
    }

    private async Task InitializeAndRefreshAsync()
    {
        await _media.InitializeAsync().ConfigureAwait(true);
        await RefreshAsync().ConfigureAwait(true);
    }

    private async Task RefreshAsync()
    {
        var snap = await _media.GetCurrentAsync().ConfigureAwait(true);
        if (snap is null)
        {
            HasMedia = false;
            TrackTitle = "";
            Artist = "";
            Cover = null;
            IsPlaying = null;
            return;
        }

        HasMedia = true;
        TrackTitle = snap.Title ?? "";
        Artist = snap.Artist ?? "";
        IsPlaying = snap.IsPlaying;

        Cover = snap.ThumbnailPng is null ? null : PngToBitmapSource(snap.ThumbnailPng);
    }

    private static BitmapSource PngToBitmapSource(byte[] png)
    {
        using var ms = new MemoryStream(png);
        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.StreamSource = ms;
        img.EndInit();
        img.Freeze();
        return img;
    }

    public void Dispose()
    {
        _media.MediaChanged -= Media_MediaChanged;
        _media.Dispose();
    }
}
