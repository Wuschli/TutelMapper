#nullable enable
using System.Threading.Tasks;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Commands;
using TutelMapper.Data;
using TutelMapper.Util;
using TutelMapper.ViewModels;

namespace TutelMapper.Tools;

public class EraserTool : ITool
{
    public bool CanUseOnDrag => true;
    public string Name => "Eraser";
    public string Icon => "\uE75C";

    public async Task Execute(ITileLibraryItem selectedTile, string?[,] target, int x, int y, UndoStack undoStack, bool isDrag)
    {
        if (string.IsNullOrEmpty(target[x, y]))
            return;
        await undoStack.Do(new EraseTileCommand(target, x, y));
    }

    public bool CanPreview(ITileLibraryItem? selectedTile)
    {
        return true;
    }

    public void DrawPreview(SKCanvas canvas, MapLayer layer, CubeCoordinates cubeCoordinates, SKPoint pixelCoordinates, CubeCoordinates hoveredHex, float hexSize, ITileLibraryItem? selectedTile)
    {
    }
}