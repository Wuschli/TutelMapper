using System.Threading.Tasks;
using TutelMapper.Util;
using TutelMapper.ViewModels;

namespace TutelMapper.Tools
{
    public interface ITool
    {
        Task Execute(TileInfo selectedTile, string[,] target, int x, int y, UndoStack undoStack);
    }
}