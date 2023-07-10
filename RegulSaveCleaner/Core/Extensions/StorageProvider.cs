using Avalonia.Platform.Storage;
using PleasantUI.Controls;

namespace RegulSaveCleaner.Core.Extensions;

public static class StorageProvider
{
    public static async Task<string?> SelectFolder(PleasantWindow window, string? directory = null)
    {
        IReadOnlyList<IStorageFolder> result = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            SuggestedStartLocation = directory is null ? null : await window.StorageProvider.TryGetFolderFromPathAsync(new Uri(directory))
        });
        
        return result.Count == 0 ? null : result[0].Path.LocalPath;
    }
}