using IAT.Core.Services;
using Microsoft.Win32;
using System.Windows;

namespace IAT.Views
{
    /// <summary>
    /// WPF implementation of <see cref="IDialogService"/> — confirmation/notification windows
    /// and native open/save file dialogs. All UI work is marshalled to the dispatcher.
    /// </summary>
    public class DialogService : IDialogService
    {
        Task<bool> IDialogService.ShowConfirmationAsync(string message, string title)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new ConfirmationDialog(message, title);
                dialog.Owner = Application.Current.MainWindow;
                return dialog.ShowDialog() == true;
            }).Task;
        }

        Task IDialogService.ShowNotificationAsync(string message, string title)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new NotificationDialog(message, title);
                dialog.Owner = Application.Current.MainWindow;
                dialog.ShowDialog();
            }).Task;
        }

        Task<string?> IDialogService.ShowOpenFileDialogAsync(string filter, string title)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dlg = new OpenFileDialog
                {
                    Title = title,
                    Filter = filter,
                    CheckFileExists = true,
                    Multiselect = false
                };
                return dlg.ShowDialog(Application.Current.MainWindow) == true
                    ? dlg.FileName
                    : null;
            }).Task;
        }

        Task<string?> IDialogService.ShowSaveFileDialogAsync(string filter, string defaultFileName, string title)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dlg = new SaveFileDialog
                {
                    Title = title,
                    Filter = filter,
                    FileName = defaultFileName,
                    AddExtension = true,
                    OverwritePrompt = true
                };
                return dlg.ShowDialog(Application.Current.MainWindow) == true
                    ? dlg.FileName
                    : null;
            }).Task;
        }
    }
}
