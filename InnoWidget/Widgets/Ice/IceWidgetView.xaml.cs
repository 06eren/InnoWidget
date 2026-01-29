using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;
using System;
using System.Collections.Generic;

namespace InnoWidget.Widgets.Ice;

public partial class IceWidgetView : UserControl
{
    private readonly DispatcherTimer _snowTimer;
    private readonly Random _rnd = new();
    private readonly List<Ellipse> _activeSnowflakes = new();

    public IceWidgetView()
    {
        InitializeComponent();
        
        // Snow animation timer
        _snowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        _snowTimer.Tick += CreateSnowflake;
        _snowTimer.Start();
        Loaded += (_, _) => CreateSnowflake(null, EventArgs.Empty);
    }

    private void CreateSnowflake(object? sender, EventArgs e)
    {
        var snowflake = new Ellipse
        {
            Width = _rnd.Next(4, 12),
            Height = _rnd.Next(4, 12),
            Fill = new SolidColorBrush(Color.FromArgb(200, 
                (byte)_rnd.Next(240, 255), 
                (byte)_rnd.Next(240, 255), 
                (byte)_rnd.Next(255, 255))),
            Effect = new BlurEffect { Radius = 0.5 }
        };

        var startX = _rnd.NextDouble() * SnowCanvas.ActualWidth;
        Canvas.SetLeft(snowflake, startX);
        Canvas.SetTop(snowflake, -snowflake.Height);

        SnowCanvas.Children.Add(snowflake);
        _activeSnowflakes.Add(snowflake);

        var fallDuration = TimeSpan.FromSeconds(_rnd.Next(3, 6));
        var swayDuration = TimeSpan.FromSeconds(1 + _rnd.NextDouble() * 2);

        // Fall animation
        var fallAnimation = new DoubleAnimation
        {
            From = -snowflake.Height,
            To = SnowCanvas.ActualHeight + snowflake.Height,
            Duration = fallDuration,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
        };

        // Sway animation
        var swayAnimation = new DoubleAnimationUsingKeyFrames();
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(0, TimeSpan.Zero));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(20, TimeSpan.FromSeconds(swayDuration.TotalSeconds / 2)));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(-15, swayDuration));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(10, TimeSpan.FromSeconds(fallDuration.TotalSeconds * 0.7)));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(0, fallDuration));

        snowflake.RenderTransform = new TransformGroup
        {
            Children = { new TranslateTransform(), new RotateTransform() }
        };

        snowflake.RenderTransformOrigin = new Point(0.5, 0.5);

        Storyboard.SetTarget(fallAnimation, snowflake);
        Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(Canvas.Top)"));

        Storyboard.SetTarget(swayAnimation, snowflake);
        Storyboard.SetTargetProperty(swayAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fallAnimation);
        storyboard.Children.Add(swayAnimation);

        storyboard.Completed += (_, _) =>
        {
            SnowCanvas.Children.Remove(snowflake);
            _activeSnowflakes.Remove(snowflake);
        };

        storyboard.Begin();
    }
}
