#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TutelMapper.Util;

namespace TutelMapper.ViewModels;

public class TileCollection : ITileLibraryItem, ITileSource, INotifyPropertyChanged
{
    private const int PreviewImageSize = 256;
    private readonly Random _random;
    private readonly byte[] _pixelBytes = new byte[PreviewImageSize * PreviewImageSize * Unsafe.SizeOf<Rgba32>()];
    private int _currentTileIndex;
    private BitmapImage? _imageSource;

    public event PropertyChangedEventHandler? PropertyChanged;
    public string DisplayName { get; }
    public string Id { get; }
    public HexType HexType { get; }

    public ImageSource PreviewImage
    {
        get
        {
            if (_imageSource == null)
            {
                _imageSource = new BitmapImage();
                SetImageSourceAsync();
            }

            return _imageSource;
        }
    }

    public ObservableCollection<ITileLibraryItem> Tiles { get; } = new();
    public ITileSource? Parent { get; }

    public TileCollection(string displayName, string id, HexType hexType, ITileSource parent)
    {
        DisplayName = displayName;
        Id = id;
        HexType = hexType;
        Parent = parent;
        _random = new Random(DateTime.UtcNow.Millisecond);
        WasPlaced();
    }

    public Stream GetPreviewImageStream()
    {
        return new MemoryStream(_pixelBytes);
    }

    public IDrawableTile GetDrawableTile()
    {
        return Tiles[_currentTileIndex].GetDrawableTile();
    }

    public void WasPlaced()
    {
        _currentTileIndex = _random.Next(Tiles.Count);
    }

    public bool ContainsId(string? tileId)
    {
        return tileId != null && Tiles.Any(item => item.ContainsId(tileId));
    }

    private async void SetImageSourceAsync()
    {
        var image = new Image<Rgba32>(PreviewImageSize, PreviewImageSize);

        // Create Collection Preview Image from 3 child Tiles using ImageSharp
        image.Mutate(x =>
        {
            if (Tiles.Any())
            {
                for (int i = 2; i >= 0; i--)
                {
                    using var stream = Tiles[i % Tiles.Count].GetPreviewImageStream();
                    var subImage = Image.Load(stream);
                    subImage.Mutate(y => y.Resize(200, 200));
                    x.DrawImage(subImage, new Point(10 + i * 20, 10 + i * 20), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver, 1);
                }
            }
            else
            {
                x.Fill(Color.Magenta);
            }
        });

        using var stream = GetPreviewImageStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;

        using var randomAccessStream = stream.AsRandomAccessStream();
        await _imageSource?.SetSourceAsync(randomAccessStream);
    }
}