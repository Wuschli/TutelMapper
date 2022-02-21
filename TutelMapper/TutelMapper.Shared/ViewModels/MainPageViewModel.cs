using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Annotations;
using TutelMapper.Util;

namespace TutelMapper.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SKPoint CursorPosition { get; set; }
        public float Zoom { get; set; } = 1f;
        public SKPoint Offset { get; set; } = new SKPoint(96f, 160f);
        public ObservableCollection<TileInfo> TileLibrary { get; } = new ObservableCollection<TileInfo>();
        public HexLayout<SKPoint, SkPointPolicy> HexGrid { get; set; }
        public string[,] MapData { get; set; }
        public TileInfo SelectedTile => TileLibrary.FirstOrDefault(info => info.IsSelected);
        public UndoStack UndoStack { get; } = new UndoStack();

        public async Task Undo()
        {
            await UndoStack.Undo();
            OnPropertyChanged();
        }

        public async Task Redo()
        {
            await UndoStack.Redo();
            OnPropertyChanged();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}