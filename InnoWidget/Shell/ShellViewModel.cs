using System.Collections.ObjectModel;
using InnoWidget.Core.Mvvm;
using InnoWidget.Host;

namespace InnoWidget.Shell;

public sealed class ShellViewModel : ObservableObject
{
    public ObservableCollection<WidgetToggleItemViewModel> WidgetToggles { get; } = new();

    public ShellViewModel(params WidgetToggleItemViewModel[] widgetToggles)
    {
        foreach (var w in widgetToggles)
            WidgetToggles.Add(w);
    }
}
