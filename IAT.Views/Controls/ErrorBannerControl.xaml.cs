using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace IAT.Views.Controls
{
    /// <summary>
    /// Interaction logic for ErrorBannerControl.xaml
    /// </summary>
    public partial class ErrorBannerControl : UserControl
    {
        /// <summary>
        /// DependencyProperty for the Title of the error banner.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ErrorBannerControl), new PropertyMetadata("Error"));
        
        /// <summary>
        /// DependencyProperty for the Message of the error banner.
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(ErrorBannerControl));

        /// <summary>
        /// Gets or sets the Title of the error banner.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the Message of the error banner.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty); 
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBannerControl"/> class.
        /// </summary>
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