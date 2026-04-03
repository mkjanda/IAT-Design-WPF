using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services
{
    /// <summary>
    /// Represents a notification message that conveys error information, including a title, message, optional
    /// exception, and additional details.
    /// </summary>
    /// <remarks>Use this record to encapsulate error information for display in user interfaces or for
    /// logging purposes. The properties provide both user-friendly and technical information about the error.</remarks>
    /// <param name="Title">The short, user-facing title that summarizes the error.</param>
    /// <param name="Message">The main message describing the error in detail.</param>
    /// <param name="Exception">The exception associated with the error, if available; otherwise, null.</param>
    /// <param name="Details">Additional details about the error, such as a stack trace or diagnostic information. Can be null if no extra
    /// details are provided.</param>
    public sealed record ErrorNotificationMessage(String Title, String Message, Exception? Exception = null, string? Details = null);


    /// <summary>
    /// Defines a service for displaying error notifications to the user.   
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for presenting error messages in a manner
    /// appropriate to the application's user interface. This may include displaying dialogs, banners, or other
    /// notification mechanisms.</remarks>
    public interface IUserNotificationService
    {
        /// <summary>
        /// Displays an error notification to the user using the specified error message.
        /// </summary>
        /// <param name="message">The error notification message to display. Cannot be null.</param>
        void ShowError(ErrorNotificationMessage message);
    }
}
