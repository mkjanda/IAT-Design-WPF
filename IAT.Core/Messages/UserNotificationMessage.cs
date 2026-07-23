using CommunityToolkit.Mvvm.Messaging.Messages;
using org.omg.PortableInterceptor;

namespace IAT.Core.Messages
{
    /// <summary>
    /// Message sent via WeakReferenceMessenger for success, info, or general user notifications.
    /// Parallel structure to ErrorNotificationMessage for consistency across the app.
    /// </summary>
    public class UserNotificationMessage : ValueChangedMessage<string>
    {
        /// <summary>
        /// Gets the title of the notification (e.g. "Success", "Info").
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the severity level of the notification for UI styling (success = green, etc.).
        /// </summary>
        public NotificationSeverity Severity { get; }

        /// <summary>
        /// Full constructor for maximum flexibility.
        /// </summary>
        public UserNotificationMessage(string title, string message, NotificationSeverity severity = NotificationSeverity.Info)
            : base(message)
        {
            Title = title ?? "Notification";
            Severity = severity;
        }

        /// <summary>
        /// Convenience constructor for quick success messages (what you used in TestDesignerViewModel).
        /// </summary>
        public UserNotificationMessage(string message)
            : this("Success", message, NotificationSeverity.Success)
        {
        }
    }

    /// <summary>
    /// Notification severity levels for consistent UI treatment (green for success, etc.).
    /// </summary>
    public enum NotificationSeverity
    {
        /// <summary>
        /// Informational notification, typically styled in a neutral color.    
        /// </summary>
        Info,
        /// <summary>
        /// Successful notification, typically styled in green.
        /// </summary>
        Success,
        /// <summary>
        /// Warning notification, typically styled in yellow or orange.
        /// </summary>
        Warning,
        /// <summary>
        /// Error notification, typically styled in red.
        /// </summary>
        Error
    }

    /// <summary>
    /// Abstract base record for user notification messages, providing a common structure for title, message, and severity.
    /// </summary>
    /// <param name="Title">The title of the notification (e.g. "Success", "Info").</param>
    /// <param name="Message">The message content of the notification.</param>
    /// <param name="Severity">The severity level of the notification for UI styling.</param>
    public abstract record NotificationMessage(string Title, string Message, NotificationSeverity Severity)
    {
        /// <summary>
        /// A factory method for creating a user notification message with specified title, message, and severity.
        /// </summary>
        /// <param name="title">The title of the notification (e.g. "Success", "Info").</param>
        /// <param name="message">The message content of the notification.</param>
        /// <param name="severity">The severity level of the notification for UI styling.</param>
        /// <returns>A new instance of <see cref="NotificationMessage"/> representing the user notification.</returns>
        public static NotificationMessage UserNotificationMessage(string title, string message, NotificationSeverity severity)  => new _UserNotification(title, message, severity);
        
        /// <summary>
        /// Creates a success notification message.
        /// </summary>
        /// <param name="message">The notification message content.</param>
        /// <returns>A notification message with success severity.</returns>
        public static NotificationMessage SuccessMessage(string message) => new _UserNotification("Success", message, NotificationSeverity.Success);
        
        /// <summary>
        /// Creates an error notification message.
        /// </summary>
        /// <param name="title">The title of the error notification.</param>
        /// <param name="message">The error message content.</param>
        /// <param name="exception">The exception that caused the error, if any.</param>
        /// <param name="details">Additional details about the error, if any.</param>
        /// <returns>A new error notification message.</returns>
        public static NotificationMessage ErrorNotificationMessage(string title, string message, Exception? exception = null, string? details = null) => new _ErrorNotificationMessage(title, message, exception, details);

        private record _UserNotification(string Title, string Message, NotificationSeverity Severity) : NotificationMessage(Title, Message, Severity);

        private record _SuccessMessage(string Message) : NotificationMessage("Success", Message, NotificationSeverity.Success);
        private record _ErrorNotificationMessage(string Title, string Message, Exception? Exception = null, string? Details = null) : NotificationMessage(Title, Message, NotificationSeverity.Error);

    }
}