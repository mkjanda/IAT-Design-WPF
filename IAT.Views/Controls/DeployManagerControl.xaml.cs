using IAT.ViewModels.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Controls;

/// <summary>
/// Interaction logic for DeployManagerControl.xaml.
/// Pure view — domain logic lives in <see cref="DeployManagerViewModel"/>.
/// Visibility drives WebSocket lifetime; size drives responsive list typography.
/// </summary>
public partial class DeployManagerControl : UserControl
{
    /// <summary>Reference width (px) at which list fonts reach their maximum (top-bar size).</summary>
    private const double ReferenceWidth = 1100.0;

    /// <summary>Top-bar body size is the hard ceiling for list type.</summary>
    private const double MaxNameFontSize = 12.0;
    private const double MaxMetaFontSize = 11.0;
    private const double MinNameFontSize = 9.5;
    private const double MinMetaFontSize = 8.5;

    public static readonly DependencyProperty ReportNameFontSizeProperty =
        DependencyProperty.Register(
            nameof(ReportNameFontSize),
            typeof(double),
            typeof(DeployManagerControl),
            new PropertyMetadata(MaxNameFontSize));

    public static readonly DependencyProperty ReportMetaFontSizeProperty =
        DependencyProperty.Register(
            nameof(ReportMetaFontSize),
            typeof(double),
            typeof(DeployManagerControl),
            new PropertyMetadata(MaxMetaFontSize));

    public static readonly DependencyProperty ReportCellPaddingProperty =
        DependencyProperty.Register(
            nameof(ReportCellPadding),
            typeof(Thickness),
            typeof(DeployManagerControl),
            new PropertyMetadata(new Thickness(12, 9, 12, 9)));

    /// <summary>Primary line (test name) in each report row — never exceeds top-bar size.</summary>
    public double ReportNameFontSize
    {
        get => (double)GetValue(ReportNameFontSizeProperty);
        set => SetValue(ReportNameFontSizeProperty, value);
    }

    /// <summary>Secondary line (date, size, URL, status) — slightly smaller than the name.</summary>
    public double ReportMetaFontSize
    {
        get => (double)GetValue(ReportMetaFontSizeProperty);
        set => SetValue(ReportMetaFontSizeProperty, value);
    }

    /// <summary>Inner padding of each report card; grows/shrinks with type.</summary>
    public Thickness ReportCellPadding
    {
        get => (Thickness)GetValue(ReportCellPaddingProperty);
        set => SetValue(ReportCellPaddingProperty, value);
    }

    public DeployManagerControl()
    {
        InitializeComponent();
        SizeChanged += OnControlSizeChanged;
        Loaded += (_, _) => UpdateResponsiveTypography(ActualWidth);
    }

    private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
            UpdateResponsiveTypography(e.NewSize.Width);
    }

    /// <summary>
    /// Scales list typography with control width. Caps at top-bar size so the
    /// account strip always remains the visual hierarchy ceiling.
    /// </summary>
    private void UpdateResponsiveTypography(double width)
    {
        if (width < 1)
            return;

        // 0 at very narrow, 1 at reference width and above
        var t = Math.Clamp(width / ReferenceWidth, 0.0, 1.0);

        ReportNameFontSize = Lerp(MinNameFontSize, MaxNameFontSize, t);
        ReportMetaFontSize = Lerp(MinMetaFontSize, MaxMetaFontSize, t);

        // Padding tracks type so cells don't feel cramped or sparse
        var padH = Lerp(8, 14, t);
        var padV = Lerp(6, 10, t);
        ReportCellPadding = new Thickness(padH, padV, padH, padV);
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is not DeployManagerViewModel vm)
            return;

        if (e.NewValue is true)
            await vm.OnActivatedAsync();
        else
            await vm.OnDeactivatedAsync();
    }
}
