using System.Collections.Generic;
using System.IO;

namespace DownloadManagerService
{
    public static class Utils
    {
        public static FileInfo GetFile(this DirectoryInfo obj, string filename) => new FileInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{filename}");

        public static IEnumerable<FileInfo> EnumerateFilesRecursively(this DirectoryInfo obj) => obj.EnumerateFiles("*", SearchOption.AllDirectories);
    }
}
