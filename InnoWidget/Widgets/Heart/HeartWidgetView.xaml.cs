using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;
using System;
using System.Collections.Generic;

namespace InnoWidget.Widgets.Heart;

public partial class HeartWidgetView : UserControl
{
    private readonly DispatcherTimer _heartTimer;
    private readonly Random _rnd = new();
    private readonly List<Ellipse> _activeHearts = new();

    public HeartWidgetView()
    {
        InitializeComponent();
        
        // Heart animation timer
        _heartTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.8)
        };
        _heartTimer.Tick += CreateHeart;
        _heartTimer.Start();
        Loaded += (_, _) => CreateHeart(null, EventArgs.Empty);
    }

    private void CreateHeart(object? sender, EventArgs e)
    {
        var heart = new Ellipse
        {
            Width = _rnd.Next(8, 16),
            Height = _rnd.Next(8, 16),
            Fill = new SolidColorBrush(Color.FromArgb(200, 
                (byte)_rnd.Next(220, 255), 
                (byte)_rnd.Next(100, 200), 
                (byte)_rnd.Next(150, 255))),
            Effect = new BlurEffect { Radius = 1 }
        };

        var startX = _rnd.NextDouble() * HeartCanvas.ActualWidth;
        Canvas.SetLeft(heart, startX);
        Canvas.SetTop(heart, -heart.Height);

        HeartCanvas.Children.Add(heart);
        _activeHearts.Add(heart);

        var fallDuration = TimeSpan.FromSeconds(_rnd.Next(3, 6));
        var swayDuration = TimeSpan.FromSeconds(1 + _rnd.NextDouble() * 2);

        // Fall animation
        var fallAnimation = new DoubleAnimation
        {
            From = -heart.Height,
            To = HeartCanvas.ActualHeight + heart.Height,
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

        // Pulse animation
        var pulseAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 1.2,
            Duration = TimeSpan.FromSeconds(1),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(fallDuration)
        };

        heart.RenderTransform = new TransformGroup
        {
            Children = { new TranslateTransform(), new ScaleTransform() }
        };

        heart.RenderTransformOrigin = new Point(0.5, 0.5);

        Storyboard.SetTarget(fallAnimation, heart);
        Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(Canvas.Top)"));

        Storyboard.SetTarget(swayAnimation, heart);
        Storyboard.SetTargetProperty(swayAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

        Storyboard.SetTarget(pulseAnimation, heart);
        Storyboard.SetTargetProperty(pulseAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(ScaleTransform.ScaleX)"));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fallAnimation);
        storyboard.Children.Add(swayAnimation);
        storyboard.Children.Add(pulseAnimation);

        storyboard.Completed += (_, _) =>
        {
            HeartCanvas.Children.Remove(heart);
            _activeHearts.Remove(heart);
        };

        storyboard.Begin();
    }
}
