namespace IAT.Core.Services
{
    /// <summary>
    /// Displays confirmation, notification, and native file dialogs.
    /// Implementations live in the Views layer so Core/ViewModels stay free of WPF dialog types.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays a confirmation dialog and returns whether the user confirmed.
        /// </summary>
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");

        /// <summary>
        /// Displays an informational notification dialog.
        /// </summary>
        Task ShowNotificationAsync(string message, string title = "Notification");

        /// <summary>
        /// Shows a native open-file dialog. Returns the selected path, or null if the user cancelled.
        /// </summary>
        /// <param name="filter">Win32 filter string, e.g. "IAT Project (*.iat)|*.iat".</param>
        /// <param name="title">Dialog title.</param>
        Task<string?> ShowOpenFileDialogAsync(string filter, string title = "Open");

        /// <summary>
        /// Shows a native save-file dialog. Returns the chosen path, or null if the user cancelled.
        /// </summary>
        /// <param name="filter">Win32 filter string, e.g. "IAT Project (*.iat)|*.iat".</param>
        /// <param name="defaultFileName">Suggested file name (no path required).</param>
        /// <param name="title">Dialog title.</param>
        Task<string?> ShowSaveFileDialogAsync(string filter, string defaultFileName = "Untitled.iat", string title = "Save As");
    }
}
