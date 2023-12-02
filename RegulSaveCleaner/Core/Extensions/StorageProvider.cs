using Avalonia.Platform.Storage;
using PleasantUI.Controls;

namespace RegulSaveCleaner.Core.Extensions;

public static class StorageProvider
{
    /// <summary>
    /// Opens the directory selection window in the system
    /// </summary>
    /// <param name="window">Window that will be locked when selecting directories</param>
    /// <param name="directory">The source directory that will be selected when the directory selection window is opened</param>
    /// <returns>Directory path</returns>
    public static async Task<string?> SelectDirectory(PleasantWindow window, string? directory = null)
    {
        IReadOnlyList<IStorageFolder> result = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            SuggestedStartLocation = directory is null ? null : await window.StorageProvider.TryGetFolderFromPathAsync(new Uri(directory))
        });
        
        return result.Count == 0 ? null : result[0].Path.LocalPath;
    }
}