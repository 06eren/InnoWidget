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
using InnoWidget.Core.Mvvm;

namespace InnoWidget.Widgets.Hardware;

public partial class HardwareWidgetView : UserControl
{
    private readonly DispatcherTimer _sakuraTimer;
    private readonly Random _rnd = new();
    private readonly List<Ellipse> _activePetals = new();

    public HardwareWidgetView()
    {
        InitializeComponent();
        DataContextChanged += HardwareWidgetView_DataContextChanged;
        
        // Sakura petal animation timer
        _sakuraTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.8)
        };
        _sakuraTimer.Tick += CreateSakuraPetal;
        _sakuraTimer.Start();
        Loaded += (_, _) => CreateSakuraPetal(null, EventArgs.Empty);
    }

    private void HardwareWidgetView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is HardwareWidgetViewModel vm)
        {
            vm.ToggleSettingsCommand = new RelayCommand(() => ToggleSettings(vm));
            vm.SettingsVisible = false;
        }
    }

    private void ToggleSettings(HardwareWidgetViewModel vm)
    {
        vm.SettingsVisible = !vm.SettingsVisible;
    }

    private void CreateSakuraPetal(object? sender, EventArgs e)
    {
        var petal = new Ellipse
        {
            Width = _rnd.Next(12, 20),
            Height = _rnd.Next(15, 25),
            Fill = new SolidColorBrush(Color.FromArgb(200, 
                (byte)_rnd.Next(240, 255), 
                (byte)_rnd.Next(180, 220), 
                (byte)_rnd.Next(200, 230))),
            Effect = new BlurEffect { Radius = 1 }
        };

        var startX = _rnd.NextDouble() * SakuraCanvas.ActualWidth;
        Canvas.SetLeft(petal, startX);
        Canvas.SetTop(petal, -petal.Height);

        SakuraCanvas.Children.Add(petal);
        _activePetals.Add(petal);

        var fallDuration = TimeSpan.FromSeconds(_rnd.Next(4, 8));
        var swayDuration = TimeSpan.FromSeconds(2 + _rnd.NextDouble() * 2);

        // Fall animation
        var fallAnimation = new DoubleAnimation
        {
            From = -petal.Height,
            To = SakuraCanvas.ActualHeight + petal.Height,
            Duration = fallDuration,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
        };

        // Sway animation
        var swayAnimation = new DoubleAnimationUsingKeyFrames();
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(0, TimeSpan.Zero));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(30, TimeSpan.FromSeconds(swayDuration.TotalSeconds / 2)));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(-20, swayDuration));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(15, TimeSpan.FromSeconds(fallDuration.TotalSeconds * 0.7)));
        swayAnimation.KeyFrames.Add(new SplineDoubleKeyFrame(0, fallDuration));

        // Rotate animation
        var rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = _rnd.Next(-180, 180),
            Duration = fallDuration,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = 0.3,
            To = 1,
            Duration = TimeSpan.FromSeconds(1),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(fallDuration)
        };

        petal.RenderTransform = new TransformGroup
        {
            Children = { new TranslateTransform(), new RotateTransform() }
        };

        petal.RenderTransformOrigin = new Point(0.5, 0.5);

        Storyboard.SetTarget(fallAnimation, petal);
        Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(Canvas.Top)"));

        Storyboard.SetTarget(swayAnimation, petal);
        Storyboard.SetTargetProperty(swayAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

        Storyboard.SetTarget(rotateAnimation, petal);
        Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));

        Storyboard.SetTarget(opacityAnimation, petal);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

        var storyboard = new Storyboard();
        storyboard.Children.Add(fallAnimation);
        storyboard.Children.Add(swayAnimation);
        storyboard.Children.Add(rotateAnimation);
        storyboard.Children.Add(opacityAnimation);

        storyboard.Completed += (_, _) =>
        {
            SakuraCanvas.Children.Remove(petal);
            _activePetals.Remove(petal);
        };

        storyboard.Begin();
    }
}
