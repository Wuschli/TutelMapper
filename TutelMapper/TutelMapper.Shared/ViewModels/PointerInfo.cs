#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Input;
using SkiaSharp;
using TutelMapper.Annotations;

namespace TutelMapper.ViewModels
{
    public class PointerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
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

        [NotifyPropertyChangedInvocator]
        [UsedImplicitly]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}