using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace InnoWidget.Core.Services;

public class OptimizedAnimationEngine
{
    private static readonly Lazy<OptimizedAnimationEngine> _instance = new(() => new OptimizedAnimationEngine());
    public static OptimizedAnimationEngine Instance => _instance.Value;

    private readonly DispatcherTimer _cleanupTimer;
    private readonly List<WeakReference<Storyboard>> _activeAnimations = new();
    private bool _isOptimized;

    private OptimizedAnimationEngine()
    {
        _cleanupTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _cleanupTimer.Tick += CleanupAnimations;
        _cleanupTimer.Start();
    }

    public void OptimizeForPerformance()
    {
        if (_isOptimized) return;

        // Reduce animation quality for performance
        Timeline.DesiredFrameRateProperty.OverrideMetadata(
            typeof(Timeline),
            new FrameworkPropertyMetadata(30)); // 30 FPS instead of 60

        // Enable hardware acceleration
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

        _isOptimized = true;
    }

    public void RegisterAnimation(Storyboard storyboard)
    {
        _activeAnimations.Add(new WeakReference<Storyboard>(storyboard));
    }

    public void CreateOptimizedAnimation(FrameworkElement target, string property, double from, double to, Duration duration)
    {
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = duration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath(property));

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        
        RegisterAnimation(storyboard);
        storyboard.Begin();
    }

    private void CleanupAnimations(object? sender, EventArgs e)
    {
        for (int i = _activeAnimations.Count - 1; i >= 0; i--)
        {
            if (!_activeAnimations[i].TryGetTarget(out var storyboard) || 
                storyboard.GetCurrentState() == ClockState.Stopped)
            {
                _activeAnimations.RemoveAt(i);
            }
        }

        // Force garbage collection if too many animations
        if (_activeAnimations.Count > 100)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Stop();
        _activeAnimations.Clear();
    }
}
