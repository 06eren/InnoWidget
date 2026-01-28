namespace InnoWidget.Core.Services.Media;

public sealed record MediaSnapshot(
    string? SourceAppId,
    string? Title,
    string? Artist,
    bool? IsPlaying,
    byte[]? ThumbnailPng);
