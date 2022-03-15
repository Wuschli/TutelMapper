using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Zio;

namespace TutelMapper.Util.FileSystem;

public class BufferedZipFileSystem : Zio.FileSystems.FileSystem
{
    private readonly IBuffer _buffer;

    public BufferedZipFileSystem(IBuffer buffer)
    {
        _buffer = buffer;
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
        using var stream = _buffer.AsStream();
        return ZipHelper.FileExists(stream, path.FullName.TrimStart('/'));
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
        using var stream = _buffer.AsStream();
        if (access != FileAccess.Read)
            throw new UnauthorizedAccessException();
        return ZipHelper.OpenEntry(stream, path.FullName.TrimStart('/'));
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
        using var stream = _buffer.AsStream();

        foreach (var item in ZipHelper.EnumerateItems(this, stream, UPath.Root))
        {
            yield return item;
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
}