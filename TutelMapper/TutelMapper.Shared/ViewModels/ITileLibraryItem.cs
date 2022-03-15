#nullable enable
using Windows.UI.Xaml.Media;
using SkiaSharp;

namespace TutelMapper.ViewModels;

public interface ITileSelectionItem
{
    string DisplayName { get; }
    ImageSource PreviewImage { get; }
}

public interface ITileLibraryItem : ITileSelectionItem
{
    string Id { get; }
    public HexType HexType { get; }
    IDrawableTile GetDrawableTile();
    void WasPlaced();
    bool ContainsId(string? tileId);
}

public interface IDrawableTile
{
    string DisplayName { get; }
    string Id { get; }
    SKImage SkiaImage { get; }
    float AspectRatio { get; }
}