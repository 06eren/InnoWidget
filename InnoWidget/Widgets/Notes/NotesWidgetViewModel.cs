using InnoWidget.Core.Mvvm;

namespace InnoWidget.Widgets.Notes;

public sealed class NotesWidgetViewModel : ObservableObject
{
    private string _text = "";
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public string Title { get; } = "Hızlı Not / To-Do";
}
