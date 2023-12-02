namespace RegulSaveCleaner.Core.Extensions;

public static class DirectoryManager
{
    /// <summary>
    /// Copies the directory to another specified location
    /// </summary>
    /// <param name="sourceDirectoryPath">Path to the original directory</param>
    /// <param name="newDirectoryPath">New directory path</param>
    /// <param name="copySubDirs">Copy also all files from the main directory</param>
    /// <exception cref="DirectoryNotFoundException">Occurs if the source directory does not exist</exception>
    public static void Copy(string sourceDirectoryPath, string newDirectoryPath, bool copySubDirs)
    {
        DirectoryInfo dir = new(sourceDirectoryPath);

        if (!dir.Exists)
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirectoryPath);

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(newDirectoryPath);

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(newDirectoryPath, file.Name);
            file.CopyTo(tempPath, true);
        }

        if (!copySubDirs)
            return;
        
        foreach (DirectoryInfo subDir in dirs)
        {
            string tempPath = Path.Combine(newDirectoryPath, subDir.Name);
            Copy(subDir.FullName, tempPath, true);
        }
    }
}