using System.Collections.Generic;
using System.IO;

namespace DownloadsManagerService
{
    public static class Utils
    {
        public static IEnumerable<FileInfo> EnumerateFilesRecursively(this DirectoryInfo obj) => obj.EnumerateFiles("*", SearchOption.AllDirectories);
    }
}
