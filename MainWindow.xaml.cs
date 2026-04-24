using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace wpf_hub_kt8;

public partial class MainWindow : Window
{
    private readonly Storyboard _ellipseStoryboard = new();

    public MainWindow()
    {
        InitializeComponent();
        ConfigureEllipseAnimation();
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

        Storyboard.SetTarget(xAnimation, CodeEllipseTransform);
        Storyboard.SetTarget(yAnimation, CodeEllipseTransform);
        Storyboard.SetTargetProperty(xAnimation, new PropertyPath(TranslateTransform.XProperty));
        Storyboard.SetTargetProperty(yAnimation, new PropertyPath(TranslateTransform.YProperty));

        _ellipseStoryboard.Children.Add(xAnimation);
        _ellipseStoryboard.Children.Add(yAnimation);
    }

    private void StartCodeAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        _ellipseStoryboard.Stop(this);
        CodeEllipseTransform.X = 0;
        CodeEllipseTransform.Y = 0;
        _ellipseStoryboard.Begin(this, true);
    }

    private void PauseCodeAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        _ellipseStoryboard.Pause(this);
    }

    private void ResumeCodeAnimationButton_Click(object sender, RoutedEventArgs e)
    {
        _ellipseStoryboard.Resume(this);
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
