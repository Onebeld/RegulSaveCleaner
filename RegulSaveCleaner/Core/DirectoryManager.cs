namespace RegulSaveCleaner.Core;

public static class DirectoryManager
{
    public static void Copy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new(sourceDirName);

        if (!dir.Exists)
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDirName);

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subDir.Name);
                Copy(subDir.FullName, tempPath, true);
            }
        }
    }
}