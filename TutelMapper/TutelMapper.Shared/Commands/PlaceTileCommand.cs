using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TutelMapper.Annotations;
using TutelMapper.ViewModels;

namespace TutelMapper.Commands
{
    public class PlaceTileCommand : ICommand
    {
        private readonly string[,] _target;
        private readonly int _x;
        private readonly int _y;
        private readonly TileInfo _tileInfo;

        private string _previousTile;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlaceTileCommand(string[,] target, int x, int y, TileInfo tileInfo)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _tileInfo = tileInfo ?? throw new ArgumentNullException(nameof(tileInfo));
            _x = x;
            _y = y;
        }

        public bool IsApplied { get; private set; }

        public Task Do()
        {
            if (IsApplied)
                return Task.CompletedTask;
            _previousTile = _target[_x, _y];
            _target[_x, _y] = _tileInfo.Name;
            IsApplied = true;
            return Task.CompletedTask;
        }

        public Task Undo()
        {
            if (!IsApplied)
                return Task.CompletedTask;
            _target[_x, _y] = _previousTile;
            IsApplied = false;
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"Place {_tileInfo.Name} at [{_x}|{_y}]";
        }

        [NotifyPropertyChangedInvocator]
        [UsedImplicitly]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}