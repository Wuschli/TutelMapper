#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SkiaSharp;
using TutelMapper.Annotations;
using TutelMapper.Util;
using Zio;

namespace TutelMapper.ViewModels;

public class TileCollection : ITileLibraryItem, ITileSource, INotifyPropertyChanged
{
    private readonly Random _random;
    private int _currentTileIndex;
    public event PropertyChangedEventHandler? PropertyChanged;
    public string DisplayName { get; set; }
    public string Id { get; set; }
    public HexType HexType { get; set; }
    public ImageSource PreviewImage => Tiles.First().PreviewImage;

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
}

public class SingleTileInfo : ITileLibraryItem, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private BitmapImage? _imageSource;
    public string DisplayName { get; set; }
    public string Id { get; set; }
    public HexType HexType { get; set; }
    private readonly FileSystemItem _imageFile;
    private readonly DrawableTile _drawableTile;

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

    public SingleTileInfo(string displayName, string id, HexType hexType, FileSystemItem imageFile)
    {
        DisplayName = displayName;
        Id = id;
        HexType = hexType;
        _imageFile = imageFile;
        _drawableTile = new DrawableTile(DisplayName, id, imageFile);
    }

    public IDrawableTile GetDrawableTile()
    {
        return _drawableTile;
    }

    public void WasPlaced()
    {
    }

    public bool ContainsId(string? tileId)
    {
        return Id == tileId;
    }

    private async void SetImageSourceAsync()
    {
        using var fileStream = _imageFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        using var stream = fileStream.AsRandomAccessStream();
        stream.Seek(0);
        await _imageSource?.SetSourceAsync(stream);
    }

    [NotifyPropertyChangedInvocator]
    [UsedImplicitly]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DrawableTile : IDrawableTile
{
    private SKImage? _skiaImage;
    private readonly FileSystemItem _imageFile;


    public string DisplayName { get; }
    public string Id { get; }

    public SKImage SkiaImage
    {
        get
        {
            if (_skiaImage == null)
            {
                using var fileStream = _imageFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                _skiaImage = SKImage.FromEncodedData(fileStream);
            }

            return _skiaImage;
        }
    }

    public float AspectRatio => SkiaImage.Height / (float)SkiaImage.Width;

    public DrawableTile(string displayName, string id, FileSystemItem imageFile)
    {
        DisplayName = displayName;
        Id = id;
        _imageFile = imageFile;
    }
}