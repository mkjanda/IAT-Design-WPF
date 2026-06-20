using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace IAT.Views.Controls
{
    public partial class ErrorBannerControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ErrorBannerControl), new PropertyMetadata("Error"));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ErrorBannerControl));

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

        public ErrorBannerControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Call this to show the banner with slide-down animation.
        /// </summary>
        public void Show(string title, string message, int autoHideSeconds = 8)
        {
            Title = title;
            Message = message;

            BannerBorder.Visibility = Visibility.Visible;

            // Slide down animation
            var storyboard = (Storyboard)FindResource("SlideDown");
            storyboard.Begin(BannerBorder);

            // Auto-hide
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(autoHideSeconds) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Hide();
            };
            timer.Start();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Hide()
        {
            var storyboard = (Storyboard)FindResource("SlideUp");
            storyboard.Completed += (s, e) => BannerBorder.Visibility = Visibility.Collapsed;
            storyboard.Begin(BannerBorder);
        }
    }
}