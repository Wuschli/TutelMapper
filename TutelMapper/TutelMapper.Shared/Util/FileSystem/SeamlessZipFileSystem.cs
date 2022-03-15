using System;
using System.Collections.Generic;
using System.IO;
using Zio;

namespace TutelMapper.Util.FileSystem;

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
        if (_fileSystem.FileExists(path))
            return true;

        var zipFile = GetZipFileForPath(path);
        if (zipFile == null)
            return false;

        var relativePathInsideZip = path.FullName.Substring(zipFile.Value.Path.GetFullPathWithoutExtension().FullName.Length);

        return ZipHelper.FileExists(zipFile.Value, relativePathInsideZip.TrimStart('/'));
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