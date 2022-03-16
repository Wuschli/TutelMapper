#nullable enable
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Input;
using Barbar.HexGrid;
using SkiaSharp;
using TutelMapper.Data;
using TutelMapper.Tools;
using TutelMapper.ViewModels;

namespace TutelMapper.Util;

public class MapDataRenderer
{
    public static void DrawMapData(MapData mapData, HexLayout<SKPoint, SkPointPolicy> hexGrid, SKCanvas canvas, SKPoint offset, float zoom, IList<PointerInfo>? pointers, ITool? selectedTool, ITileLibraryItem? selectedTile)
    {
        // draw some text
        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 12
        };

        var gridPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        canvas.Translate(offset);
        canvas.Scale(zoom);

        // draw hex grid
        var pointerInfo = pointers?.FirstOrDefault();
        var hoveredHex = new CubeCoordinates(-1, -1, -1);
        if (pointerInfo != null && pointerInfo.Type != PointerDeviceType.Touch)
        {
            var adjustedCursorPosition = new SKPoint((pointerInfo.Position.X - offset.X) / zoom, (pointerInfo.Position.Y - offset.Y) / zoom);
            hoveredHex = hexGrid.PixelToHex(adjustedCursorPosition).Round();
        }

        for (int column = 0; column < mapData.Width; column++)
        for (int row = 0; row < mapData.Height; row++)
        {
            var cubeCoordinates = hexGrid.ToCubeCoordinates(new OffsetCoordinates(column, row));
            var vertices = hexGrid.PolygonCorners(cubeCoordinates);
            var path = new SKPath();
            path.AddPoly(vertices.ToArray());
            canvas.DrawPath(path, gridPaint);
        }

        // draw map data
        for (var layerIndex = mapData.Layers.Count - 1; layerIndex >= 0; layerIndex--)
        {
            var layer = mapData.Layers[layerIndex];
            if (!layer.IsVisible)
                continue;
            for (int row = 0; row < layer.Data.GetLength(1); row++)
            {
                //draw odd tiles in row
                for (int column = 1; column < layer.Data.GetLength(0); column += 2)
                {
                    DrawTile(mapData, hexGrid, layer, column, row, hoveredHex, canvas, paint, layerIndex == mapData.SelectedLayerIndex, selectedTool, selectedTile);
                }

                //draw even tiles in row
                for (int column = 0; column < layer.Data.GetLength(0); column += 2)
                {
                    DrawTile(mapData, hexGrid, layer, column, row, hoveredHex, canvas, paint, layerIndex == mapData.SelectedLayerIndex, selectedTool, selectedTile);
                }
            }
        }
    }

    private static void DrawTile(MapData mapData, HexLayout<SKPoint, SkPointPolicy> hexGrid, MapLayer layer, int column, int row, CubeCoordinates hoveredHex, SKCanvas canvas, SKPaint paint, bool isActiveLayer, ITool? selectedTool, ITileLibraryItem? selectedTile)
    {
        var tileId = layer.Data[column, row];
        var cubeCoordinates = hexGrid.ToCubeCoordinates(new OffsetCoordinates(column, row));
        var pixelCoordinates = hexGrid.HexToPixel(cubeCoordinates);
        var rect = new SKRect(pixelCoordinates.X - mapData.HexSize, pixelCoordinates.Y - mapData.HexSize, pixelCoordinates.X + mapData.HexSize, pixelCoordinates.Y + mapData.HexSize);
        var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;

        if (isActiveLayer && hovered && selectedTool != null && selectedTool.CanPreview(selectedTile))
        {
            selectedTool.DrawPreview(canvas, layer, cubeCoordinates, pixelCoordinates, hoveredHex, mapData.HexSize, selectedTile);
        }
        else if (!string.IsNullOrEmpty(tileId))
        {
            var drawableTile = App.TileLibrary.GetTile(tileId!);
            if (drawableTile != null)
            {
                var fillRect = rect.AspectFill(new SKSize(mapData.HexSize, mapData.HexSize * drawableTile.AspectRatio));
                var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + mapData.HexSize) + drawableTile.Offset.Y;
                fillRect.Location -= new SKPoint(drawableTile.Offset.X, verticalOffset);
                canvas.DrawImage(drawableTile.SkiaImage, fillRect);
            }
            else
            {
                canvas.DrawText($"Tile not found!\n{tileId}", pixelCoordinates - new SKPoint(mapData.HexSize / 2f, mapData.HexSize / 2f), paint);
            }
        }
    }
}