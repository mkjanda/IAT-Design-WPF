using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services;
using System.Windows;

namespace IAT.Views
{
    public class DialogService : IDialogService
    {
        Task<bool> IDialogService.ShowConfirmationAsync(string message, string title = "Confirm")
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new ConfirmationDialog(message, title);
                dialog.Owner = Application.Current.MainWindow; // proper parenting
                return dialog.ShowDialog() == true;
            }).Task;
        }

        Task IDialogService.ShowNotificationAsync(string message, string title = "Notification")
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new NotificationDialog(message, title);
                dialog.Owner = Application.Current.MainWindow; // proper parenting
                dialog.ShowDialog();
            }).Task;
        }
    }
}
