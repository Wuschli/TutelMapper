#nullable enable
using System.Threading.Tasks;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Commands;
using TutelMapper.Data;
using TutelMapper.Util;
using TutelMapper.ViewModels;

namespace TutelMapper.Tools;

public class BrushTool : ITool
{
    public bool CanUseOnDrag => true;
    public string Name => "Brush";
    public string Icon => "\uED63";

    public async Task Execute(ITileLibraryItem selectedTile, string?[,] target, int x, int y, UndoStack undoStack, bool isDrag)
    {
        if (isDrag && selectedTile.ContainsId(target[x, y]))
            return;
        var drawable = selectedTile.GetDrawableTile();
        if (drawable.Id == target[x, y])
            return;
        await undoStack.Do(new PlaceTileCommand(target, x, y, drawable));
        selectedTile.WasPlaced();
    }

    public bool CanPreview(ITileLibraryItem? selectedTile)
    {
        return selectedTile != null;
    }

    public void DrawPreview(SKCanvas canvas, MapLayer layer, CubeCoordinates cubeCoordinates, SKPoint pixelCoordinates, CubeCoordinates hoveredHex, float hexSize, ITileLibraryItem? selectedTile)
    {
        var drawableTile = selectedTile?.GetDrawableTile();
        if (drawableTile?.SkiaImage == null)
            return;
        var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;
        if (!hovered)
            return;
        var rect = new SKRect(pixelCoordinates.X - hexSize, pixelCoordinates.Y - hexSize, pixelCoordinates.X + hexSize, pixelCoordinates.Y + hexSize);
        var fillRect = rect.AspectFill(new SKSize(hexSize, hexSize * drawableTile.AspectRatio));
        var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + hexSize);
        fillRect.Location -= new SKPoint(0, verticalOffset);
        canvas.DrawImage(drawableTile.SkiaImage, fillRect);
    }
}