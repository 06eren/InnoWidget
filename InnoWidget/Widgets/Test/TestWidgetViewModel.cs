using InnoWidget.Core.Mvvm;

namespace InnoWidget.Widgets.Test;

public class TestWidgetViewModel : ObservableObject
{
    private string _message = "Kasma sorunu çözüldü! ✅";

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public TestWidgetViewModel()
    {
        // TIMER YOK - KASMA SORUNU KÖKEN ÇÖZÜM
        // Sadece statik veri
    }
}
