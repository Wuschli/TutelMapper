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
    private readonly Dictionary<string, ITileInfo> _tileCache = new();

    public ObservableCollection<ITileInfo> Tiles { get; } = new();
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

        await LoadTilesRecursive(FileSystem, UPath.Root);
    }

    private async Task LoadTilesRecursive(IFileSystem fileSystem, UPath path)
    {
        var manifestPath = path / "manifest.json";
        TilesetManifest? manifest = null;
        if (fileSystem.FileExists(manifestPath))
        {
            using var stream = fileSystem.OpenFile(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            manifest = await JsonSerializer.DeserializeAsync<TilesetManifest>(stream, SerializerOptions);
        }

        foreach (var item in fileSystem.EnumerateItems(path, SearchOption.TopDirectoryOnly))
        {
            if (item.IsDirectory)
            {
                await LoadTilesRecursive(fileSystem, item.AbsolutePath);
                continue;
            }

            if (item.Path.GetExtensionWithDot() != ".png")
                continue;

            var tileName = item.Path.GetFullPathWithoutExtension().ToRelative().FullName;
            Tiles.Add(new SingleTileInfo
            {
                Name = tileName,
                ImageFile = item,
                HexType = manifest?.HexType ?? HexType.Flat
            });
        }
    }


    public ITileInfo? GetTile(string tileName)
    {
        if (_tileCache.TryGetValue(tileName, out var tileInfo))
            return tileInfo;

        tileInfo = Tiles.FirstOrDefault(info => info.Name == tileName);
        if (tileInfo != null)
            _tileCache[tileName] = tileInfo;
        return tileInfo;
    }
}