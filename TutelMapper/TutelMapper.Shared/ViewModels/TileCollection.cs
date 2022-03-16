#nullable enable
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using TutelMapper.Util;

namespace TutelMapper.ViewModels;

public class TileCollection : ITileLibraryItem, ITileSource, INotifyPropertyChanged
{
    private readonly Random _random;
    private int _currentTileIndex;
    public event PropertyChangedEventHandler? PropertyChanged;
    public string DisplayName { get; }
    public string Id { get; }
    public HexType HexType { get; }
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