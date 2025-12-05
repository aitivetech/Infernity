namespace Infernity.Framework.Core.Io;

public static class FileInfoExtensions
{
    extension(FileInfo fileInfo)
    {
        public FileInfo CreateMissingParentDirectories()
        {
            if (!fileInfo.Exists)
            {
                var parentDirectory = fileInfo.Directory;

                if (parentDirectory is { Exists: false })
                {
                    parentDirectory.Create();
                }
            }
            
            return fileInfo;
        }
    }
}