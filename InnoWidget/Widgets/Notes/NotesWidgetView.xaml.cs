using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace InnoWidget.Widgets.Notes;

public partial class NotesWidgetView : UserControl
{
    private readonly DispatcherTimer _leafTimer;
    private readonly Random _rnd = new();
    private readonly List<Ellipse> _activeLeaves = new();

    public NotesWidgetView()
    {
        InitializeComponent();
        _leafTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };
        _leafTimer.Tick += CreateFallingLeaf;
        _leafTimer.Start();
        Loaded += (_, _) => CreateFallingLeaf(null, EventArgs.Empty);
    }

    private void CreateFallingLeaf(object? sender, EventArgs e)
    {

        var leaf = new Ellipse
        {
            Width = _rnd.Next(8, 16),
            Height = _rnd.Next(10, 18),
            Fill = new SolidColorBrush(Color.FromArgb(180, 
                (byte)_rnd.Next(120, 200), 
                (byte)_rnd.Next(180, 255), 
                (byte)_rnd.Next(100, 160))),
            Effect = new BlurEffect { Radius = 0.5 }
        };

        var startX = _rnd.NextDouble() * LeavesCanvas.ActualWidth;
        Canvas.SetLeft(leaf, startX);
        Canvas.SetTop(leaf, -leaf.Height);

        LeavesCanvas.Children.Add(leaf);
        _activeLeaves.Add(leaf);

        var fallDuration = TimeSpan.FromSeconds(_rnd.Next(3, 6));
        var swayDuration = TimeSpan.FromSeconds(1.5 + _rnd.NextDouble() * 1);

        // Fall animation
        var fallAnimation = new DoubleAnimation
        {
            From = -leaf.Height,
            To = LeavesCanvas.ActualHeight + leaf.Height,
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
        swayAnimation.RepeatBehavior = new RepeatBehavior(1);

        // Rotate animation
        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = _rnd.Next(-180, 180),
            Duration = fallDuration,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        leaf.RenderTransform = new TransformGroup
        {
            Children = { new TranslateTransform(), new RotateTransform() }
        };

        leaf.RenderTransformOrigin = new Point(0.5, 0.5);

        Storyboard.SetTarget(fallAnimation, leaf);
        Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(Canvas.Top)"));

        Storyboard.SetTarget(swayAnimation, leaf);
        Storyboard.SetTargetProperty(swayAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

        Storyboard.SetTarget(rotateAnimation, leaf);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fallAnimation);
        storyboard.Children.Add(swayAnimation);
        storyboard.Children.Add(rotateAnimation);

        storyboard.Completed += (_, _) =>
        {
            LeavesCanvas.Children.Remove(leaf);
            _activeLeaves.Remove(leaf);
        };

        storyboard.Begin();
    }

    private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        if (tb.IsReadOnly)
        {
            tb.IsReadOnly = false;
            tb.Focus();
            e.Handled = true;
        }
    }

    private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox tb)
            tb.IsReadOnly = true;
    }
}
