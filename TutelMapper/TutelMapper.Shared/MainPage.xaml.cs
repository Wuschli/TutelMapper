using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Barbar.HexGrid;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using TutelMapper.Tools;
using TutelMapper.Util;
using TutelMapper.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TutelMapper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private bool _dragging;
        private bool _pageIsActive;
        private bool _somethingChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public MainPageViewModel VM { get; } = new MainPageViewModel();

        private readonly BrushTool _brushTool = new BrushTool();

        private const float HexSize = 64f;

        public MainPage()
        {
            InitializeComponent();
            VM.HexGrid = HexLayoutFactory.CreateFlatHexLayout<SKPoint, SkPointPolicy>(new SKPoint(HexSize, HexSize), new SKPoint(0, 0), Offset.Even);
            VM.MapData = new string[40, 40];
            VM.PropertyChanged += (_, __) => _somethingChanged = true;
            //VM.UndoStack.Stack.CollectionChanged += (_, __) => HistoryScrollViewer.ChangeView(null, HistoryScrollViewer.ExtentHeight, null);
            HistoryListView.SizeChanged += (_, __) => HistoryScrollViewer.ChangeView(null, HistoryScrollViewer.ExtentHeight, null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var tilesPath = Path.Combine("Tiles");
            foreach (var tileFile in Directory.EnumerateFiles(tilesPath, "*.png", SearchOption.AllDirectories))
            {
                var tileName = tileFile;
                if (Path.HasExtension(tileName))
                    tileName = tileName.Substring(0, tileName.Length - Path.GetExtension(tileFile).Length);
                tileName = tileName.Substring(tilesPath.Length + 1);
                VM.TileLibrary.Add(new TileInfo
                {
                    Name = tileName,
                    ImagePath = tileFile
                });
            }

            _pageIsActive = true;
            DrawLoop();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _pageIsActive = false;
        }

        private async Task DrawLoop()
        {
            while (_pageIsActive)
            {
                if (_somethingChanged)
                {
                    Canvas.Invalidate();
                    _somethingChanged = false;
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0 / 60));
            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // the canvas and properties
            var canvas = e.Surface.Canvas;

            // get the screen density for scaling
            var display = DisplayInformation.GetForCurrentView();
            var scale = display.LogicalDpi / 96.0f;

            // handle the device screen density
            canvas.Scale(scale);

            // make sure the canvas is blank
            canvas.Clear(SKColors.Gray);

            // draw some text
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Center,
                TextSize = 12
            };

            //canvas.DrawText($"SkiaSharp", coord, paint);
            //canvas.DrawCircle(VM.CursorPosition, 10, paint);

            var gridPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            var gridHoveredPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                BlendMode = SKBlendMode.Multiply
            };

            canvas.Translate(VM.Offset);
            canvas.Scale(VM.Zoom);

            // draw hex grid
            var adjustedCursorPosition = new SKPoint((VM.CursorPosition.X - VM.Offset.X) / VM.Zoom, (VM.CursorPosition.Y - VM.Offset.Y) / VM.Zoom);
            var hoveredHex = VM.HexGrid.PixelToHex(adjustedCursorPosition).Round();

            for (int column = 0; column < VM.MapData.GetLength(0); column++)
            for (int row = 0; row < VM.MapData.GetLength(1); row++)
            {
                var cubeCoordinates = VM.HexGrid.ToCubeCoordinates(new OffsetCoordinates(column, row));
                var vertices = VM.HexGrid.PolygonCorners(cubeCoordinates);
                var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;
                var path = new SKPath();
                path.AddPoly(vertices.ToArray());
                if (hovered)
                    canvas.DrawPath(path, gridHoveredPaint);
                canvas.DrawPath(path, gridPaint);
            }

            // draw map data
            for (int row = 0; row < VM.MapData.GetLength(1); row++)
            {
                //draw odd tiles in row
                for (int column = 1; column < VM.MapData.GetLength(0); column += 2)
                {
                    DrawTile(column, row, hoveredHex, canvas, paint);
                }

                //draw even tiles in row
                for (int column = 0; column < VM.MapData.GetLength(0); column += 2)
                {
                    DrawTile(column, row, hoveredHex, canvas, paint);
                }
            }
        }

        private void DrawTile(int column, int row, CubeCoordinates hoveredHex, SKCanvas canvas, SKPaint paint)
        {
            var tileName = VM.MapData[column, row];
            var cubeCoordinates = VM.HexGrid.ToCubeCoordinates(new OffsetCoordinates(column, row));
            var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;
            var pixelCoordinates = VM.HexGrid.HexToPixel(cubeCoordinates);
            var rect = new SKRect(pixelCoordinates.X - HexSize, pixelCoordinates.Y - HexSize, pixelCoordinates.X + HexSize, pixelCoordinates.Y + HexSize);


            if (hovered && VM.SelectedTile != null)
            {
                var fillRect = rect.AspectFill(new SKSize(HexSize, HexSize * VM.SelectedTile.AspectRatio));
                var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + HexSize);
                fillRect.Location -= new SKPoint(0, verticalOffset);
                canvas.DrawImage(VM.SelectedTile.Image, fillRect);
            }
            else if (!string.IsNullOrEmpty(tileName))
            {
                var tileInfo = VM.TileLibrary.FirstOrDefault(info => info.Name == tileName);
                if (tileInfo != null)
                {
                    var fillRect = rect.AspectFill(new SKSize(HexSize, HexSize * tileInfo.AspectRatio));
                    var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + HexSize);
                    fillRect.Location -= new SKPoint(0, verticalOffset);
                    canvas.DrawImage(tileInfo.Image, fillRect);
                }
                else
                {
                    canvas.DrawText($"Tile not found!\n{tileName}", pixelCoordinates - new SKPoint(HexSize / 2, HexSize / 2), paint);
                }
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var newPosition = e.GetCurrentPoint(Canvas).Position.ToSKPoint();

            if (_dragging)
            {
                VM.Offset += newPosition - VM.CursorPosition;
            }

            VM.CursorPosition = newPosition;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(Canvas).Properties.MouseWheelDelta;
            var zoom = VM.Zoom + delta * 0.001f;
            if (zoom < 0.05f)
                zoom = 0.05f;
            if (zoom > 3)
                zoom = 3;

            // zoom to mouse pointer
            var worldPosition = new SKPoint((VM.CursorPosition.X - VM.Offset.X) / VM.Zoom, (VM.CursorPosition.Y - VM.Offset.Y) / VM.Zoom);
            var newOffset = new SKPoint((worldPosition.X * zoom - VM.CursorPosition.X) * -1f, (worldPosition.Y * zoom - VM.CursorPosition.Y) * -1f);
            VM.Offset = newOffset;

            VM.Zoom = zoom;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _dragging = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _dragging = false;
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            _dragging = false;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (VM.SelectedTile == null)
                return;

            var point = e.GetPosition(Canvas).ToSKPoint();

            var adjustedCursorPosition = new SKPoint((point.X - VM.Offset.X) / VM.Zoom, (point.Y - VM.Offset.Y) / VM.Zoom);
            var cubeCoordinates = VM.HexGrid.PixelToHex(adjustedCursorPosition).Round();
            var offsetCoordinates = VM.HexGrid.ToOffsetCoordinates(cubeCoordinates);
            if (offsetCoordinates.Row >= 0 && offsetCoordinates.Column >= 0 && offsetCoordinates.Column < VM.MapData.GetLength(0) && offsetCoordinates.Row < VM.MapData.GetLength(1))
            {
                _brushTool.Execute(VM.SelectedTile, VM.MapData, offsetCoordinates.Column, offsetCoordinates.Row, VM.UndoStack);
                //VM.MapData[offsetCoordinates.Column, offsetCoordinates.Row] = VM.SelectedTile.Name;
            }
        }
    }
}