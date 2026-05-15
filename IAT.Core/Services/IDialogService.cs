using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services
{
    /// <summary>
    /// This interface defines a service for displaying dialogs to the user, such as confirmation prompts and notifications. It provides 
    /// asynchronous methods for showing confirmation dialogs that return a boolean result based on user input, as well as methods for displaying 
    /// informational notifications without expecting a response. This abstraction allows for consistent dialog handling across the application 
    /// and enables easy mocking for unit testing purposes.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays an asynchronous confirmation dialog with the specified message and title, and returns the user's
        /// response.
        /// </summary>
        /// <param name="message">The message to display in the confirmation dialog. This text should clearly indicate the action to be
        /// confirmed by the user.</param>
        /// <param name="title">The title of the confirmation dialog. Defaults to "Confirm" if not specified.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the user
        /// confirms; otherwise, <see langword="false"/>.</returns>
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");

        /// <summary>
        /// Displays a notification to the user asynchronously with the specified message and optional title.
        /// </summary>
        /// <param name="message">The message content to display in the notification. Cannot be null or empty.</param>
        /// <param name="title">The title of the notification. Defaults to "Notification" if not specified.</param>
        /// <returns>A task that represents the asynchronous operation of displaying the notification.</returns>
        Task ShowNotificationAsync(string message, string title = "Notification");
    }
}
