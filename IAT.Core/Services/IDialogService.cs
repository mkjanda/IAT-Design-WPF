using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");
        Task ShowNotificationAsync(string message, string title = "Notification");
    }
}
