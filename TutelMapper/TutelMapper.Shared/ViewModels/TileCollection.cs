#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SkiaSharp;
using TutelMapper.Util;

namespace TutelMapper.ViewModels;

public class TileCollection : ITileLibraryItem, ITileSource, INotifyPropertyChanged
{
    private const int PreviewImageSize = 256;
    private readonly Random _random;
    private readonly byte[] _pixelBytes = new byte[PreviewImageSize * PreviewImageSize * 4 * 8];
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
        var bitmap = new SKBitmap(PreviewImageSize, PreviewImageSize);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear();

        // Create Collection Preview Image from 3 child Tiles using SkiaSharp

        Stream? stream;
        if (Tiles.Any())
        {
            for (int i = 2; i >= 0; i--)
            {
                using (stream = Tiles[i % Tiles.Count].GetPreviewImageStream())
                {
                    var subImage = SKBitmap.Decode(stream);
                    var destRect = new SKRect(10 + i * 20f, (10 + i * 20f), (210 + i * 20f), (210 + i * 20f));
                    canvas.DrawBitmap(subImage, destRect);
                }
            }
        }
        else
        {
            canvas.Clear(SKColors.Magenta);
        }

        stream = GetPreviewImageStream();
        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        stream.Position = 0;

        using var randomAccessStream = stream.AsRandomAccessStream();
        await _imageSource?.SetSourceAsync(randomAccessStream);
    }
}