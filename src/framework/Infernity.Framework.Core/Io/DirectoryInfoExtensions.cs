namespace Infernity.Framework.Core.Io;

public static class DirectoryInfoExtensions
{
    extension(DirectoryInfo directoryInfo)
    {
        public bool ContainsAnyFileWithExtension(IReadOnlyList<string> extensions)
        {
            foreach (var file in directoryInfo.EnumerateFiles("*.*",
                         new EnumerationOptions() { RecurseSubdirectories = true, }))
            {
                if (extensions.Contains(file.Extension))
                {
                    return true;
                }
            }

            return false;
        }
    }
}