using System;
using System.Windows;
using Size = System.Windows.Size;

namespace InnoWidget.Host;

public sealed class WidgetDefinition
{
    public string Id { get; }
    public string Title { get; }
    public Size DefaultSize { get; }
    public Func<object> CreateViewModel { get; }

    public WidgetDefinition(string id, string title, Size defaultSize, Func<object> createViewModel)
    {
        Id = id;
        Title = title;
        DefaultSize = defaultSize;
        CreateViewModel = createViewModel;
    }
}
