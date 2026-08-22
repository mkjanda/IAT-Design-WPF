using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Non-blocking error banner with slide-down / slide-up animation.
    /// Resource Storyboards are frozen — always <see cref="Freezable.Clone"/> before
    /// attaching handlers or beginning on a target.
    /// </summary>
    public partial class ErrorBannerControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ErrorBannerControl),
                new PropertyMetadata("Error"));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ErrorBannerControl),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private DispatcherTimer? _autoHideTimer;
        private Storyboard? _activeStoryboard;

        public ErrorBannerControl()
        {
            InitializeComponent();
            DataContext = this;

            // Animations target (UIElement.RenderTransform).(TranslateTransform.Y).
            // Ensure a live transform exists so Begin never no-ops or throws.
            if (BannerBorder.RenderTransform is not TranslateTransform)
                BannerBorder.RenderTransform = new TranslateTransform();
        }

        /// <summary>
        /// Shows the banner with a slide-down animation and optional auto-hide.
        /// Safe to call repeatedly — cancels any in-flight hide/show.
        /// </summary>
        public void Show(string title, string message, int autoHideSeconds = 15)
        {
            Title = title ?? "Error";
            Message = message ?? string.Empty;

            StopAutoHide();
            StopActiveStoryboard();

            BannerBorder.Visibility = Visibility.Visible;
            BannerBorder.Opacity = 1;

            if (BannerBorder.RenderTransform is TranslateTransform tt)
                tt.Y = 0;

            // Resource Storyboards are frozen. Clone() returns a mutable copy that can
            // accept Completed handlers and be begun on a specific target.
            var storyboard = GetMutableStoryboard("SlideDown");
            if (storyboard is not null)
            {
                _activeStoryboard = storyboard;
                storyboard.Begin(BannerBorder, isControllable: true);
            }

            if (autoHideSeconds > 0)
            {
                _autoHideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(autoHideSeconds)
                };
                _autoHideTimer.Tick += OnAutoHideTick;
                _autoHideTimer.Start();
            }
        }

        private void OnAutoHideTick(object? sender, EventArgs e)
        {
            StopAutoHide();
            Hide();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Hide();

        /// <summary>
        /// Hides the banner with a slide-up animation, then collapses visibility.
        /// </summary>
        public void Hide()
        {
            StopAutoHide();
            StopActiveStoryboard();

            if (BannerBorder.Visibility != Visibility.Visible)
                return;

            var storyboard = GetMutableStoryboard("SlideUp");
            if (storyboard is null)
            {
                BannerBorder.Visibility = Visibility.Collapsed;
                return;
            }

            _activeStoryboard = storyboard;
            EventHandler? onCompleted = null;
            onCompleted = (_, _) =>
            {
                storyboard.Completed -= onCompleted;
                if (ReferenceEquals(_activeStoryboard, storyboard))
                    _activeStoryboard = null;
                BannerBorder.Visibility = Visibility.Collapsed;
            };
            storyboard.Completed += onCompleted;
            storyboard.Begin(BannerBorder, isControllable: true);
        }

        /// <summary>
        /// Resolves a resource Storyboard and returns an unfrozen clone safe to modify.
        /// </summary>
        private Storyboard? GetMutableStoryboard(string resourceKey)
        {
            if (TryFindResource(resourceKey) is not Storyboard resource)
                return null;

            // Frozen resource dictionaries cannot have handlers attached or be modified.
            // Clone() produces a deep, unfrozen copy.
            return resource.Clone();
        }

        private void StopAutoHide()
        {
            if (_autoHideTimer is null) return;
            _autoHideTimer.Stop();
            _autoHideTimer.Tick -= OnAutoHideTick;
            _autoHideTimer = null;
        }

        private void StopActiveStoryboard()
        {
            if (_activeStoryboard is null) return;
            try
            {
                _activeStoryboard.Stop(BannerBorder);
            }
            catch
            {
                // Target may already be detached — ignore.
            }
            _activeStoryboard = null;
        }
    }
}
