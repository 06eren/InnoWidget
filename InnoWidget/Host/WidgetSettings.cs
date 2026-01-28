namespace InnoWidget.Host;

public sealed class WidgetSettings
{
    public string Id { get; set; } = "";
    public bool IsOpen { get; set; }
    public double Left { get; set; }
    public double Top { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Opacity { get; set; } = 1.0;
}
