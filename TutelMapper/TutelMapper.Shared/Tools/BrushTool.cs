﻿using System.Threading.Tasks;
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
    public bool IsSelected { get; set; }

    public async Task Execute(TileInfo selectedTile, string[,] target, int x, int y, UndoStack undoStack)
    {
        if (target[x, y] == selectedTile.Name)
            return;
        await undoStack.Do(new PlaceTileCommand(target, x, y, selectedTile));
    }

    public void DrawPreview(SKCanvas canvas, MapLayer layer, CubeCoordinates cubeCoordinates, SKPoint pixelCoordinates, CubeCoordinates hoveredHex, float hexSize, TileInfo? selectedTile)
    {
        if (selectedTile == null)
            return;
        var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;
        if (!hovered)
            return;
        var rect = new SKRect(pixelCoordinates.X - hexSize, pixelCoordinates.Y - hexSize, pixelCoordinates.X + hexSize, pixelCoordinates.Y + hexSize);
        var fillRect = rect.AspectFill(new SKSize(hexSize, hexSize * selectedTile.AspectRatio));
        var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + hexSize);
        fillRect.Location -= new SKPoint(0, verticalOffset);
        canvas.DrawImage(selectedTile.SkiaImage, fillRect);
    }
}