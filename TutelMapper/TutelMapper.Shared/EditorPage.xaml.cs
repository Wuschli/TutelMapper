using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Barbar.HexGrid;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using TutelMapper.Annotations;
using TutelMapper.Data;
using TutelMapper.Tools;
using TutelMapper.Util;
using TutelMapper.ViewModels;

// ReSharper disable RedundantExtendsListEntry

namespace TutelMapper
{
    public sealed partial class EditorPage : Page, INotifyPropertyChanged
    {
        private bool _pageIsActive;
        private bool _somethingChanged;

        private const float HexSize = 64f;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainPageViewModel VM { get; } = new MainPageViewModel();
        public uint? DraggingPointer { get; private set; }
        public uint? PaintingPointer { get; private set; }
        public ObservableCollection<PointerInfo> Pointers { get; } = new ObservableCollection<PointerInfo>();
        public TileLibrary TileLibrary => App.TileLibrary;

        public EditorPage()
        {
            InitializeComponent();
            VM.HexGrid = HexLayoutFactory.CreateFlatHexLayout<SKPoint, SkPointPolicy>(new SKPoint(HexSize, HexSize), new SKPoint(0, 0), Offset.Even);
            VM.New();
            VM.SelectedTool = new BrushTool();

            VM.PropertyChanged += (_, _) => _somethingChanged = true;
            HistoryListView.SizeChanged += (_, _) => HistoryScrollViewer.ChangeView(null, HistoryScrollViewer.ExtentHeight, null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _ = Load();

            _pageIsActive = true;
            _ = DrawLoop();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _pageIsActive = false;
        }

        private async Task Load()
        {
            try
            {
                await App.TileLibrary.Load();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

            var gridPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.Translate(VM.Offset);
            canvas.Scale(VM.Zoom);

            // draw hex grid
            var pointerInfo = Pointers.FirstOrDefault();
            var hoveredHex = new CubeCoordinates(-1, -1, -1);
            if (pointerInfo != null && pointerInfo.Type != PointerDeviceType.Touch)
            {
                var adjustedCursorPosition = new SKPoint((pointerInfo.Position.X - VM.Offset.X) / VM.Zoom, (pointerInfo.Position.Y - VM.Offset.Y) / VM.Zoom);
                hoveredHex = VM.HexGrid.PixelToHex(adjustedCursorPosition).Round();
            }

            for (int column = 0; column < VM.MapData.Width; column++)
            for (int row = 0; row < VM.MapData.Height; row++)
            {
                var cubeCoordinates = VM.HexGrid.ToCubeCoordinates(new OffsetCoordinates(column, row));
                var vertices = VM.HexGrid.PolygonCorners(cubeCoordinates);
                var path = new SKPath();
                path.AddPoly(vertices.ToArray());
                canvas.DrawPath(path, gridPaint);
            }

            // draw map data
            for (var layerIndex = 0; layerIndex < VM.MapData.Layers.Count; layerIndex++)
            {
                var layer = VM.MapData.Layers[layerIndex];
                for (int row = 0; row < layer.Data.GetLength(1); row++)
                {
                    //draw odd tiles in row
                    for (int column = 1; column < layer.Data.GetLength(0); column += 2)
                    {
                        DrawTile(layer, column, row, hoveredHex, canvas, paint, layerIndex == VM.MapData.SelectedLayerIndex);
                    }

                    //draw even tiles in row
                    for (int column = 0; column < layer.Data.GetLength(0); column += 2)
                    {
                        DrawTile(layer, column, row, hoveredHex, canvas, paint, layerIndex == VM.MapData.SelectedLayerIndex);
                    }
                }
            }
        }

        private void DrawTile(MapLayer layer, int column, int row, CubeCoordinates hoveredHex, SKCanvas canvas, SKPaint paint, bool isActiveLayer)
        {
            var tileName = layer.Data[column, row];
            var cubeCoordinates = VM.HexGrid.ToCubeCoordinates(new OffsetCoordinates(column, row));
            var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;
            var pixelCoordinates = VM.HexGrid.HexToPixel(cubeCoordinates);
            var rect = new SKRect(pixelCoordinates.X - HexSize, pixelCoordinates.Y - HexSize, pixelCoordinates.X + HexSize, pixelCoordinates.Y + HexSize);

            if (isActiveLayer && hovered && VM.SelectedTile != null)
            {
                var fillRect = rect.AspectFill(new SKSize(HexSize, HexSize * VM.SelectedTile.AspectRatio));
                var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + HexSize);
                fillRect.Location -= new SKPoint(0, verticalOffset);
                canvas.DrawImage(VM.SelectedTile.SkiaImage, fillRect);
            }
            else if (!string.IsNullOrEmpty(tileName))
            {
                var tileInfo = App.TileLibrary.GetTile(tileName);
                if (tileInfo != null)
                {
                    var fillRect = rect.AspectFill(new SKSize(HexSize, HexSize * tileInfo.AspectRatio));
                    var verticalOffset = fillRect.Bottom - (pixelCoordinates.Y + HexSize);
                    fillRect.Location -= new SKPoint(0, verticalOffset);
                    canvas.DrawImage(tileInfo.SkiaImage, fillRect);
                }
                else
                {
                    canvas.DrawText($"Tile not found!\n{tileName}", pixelCoordinates - new SKPoint(HexSize / 2, HexSize / 2), paint);
                }
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            var newPosition = pointer.Position.ToSKPoint();
            var pointerInfo = Pointers.FirstOrDefault(info => info.PointerId == pointer.PointerId);

            if (pointerInfo == null)
                return;

            if (DraggingPointer == pointer.PointerId)
            {
                VM.Offset += newPosition - pointerInfo.Position;
            }

            if (PaintingPointer == pointer.PointerId)
            {
                ExecuteSelectedToolAt(pointer.Position.ToSKPoint(), true);
            }

            pointerInfo.Position = newPosition;
            _somethingChanged = true;
        }

        private void OnPointerWheelScrolled(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            var pointerInfo = Pointers.FirstOrDefault(info => info.PointerId == pointer.PointerId);
            if (pointerInfo == null)
                return;

            var delta = pointer.Properties.MouseWheelDelta;

            var zoom = VM.Zoom + delta * 0.001f;
            if (zoom < 0.05f)
                zoom = 0.05f;
            if (zoom > 3)
                zoom = 3;

            // zoom to mouse pointer
            var worldPosition = new SKPoint((pointerInfo.Position.X - VM.Offset.X) / VM.Zoom, (pointerInfo.Position.Y - VM.Offset.Y) / VM.Zoom);
            var newOffset = new SKPoint((worldPosition.X * zoom - pointerInfo.Position.X) * -1f, (worldPosition.Y * zoom - pointerInfo.Position.Y) * -1f);
            VM.Offset = newOffset;

            VM.Zoom = zoom;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            if (pointer.PointerDevice.PointerDeviceType != PointerDeviceType.Touch && pointer.Properties.IsLeftButtonPressed)
                PaintingPointer = pointer.PointerId;
            else
                DraggingPointer = pointer.PointerId;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            if (pointer.PointerId == DraggingPointer)
                DraggingPointer = null;

            if (pointer.PointerId == PaintingPointer)
                PaintingPointer = null;
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            Pointers.Add(new PointerInfo(pointer.PointerId, pointer.PointerDevice.PointerDeviceType)
            {
                Position = pointer.Position.ToSKPoint()
            });
            if (e.GetCurrentPoint(Canvas).PointerDevice.PointerDeviceType == PointerDeviceType.Touch)
                DraggingPointer = pointer.PointerId;
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);

            var pointerInfo = Pointers.FirstOrDefault(info => info.PointerId == pointer.PointerId);
            if (pointerInfo != null)
                Pointers.Remove(pointerInfo);

            if (pointer.PointerId == DraggingPointer)
                DraggingPointer = null;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (VM.SelectedTile == null)
                return;

            var point = e.GetPosition(Canvas).ToSKPoint();

            ExecuteSelectedToolAt(point);
        }

        private void ExecuteSelectedToolAt(SKPoint point, bool isDrag = false)
        {
            if (VM.SelectedTile == null)
                return;
            if (VM.SelectedTool == null)
                return;
            if (!VM.SelectedTool.CanUseOnDrag && isDrag)
                return;
            var adjustedCursorPosition = new SKPoint((point.X - VM.Offset.X) / VM.Zoom, (point.Y - VM.Offset.Y) / VM.Zoom);
            var cubeCoordinates = VM.HexGrid.PixelToHex(adjustedCursorPosition).Round();
            var offsetCoordinates = VM.HexGrid.ToOffsetCoordinates(cubeCoordinates);
            var layer = VM.MapData.Layers[VM.MapData.SelectedLayerIndex];
            if (offsetCoordinates.Row >= 0 && offsetCoordinates.Column >= 0 && offsetCoordinates.Column < layer.Data.GetLength(0) && offsetCoordinates.Row < layer.Data.GetLength(1))
            {
                VM.SelectedTool.Execute(VM.SelectedTile, layer.Data, offsetCoordinates.Column, offsetCoordinates.Row, VM.UndoStack)
                    .ContinueWith(_ => _somethingChanged = true);
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            App.TryGoBack();
        }

        [NotifyPropertyChangedInvocator]
        [UsedImplicitly]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}