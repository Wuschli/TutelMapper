#nullable enable
using System;
using System.ComponentModel;
using System.IO;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Zio;

namespace TutelMapper.ViewModels;

public class SingleTileInfo : ITileLibraryItem, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private BitmapImage? _imageSource;
    public string DisplayName { get; }
    public string Id { get; }
    public HexType HexType { get; }
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
}