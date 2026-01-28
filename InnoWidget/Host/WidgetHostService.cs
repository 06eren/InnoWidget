using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace InnoWidget.Host;

public sealed class WidgetHostService
{
    private readonly Dictionary<string, (WidgetDefinition def, WidgetWindow window)> _open = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _opacityById = new(StringComparer.OrdinalIgnoreCase);

    public bool IsOpen(string id) => _open.ContainsKey(id);

    public void Show(WidgetDefinition def, WidgetSettings settings)
    {
        if (_open.TryGetValue(def.Id, out var existing))
        {
            existing.window.Activate();
            return;
        }

        var vm = def.CreateViewModel();
        var w = new WidgetWindow
        {
            Title = def.Title,
            Width = settings.Width > 0 ? settings.Width : def.DefaultSize.Width,
            Height = settings.Height > 0 ? settings.Height : def.DefaultSize.Height,
            Left = settings.Left,
            Top = settings.Top,
            DataContext = vm
        };

        var bgOpacity = ClampOpacity(settings.Opacity);
        _opacityById[def.Id] = bgOpacity;
        w.SetBackgroundOpacity(bgOpacity);

        w.Closed += (_, _) =>
        {
            if (vm is IDisposable d)
                d.Dispose();
            _open.Remove(def.Id);
        };

        _open[def.Id] = (def, w);
        w.Show();
    }

    public void Close(string id)
    {
        if (_open.TryGetValue(id, out var entry))
            entry.window.Close();
    }

    public IReadOnlyList<WidgetSettings> CaptureLayout()
    {
        return _open.Select(x => new WidgetSettings
        {
            Id = x.Key,
            IsOpen = true,
            Left = x.Value.window.Left,
            Top = x.Value.window.Top,
            Width = x.Value.window.Width,
            Height = x.Value.window.Height,
            Opacity = _opacityById.TryGetValue(x.Key, out var op) ? op : 1.0
        }).ToList();
    }

    public void SetOpacity(string id, double opacity)
    {
        if (_open.TryGetValue(id, out var entry))
        {
            var bgOpacity = ClampOpacity(opacity);
            _opacityById[id] = bgOpacity;
            entry.window.SetBackgroundOpacity(bgOpacity);
        }
        else
        {
            _opacityById[id] = ClampOpacity(opacity);
        }
    }

    private static double ClampOpacity(double opacity)
    {
        if (double.IsNaN(opacity) || double.IsInfinity(opacity))
            return 1.0;

        return Math.Clamp(opacity, 0.2, 1.0);
    }

    public void CloseAll()
    {
        foreach (var id in _open.Keys.ToList())
            Close(id);
    }
}
