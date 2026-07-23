using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Messages;
using IAT.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace IAT_Design_WPF.Services
{
    /// <summary>
    /// Provides methods for displaying user notifications and errors.
    /// </summary>
    public class UserNotificationService : IAT.Core.Services.IUserNotificationService
    {
        /// <summary>
        /// Shows a user notification message.
        /// </summary>
        /// <param name="message">The notification message.</param>
        public void ShowNotification(UserNotificationMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send(message);
            });
        }

        /// <summary>
        /// Shows an error notification message.
        /// </summary>
        /// <param name="messsage">The error message.</param>
        public void ShowError(ErrorNotificationMessage messsage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send(messsage);  
            });
        }
    }
}
