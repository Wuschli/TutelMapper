#nullable enable
using System.IO;
using SkiaSharp;
using Zio;

namespace TutelMapper.ViewModels;

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