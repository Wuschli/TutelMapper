using SkiaSharp;

namespace TutelMapper.ViewModels;

public interface IDrawableTile
{
    string DisplayName { get; }
    string Id { get; }
    SKImage SkiaImage { get; }
    float AspectRatio { get; }
    SKPoint Offset { get; }
}