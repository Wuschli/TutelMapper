using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Zio;

namespace TutelMapper.Util;

public static class ZipHelper
{
    public static IEnumerable<FileSystemItem> EnumerateItems(IFileSystem fileSystem, FileSystemItem zipFile, UPath basePath)
    {
        using var stream = zipFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        return EnumerateItems(fileSystem, stream, basePath);
    }

    public static IEnumerable<FileSystemItem> EnumerateItems(IFileSystem fileSystem, Stream stream, UPath basePath)
    {
        using var zipStream = new ZipInputStream(stream);
        ZipEntry entry;
        while ((entry = zipStream.GetNextEntry()) != null)
        {
            yield return new FileSystemItem(fileSystem, basePath / entry.Name, entry.IsDirectory);
        }
    }

    public static bool ZipSearchPredicate(ref FileSystemItem item)
    {
        return item.Path.GetExtensionWithDot()?.ToLowerInvariant() == ".zip";
    }

    public static Stream OpenEntry(FileSystemItem zipFile, string relativePathInsideZip)
    {
        using var stream = zipFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        return OpenEntry(stream, relativePathInsideZip);
    }

    public static Stream OpenEntry(Stream stream, string relativePathInsideZip)
    {
        using var zipStream = new ZipInputStream(stream);
        ZipEntry entry;

        while ((entry = zipStream.GetNextEntry()) != null)
        {
            if (entry.IsDirectory)
                continue;
            if (entry.Name != relativePathInsideZip)
                continue;
            var result = new MemoryStream(new byte[entry.Size]);
            zipStream.CopyStream(result, (int)entry.Size);
            result.Position = 0;
            return result;
        }

        throw new FileNotFoundException();
    }
}