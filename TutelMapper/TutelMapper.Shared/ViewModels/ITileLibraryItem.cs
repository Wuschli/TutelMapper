#nullable enable
using Windows.UI.Xaml.Media;
using SkiaSharp;

namespace TutelMapper.ViewModels;

public interface ITileLibraryItem
{
    string DisplayName { get; }
    string Id { get; }
    public HexType HexType { get; }
    ImageSource PreviewImage { get; }
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