using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Messages;
using IAT.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace IAT_Design_WPF.Services
{
    public class UserNotificationService : IAT.Core.Services.IUserNotificationService
    {
        public void ShowNotification(UserNotificationMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send(message);
            });
        }

        public void ShowError(ErrorNotificationMessage messsage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send(messsage);  
            });
        }
    }
}
