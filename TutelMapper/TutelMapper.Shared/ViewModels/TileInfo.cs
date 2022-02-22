using System.ComponentModel;
using SkiaSharp;

namespace TutelMapper.ViewModels
{
    public class TileInfo : INotifyPropertyChanged
    {
        private SKImage _image;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name { get; set; }
        public string ImagePath { get; set; }

        public SKImage Image
        {
            get
            {
                if (_image == null)
                {
                    _image = SKImage.FromEncodedData(ImagePath);
                }

                return _image;
            }
        }

        public bool IsSelected { get; set; }
        public float AspectRatio => Image.Height / (float)Image.Width;
    }
}