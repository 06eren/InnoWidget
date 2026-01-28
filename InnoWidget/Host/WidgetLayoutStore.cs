using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace InnoWidget.Host;

public sealed class WidgetLayoutStore
{
    private readonly string _filePath;

    public WidgetLayoutStore(string appName = "InnoWidget")
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "widgets.json");
    }

    public IReadOnlyDictionary<string, WidgetSettings> Load()
    {
        if (!File.Exists(_filePath))
            return new Dictionary<string, WidgetSettings>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var json = File.ReadAllText(_filePath);
            var items = JsonSerializer.Deserialize<List<WidgetSettings>>(json) ?? new List<WidgetSettings>();
            return items.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, WidgetSettings>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Save(IEnumerable<WidgetSettings> settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
