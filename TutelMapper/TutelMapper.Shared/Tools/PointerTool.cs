#nullable enable
using System.Threading.Tasks;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Data;
using TutelMapper.Util;
using TutelMapper.ViewModels;

namespace TutelMapper.Tools;

public class PointerTool : ITool
{
    public bool CanUseOnDrag => true;
    public string Name => "Pointer";
    public string Icon => "\uF271";

    public Task Execute(ITileLibraryItem selectedTile, string?[,] target, int x, int y, UndoStack undoStack)
    {
        return Task.CompletedTask;
    }

    public bool CanPreview(ITileLibraryItem? selectedTile)
    {
        return false;
    }

    public void DrawPreview(SKCanvas canvas, MapLayer layer, CubeCoordinates cubeCoordinates, SKPoint pixelCoordinates, CubeCoordinates hoveredHex, float hexSize, ITileLibraryItem? selectedTile)
    {
    }
}