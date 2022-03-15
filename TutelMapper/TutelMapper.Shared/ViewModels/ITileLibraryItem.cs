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
}

public interface IDrawableTile
{
    string DisplayName { get; }
    string Id { get; }
    SKImage SkiaImage { get; }
    float AspectRatio { get; }
}