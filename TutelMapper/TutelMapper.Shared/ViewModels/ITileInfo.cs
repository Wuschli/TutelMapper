using Windows.UI.Xaml.Media;
using SkiaSharp;

namespace TutelMapper.ViewModels;

public interface ITileInfo
{
    string Name { get; }
    ImageSource ImageSource { get; }
    SKImage SkiaImage { get; }
    float AspectRatio { get; }
}