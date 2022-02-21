using System;
using System.Threading.Tasks;
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

        public PlaceTileCommand(string[,] target, int x, int y, TileInfo tileInfo)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _tileInfo = tileInfo ?? throw new ArgumentNullException(nameof(tileInfo));
            _x = x;
            _y = y;
        }

        public Task Do()
        {
            _previousTile = _target[_x, _y];
            _target[_x, _y] = _tileInfo.Name;
            return Task.CompletedTask;
        }

        public Task Undo()
        {
            _target[_x, _y] = _previousTile;
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"Place {_tileInfo.Name} at [{_x}|{_y}]";
        }
    }
}