#nullable enable
using System.Threading.Tasks;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Data;
using TutelMapper.Util;
using TutelMapper.ViewModels;

namespace TutelMapper.Tools;

public interface ITool
{
    bool CanUseOnDrag { get; }
    string Name { get; }
    string Icon { get; }
    Task Execute(ITileLibraryItem selectedTile, string?[,] target, int x, int y, UndoStack undoStack, bool isDrag);
    bool CanPreview(ITileLibraryItem? selectedTile);
    void DrawPreview(SKCanvas canvas, MapLayer layer, CubeCoordinates cubeCoordinates, SKPoint pixelCoordinates, CubeCoordinates hoveredHex, float hexSize, ITileLibraryItem? selectedTile);
}