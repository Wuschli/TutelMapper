using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Zio;

namespace TutelMapper.Util.FileSystem
{
    public static class ZipHelper
    {
        public static IEnumerable<FileSystemItem> EnumerateItems(IFileSystem fileSystem, FileSystemItem zipFile, UPath basePath)
        {
            using var stream = zipFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
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

    public class SeamlessZipFileSystem : Zio.FileSystems.FileSystem
    {
        private readonly IFileSystem _fileSystem;

        public SeamlessZipFileSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            throw new NotImplementedException();
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool FileExistsImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteFileImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            if (_fileSystem.FileExists(path))
                return _fileSystem.OpenFile(path, mode, access, share);
            var zipFile = GetZipFileForPath(path);
            if (zipFile == null)
                throw new FileNotFoundException();
            var relativePathInsideZip = path.FullName.Substring(zipFile.Value.Path.GetFullPathWithoutExtension().FullName.Length);
            if (access != FileAccess.Read)
                throw new UnauthorizedAccessException();
            return ZipHelper.OpenEntry(zipFile.Value, relativePathInsideZip.TrimStart('/'));
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<FileSystemItem> EnumerateItemsImpl(UPath path, SearchOption searchOption, SearchPredicate? searchPredicate)
        {
            foreach (var item in _fileSystem.EnumerateItems(path, searchOption, searchPredicate))
            {
                yield return item;
            }

            foreach (var zipFile in _fileSystem.EnumerateItems(UPath.Root, SearchOption.AllDirectories, ZipHelper.ZipSearchPredicate))
            {
                var zipFilePath = zipFile.Path.GetFullPathWithoutExtension().FullName;
                if (path.FullName.StartsWith(zipFilePath) || (searchOption == SearchOption.AllDirectories && zipFilePath.StartsWith(path.FullName)))
                    foreach (var item in ZipHelper.EnumerateItems(this, zipFile, zipFilePath))
                    {
                        yield return item;
                    }
            }
        }

        protected override IFileSystemWatcher WatchImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override string ConvertPathToInternalImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            throw new NotImplementedException();
        }

        private FileSystemItem? GetZipFileForPath(UPath path)
        {
            foreach (var zipFile in _fileSystem.EnumerateItems(UPath.Root, SearchOption.AllDirectories, ZipHelper.ZipSearchPredicate))
            {
                var zipFilePath = zipFile.Path.GetFullPathWithoutExtension().FullName;
                if (path.FullName.StartsWith(zipFilePath))
                    return zipFile;
            }

            return null;
        }
    }

    public static class UPathExtensions
    {
        public static UPath GetFullPathWithoutExtension(this UPath path)
        {
            var extension = path.GetExtensionWithDot();
            if (string.IsNullOrEmpty(extension))
                return path;
            return path.FullName.Substring(0, path.FullName.Length - extension.Length);
        }
    }

    public static class StreamExtensions
    {
        public static void CopyStream(this Stream input, Stream output, int bytes, int bufferSize = 32768)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
            output.Flush();
        }
    }
}