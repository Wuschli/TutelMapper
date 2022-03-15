#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using TutelMapper.Data;
using TutelMapper.Util.FileSystem;
using TutelMapper.ViewModels;
using Zio;
using Zio.FileSystems;

namespace TutelMapper.Util;

public class TileLibrary
{
    private readonly Dictionary<string, ITileLibraryItem> _tileCache = new();

    public ObservableCollection<ITileLibraryItem> Tiles { get; } = new();
    private IFileSystem? FileSystem { get; set; }

    private JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task InitializeFileSystem()
    {
        var tilesZip = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/tiles.zip"));
        var tilesZipBuffer = await FileIO.ReadBufferAsync(tilesZip);

        var defaultTilesFileSystem = new BufferedZipFileSystem(tilesZipBuffer);

#if __WASM__
            FileSystem = defaultTilesFileSystem;
#else
        var physicalFileSystem = new PhysicalFileSystem();
        var tilesPath = physicalFileSystem.ConvertPathFromInternal(".") / "Tiles";
        var subFileSystem = new SubFileSystem(physicalFileSystem, tilesPath);
        var seamlessZipFileSystem = new SeamlessZipFileSystem(subFileSystem);
        var aggregateFileSystem = new AggregateFileSystem();
        aggregateFileSystem.AddFileSystem(defaultTilesFileSystem);
        aggregateFileSystem.AddFileSystem(seamlessZipFileSystem);
        FileSystem = aggregateFileSystem;
#endif
    }

    public async Task Load()
    {
        _tileCache.Clear();
        Tiles.Clear();

        await InitializeFileSystem();

        if (FileSystem == null)
            return;

        await LoadTilesRecursive(FileSystem, UPath.Root, Tiles);
    }

    private async Task LoadTilesRecursive(IFileSystem fileSystem, UPath path, ICollection<ITileLibraryItem> parent)
    {
        var manifestPath = path / "manifest.json";
        TilesetManifest? manifest = null;
        if (fileSystem.FileExists(manifestPath))
        {
            using var stream = fileSystem.OpenFile(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            manifest = await JsonSerializer.DeserializeAsync<TilesetManifest>(stream, SerializerOptions);
        }

        var hexType = manifest?.HexType ?? HexType.Flat;

        var collection = parent;

        if (path != UPath.Root)
        {
            var tileCollection = new TileCollection(path.GetName(), path.FullName, hexType);
            parent.Add(tileCollection);
            collection = tileCollection.Tiles;
        }

        foreach (var item in fileSystem.EnumerateItems(path, SearchOption.TopDirectoryOnly))
        {
            if (item.IsDirectory)
            {
                await LoadTilesRecursive(fileSystem, item.AbsolutePath, collection);
                continue;
            }

            if (item.Path.GetExtensionWithDot() != ".png")
                continue;

            var tileName = item.Path.GetFullPathWithoutExtension().ToRelative().FullName;
            var singleTileInfo = new SingleTileInfo(item.Path.GetNameWithoutExtension()!, tileName, hexType, item);
            collection.Add(singleTileInfo);
            _tileCache.Add(singleTileInfo.Id, singleTileInfo);
        }
    }


    public IDrawableTile? GetTile(string tileName)
    {
        if (_tileCache.TryGetValue(tileName, out var tileInfo))
            return tileInfo.GetDrawableTile();

        tileInfo = Tiles.FirstOrDefault(info => info.Id == tileName);
        if (tileInfo != null)
            _tileCache[tileName] = tileInfo;
        return tileInfo?.GetDrawableTile();
    }
}