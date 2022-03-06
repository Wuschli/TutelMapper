using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SkiaSharp;
using TutelMapper.Annotations;
using Zio;

namespace TutelMapper.ViewModels
{
    public class TileInfo : INotifyPropertyChanged
    {
        private SKImage? _skiaImage;
        private BitmapImage _imageSource;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        public FileSystemItem ImageFile { get; set; }

        public ImageSource ImageSource
        {
            get
            {
                if (_imageSource == null)
                {
                    _imageSource = new BitmapImage();
                    using var fileStream = ImageFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var stream = fileStream.AsRandomAccessStream();
                    stream.Seek(0);
                    _ = _imageSource.SetSourceAsync(stream);
                }

                return _imageSource;
            }
        }

        public SKImage SkiaImage
        {
            get
            {
                if (_skiaImage == null)
                {
                    using var fileStream = ImageFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    _skiaImage = SKImage.FromEncodedData(fileStream);
                }

                return _skiaImage;
            }
        }

        public bool IsSelected { get; set; }
        public float AspectRatio => SkiaImage.Height / (float)SkiaImage.Width;

        [NotifyPropertyChangedInvocator]
        [UsedImplicitly]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}