using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Services;

namespace IAT_Design_WPF.Services
{
    public class UserNotificationService : IUserNotificationService
    {
        public void ShowError(ErrorNotificationMessage messsage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Send(messsage);  
            });
        }
    }
}
