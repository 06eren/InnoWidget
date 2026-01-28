using System;
using System.Windows.Input;
using InnoWidget.Core.Mvvm;

namespace InnoWidget.Host;

public sealed class WidgetToggleItemViewModel : ObservableObject
{
    private readonly Action<bool> _setOpen;
    private readonly Action<double> _setOpacity;

    public string Id { get; }
    public string Title { get; }
    public string IconUri { get; }

    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (SetProperty(ref _isOpen, value))
                _setOpen(value);
        }
    }

    public ICommand ToggleCommand { get; }

    private double _opacity = 1.0;
    public double Opacity
    {
        get => _opacity;
        set
        {
            if (SetProperty(ref _opacity, value))
                _setOpacity(value);
        }
    }

    public WidgetToggleItemViewModel(string id, string title, string iconUri, bool isOpen, double opacity, Action<bool> setOpen, Action<double> setOpacity)
    {
        Id = id;
        Title = title;
        IconUri = iconUri;
        _isOpen = isOpen;
        _setOpen = setOpen;
        _opacity = opacity;
        _setOpacity = setOpacity;
        ToggleCommand = new RelayCommand(() => IsOpen = !IsOpen);
    }
}
