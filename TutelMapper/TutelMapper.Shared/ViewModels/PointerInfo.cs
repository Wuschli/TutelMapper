using System.ComponentModel;
using Windows.Devices.Input;
using SkiaSharp;

namespace TutelMapper.ViewModels
{
    public class PointerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public uint PointerId { get; }
        public PointerDeviceType Type { get; }
        public SKPoint Position { get; set; }

        public PointerInfo(uint pointerId, PointerDeviceType type)
        {
            PointerId = pointerId;
            Type = type;
        }

        public override string ToString()
        {
            return $"{PointerId}: {Position}";
        }
    }
}