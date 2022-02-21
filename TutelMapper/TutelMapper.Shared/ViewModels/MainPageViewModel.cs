using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Util;

namespace TutelMapper.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public SKPoint CursorPosition { get; set; }
        public float Zoom { get; set; } = 1f;
        public SKPoint Offset { get; set; } = new SKPoint(96f, 160f);
        public ObservableCollection<TileInfo> TileLibrary { get; } = new ObservableCollection<TileInfo>();
        public HexLayout<SKPoint, SkPointPolicy> HexGrid { get; set; }
        public string[,] MapData { get; set; }
        public TileInfo SelectedTile => TileLibrary.FirstOrDefault(info => info.IsSelected);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}