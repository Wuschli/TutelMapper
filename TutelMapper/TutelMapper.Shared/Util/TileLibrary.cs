using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using TutelMapper.Util.FileSystem;
using TutelMapper.ViewModels;
using Zio;
using Zio.FileSystems;

namespace TutelMapper.Util
{
    public class TileLibrary
    {
        private readonly Dictionary<string, TileInfo> _tileCache = new Dictionary<string, TileInfo>();

        public ObservableCollection<TileInfo> Tiles { get; } = new ObservableCollection<TileInfo>();
        private IFileSystem? FileSystem { get; set; }

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

            foreach (var item in FileSystem.EnumerateItems(UPath.Root, SearchOption.AllDirectories).OrderBy(item => item.FullName))
            {
                if (item.IsDirectory || item.Path.GetExtensionWithDot() != ".png")
                    continue;

                var tileName = item.Path.GetFullPathWithoutExtension().ToRelative().FullName;
                Tiles.Add(new TileInfo
                {
                    Name = tileName,
                    ImageFile = item
                });
            }
        }

        public TileInfo? GetTile(string tileName)
        {
            if (_tileCache.TryGetValue(tileName, out var tileInfo))
                return tileInfo;

            tileInfo = Tiles.FirstOrDefault(info => info.Name == tileName);
            if (tileInfo != null)
                _tileCache[tileName] = tileInfo;
            return tileInfo;
        }
    }
}