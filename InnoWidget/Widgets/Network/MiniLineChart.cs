using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace InnoWidget.Widgets.Network;

public sealed class MiniLineChart : FrameworkElement
{
    public static readonly DependencyProperty ValuesProperty =
        DependencyProperty.Register(nameof(Values), typeof(IEnumerable<double>), typeof(MiniLineChart),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnChartPropertyChanged));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(MiniLineChart),
            new FrameworkPropertyMetadata(Brushes.Lime, FrameworkPropertyMetadataOptions.AffectsRender, OnChartPropertyChanged));

    public static readonly DependencyProperty GridStrokeProperty =
        DependencyProperty.Register(nameof(GridStroke), typeof(Brush), typeof(MiniLineChart),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), FrameworkPropertyMetadataOptions.AffectsRender, OnChartPropertyChanged));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(MiniLineChart),
            new FrameworkPropertyMetadata(1.5, FrameworkPropertyMetadataOptions.AffectsRender, OnChartPropertyChanged));

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(MiniLineChart),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender, OnChartPropertyChanged));

    private StreamGeometry? _cachedGeometry;
    private Pen? _cachedPen;
    private Pen? _cachedGridPen;
    private double _cachedWidth;
    private double _cachedHeight;
    private double _cachedMax;
    private IEnumerable<double>? _cachedValuesRef;
    private bool _isDirty = true;

    public IEnumerable<double>? Values
    {
        get => (IEnumerable<double>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public Brush GridStroke
    {
        get => (Brush)GetValue(GridStrokeProperty);
        set => SetValue(GridStrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    private static void OnChartPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MiniLineChart c)
        {
            c._isDirty = true;
            c.InvalidateVisual();
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _isDirty = true;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 2 || h <= 2)
            return;

        var rect = new Rect(0, 0, w, h);
        dc.DrawRectangle(Brushes.Transparent, null, rect);

        if (_cachedGridPen is null || !ReferenceEquals(_cachedGridPen.Brush, GridStroke))
        {
            _cachedGridPen = new Pen(GridStroke, 1);
            _cachedGridPen.Freeze();
        }

        // subtle horizontal grid lines
        dc.DrawLine(_cachedGridPen, new Point(0, h * 0.25), new Point(w, h * 0.25));
        dc.DrawLine(_cachedGridPen, new Point(0, h * 0.5), new Point(w, h * 0.5));
        dc.DrawLine(_cachedGridPen, new Point(0, h * 0.75), new Point(w, h * 0.75));

        var valuesRef = Values;
        if (valuesRef is null)
            return;

        if (_isDirty || !ReferenceEquals(_cachedValuesRef, valuesRef) || _cachedWidth != w || _cachedHeight != h)
        {
            BuildGeometry(valuesRef, w, h);
        }

        if (_cachedGeometry is null)
            return;

        if (_cachedPen is null || !ReferenceEquals(_cachedPen.Brush, Stroke) || Math.Abs(_cachedPen.Thickness - StrokeThickness) > 0.001)
        {
            _cachedPen = new Pen(Stroke, StrokeThickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };
            _cachedPen.Freeze();
        }

        dc.DrawGeometry(null, _cachedPen, _cachedGeometry);
    }

    private void BuildGeometry(IEnumerable<double> valuesRef, double w, double h)
    {
        _cachedValuesRef = valuesRef;
        _cachedWidth = w;
        _cachedHeight = h;

        if (!TryGetCount(valuesRef, out var count) || count < 2)
        {
            _cachedGeometry = null;
            _isDirty = false;
            return;
        }

        var max = MaxValue;
        if (double.IsNaN(max) || max <= 0)
            max = GetMaxNonNegative(valuesRef);

        if (max <= 0)
            max = 1;

        if (Math.Abs(_cachedMax - max) > 0.0001)
            _cachedMax = max;

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            var i = 0;
            foreach (var raw in valuesRef)
            {
                var v = raw;
                if (double.IsNaN(v) || double.IsInfinity(v) || v < 0)
                    v = 0;

                var x = (w - 1) * i / (count - 1);
                var y = (h - 1) - ((h - 2) * (v / max));

                if (i == 0)
                    ctx.BeginFigure(new Point(x, y), isFilled: false, isClosed: false);
                else
                    ctx.LineTo(new Point(x, y), isStroked: true, isSmoothJoin: true);

                i++;
                if (i >= count)
                    break;
            }
        }
        geo.Freeze();
        _cachedGeometry = geo;
        _isDirty = false;
    }

    private static bool TryGetCount(IEnumerable<double> values, out int count)
    {
        if (values is ICollection<double> c)
        {
            count = c.Count;
            return true;
        }

        if (values is IReadOnlyCollection<double> rc)
        {
            count = rc.Count;
            return true;
        }

        if (values is Array a)
        {
            count = a.Length;
            return true;
        }

        count = 0;
        return false;
    }

    private static double GetMaxNonNegative(IEnumerable<double> values)
    {
        var max = 0.0;
        foreach (var v0 in values)
        {
            var v = v0;
            if (double.IsNaN(v) || double.IsInfinity(v) || v < 0)
                continue;

            if (v > max)
                max = v;
        }
        return max;
    }
}
