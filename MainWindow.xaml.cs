using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace wpf_hub_kt8;

public partial class MainWindow : Window
{
    private const double KeyFrameSpeedPixelsPerSecond = 180;
    private AnimationClock? _ellipseXClock;
    private AnimationClock? _ellipseYClock;
    private Point _keyFrameCenter;
    private Vector _keyFrameVelocity = new(1, -0.7);
    private TimeSpan _lastKeyFrameRenderTime;
    private bool _keyFrameAnimationInitialized;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureEllipseAnimation();
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
        KeyFrameCanvas.SizeChanged += KeyFrameCanvas_SizeChanged;
    }

    private void WidthAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        double targetWidth = WidthAnimationButton.Width < 240 ? 300 : 170;

        var animation = new DoubleAnimation
        {
            To = targetWidth,
            Duration = TimeSpan.FromSeconds(0.45),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        WidthAnimationButton.BeginAnimation(WidthProperty, animation);
    }

    private void ConfigureEllipseAnimation()
    {
        var motionPath = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, 0) };

        figure.Segments.Add(new BezierSegment(new Point(60, -120), new Point(190, -120), new Point(250, -20), true));
        figure.Segments.Add(new QuadraticBezierSegment(new Point(320, 50), new Point(420, -70), true));
        figure.Segments.Add(new BezierSegment(new Point(490, -120), new Point(580, -80), new Point(660, 15), true));
        motionPath.Figures.Add(figure);

        var xAnimation = new DoubleAnimationUsingPath
        {
            PathGeometry = motionPath,
            Source = PathAnimationSource.X,
            Duration = TimeSpan.FromSeconds(5),
            RepeatBehavior = RepeatBehavior.Forever
        };

        var yAnimation = new DoubleAnimationUsingPath
        {
            PathGeometry = motionPath,
            Source = PathAnimationSource.Y,
            Duration = TimeSpan.FromSeconds(5),
            RepeatBehavior = RepeatBehavior.Forever
        };

        _ellipseXClock = xAnimation.CreateClock();
        _ellipseYClock = yAnimation.CreateClock();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeKeyFrameAnimation();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        CompositionTarget.Rendering -= CompositionTarget_Rendering;
    }

    private void KeyFrameCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_keyFrameAnimationInitialized)
        {
            InitializeKeyFrameAnimation();
            return;
        }

        _keyFrameCenter = ClampPointToCanvas(_keyFrameCenter);
        KeyFrameEllipseGeometry.Center = _keyFrameCenter;
    }

    private void InitializeKeyFrameAnimation()
    {
        if (KeyFrameCanvas.ActualWidth <= 0 || KeyFrameCanvas.ActualHeight <= 0)
        {
            return;
        }

        _keyFrameVelocity.Normalize();
        _keyFrameVelocity *= KeyFrameSpeedPixelsPerSecond;
        _keyFrameCenter = ClampPointToCanvas(new Point(
            KeyFrameEllipseGeometry.RadiusX + 22,
            KeyFrameCanvas.ActualHeight - KeyFrameEllipseGeometry.RadiusY - 22));

        KeyFrameEllipseGeometry.Center = _keyFrameCenter;
        _lastKeyFrameRenderTime = TimeSpan.Zero;

        CompositionTarget.Rendering -= CompositionTarget_Rendering;
        CompositionTarget.Rendering += CompositionTarget_Rendering;
        _keyFrameAnimationInitialized = true;
    }

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        if (!_keyFrameAnimationInitialized || e is not RenderingEventArgs renderingEventArgs)
        {
            return;
        }

        if (_lastKeyFrameRenderTime == TimeSpan.Zero)
        {
            _lastKeyFrameRenderTime = renderingEventArgs.RenderingTime;
            return;
        }

        double deltaSeconds = (renderingEventArgs.RenderingTime - _lastKeyFrameRenderTime).TotalSeconds;
        _lastKeyFrameRenderTime = renderingEventArgs.RenderingTime;

        if (deltaSeconds <= 0)
        {
            return;
        }

        double velocityX = _keyFrameVelocity.X;
        double velocityY = _keyFrameVelocity.Y;

        _keyFrameCenter.X = AdvanceAxis(
            _keyFrameCenter.X,
            ref velocityX,
            KeyFrameEllipseGeometry.RadiusX,
            KeyFrameCanvas.ActualWidth - KeyFrameEllipseGeometry.RadiusX,
            deltaSeconds);

        _keyFrameCenter.Y = AdvanceAxis(
            _keyFrameCenter.Y,
            ref velocityY,
            KeyFrameEllipseGeometry.RadiusY,
            KeyFrameCanvas.ActualHeight - KeyFrameEllipseGeometry.RadiusY,
            deltaSeconds);

        _keyFrameVelocity = new Vector(velocityX, velocityY);
        KeyFrameEllipseGeometry.Center = _keyFrameCenter;
    }

    private Point ClampPointToCanvas(Point point)
    {
        double minX = KeyFrameEllipseGeometry.RadiusX;
        double maxX = Math.Max(minX, KeyFrameCanvas.ActualWidth - KeyFrameEllipseGeometry.RadiusX);
        double minY = KeyFrameEllipseGeometry.RadiusY;
        double maxY = Math.Max(minY, KeyFrameCanvas.ActualHeight - KeyFrameEllipseGeometry.RadiusY);

        return new Point(
            Math.Clamp(point.X, minX, maxX),
            Math.Clamp(point.Y, minY, maxY));
    }

    private static double AdvanceAxis(double position, ref double velocity, double min, double max, double deltaSeconds)
    {
        if (max <= min)
        {
            velocity = 0;
            return min;
        }

        double next = position + velocity * deltaSeconds;

        while (next < min || next > max)
        {
            if (next < min)
            {
                next = min + (min - next);
                velocity = Math.Abs(velocity);
            }
            else if (next > max)
            {
                next = max - (next - max);
                velocity = -Math.Abs(velocity);
            }
        }

        return next;
    }

    private void StartCodeAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        CodeEllipseTransform.X = 0;
        CodeEllipseTransform.Y = 0;

        ConfigureEllipseAnimation();

        CodeEllipseTransform.ApplyAnimationClock(TranslateTransform.XProperty, _ellipseXClock);
        CodeEllipseTransform.ApplyAnimationClock(TranslateTransform.YProperty, _ellipseYClock);

        _ellipseXClock?.Controller?.Begin();
        _ellipseYClock?.Controller?.Begin();
    }

    private void PauseCodeAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        _ellipseXClock?.Controller?.Pause();
        _ellipseYClock?.Controller?.Pause();
    }

    private void ResumeCodeAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        _ellipseXClock?.Controller?.Resume();
        _ellipseYClock?.Controller?.Resume();
    }

    private void MenuToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        MenuToggleButton.Content = "Скрыть выпадающее меню";
        MenuPanel.Visibility = Visibility.Visible;

        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.25)
        };

        var scaleAnimation = new DoubleAnimation
        {
            From = 0.8,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.25),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        MenuPanel.BeginAnimation(OpacityProperty, opacityAnimation);
        MenuPanelScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }

    private void MenuToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        MenuToggleButton.Content = "Показать выпадающее меню";

        var opacityAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2)
        };

        var scaleAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0.8,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        opacityAnimation.Completed += (_, _) => MenuPanel.Visibility = Visibility.Collapsed;

        MenuPanel.BeginAnimation(OpacityProperty, opacityAnimation);
        MenuPanelScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
    }
}
