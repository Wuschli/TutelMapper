#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Barbar.HexGrid;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using TutelMapper.Annotations;
using TutelMapper.Data;
using TutelMapper.Dialogs;
using TutelMapper.Util;
using TutelMapper.ViewModels;

// ReSharper disable RedundantExtendsListEntry

namespace TutelMapper;

public sealed partial class EditorPage : Page, INotifyPropertyChanged
{
    private bool _pageIsActive;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPageViewModel VM { get; } = new();
    public uint? DraggingPointer { get; private set; }
    public uint? PaintingPointer { get; private set; }
    public ObservableCollection<PointerInfo> Pointers { get; } = new();

    public EditorPage()
    {
        InitializeComponent();
        VM.NewMap(40, 40, HexType.Flat, 64);

        VM.PropertyChanged += (_, _) => VM.SetDirty();
        HistoryListView.SizeChanged += (_, _) => HistoryScrollViewer.ChangeView(null, HistoryScrollViewer.ExtentHeight, null);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _ = VM.LoadTiles();

        _pageIsActive = true;
        _ = DrawLoop();
        SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += CloseRequested;
    }

    private async void CloseRequested(object? sender, SystemNavigationCloseRequestedPreviewEventArgs e)
    {
        if (!VM.UndoStack.HasUnsavedChanges)
            return;
        var deferral = e.GetDeferral();
        var dialog = new MessageDialog("The map is about to close, would you like to save your changes?", "Unsaved Changes");
        var yesCommand = new UICommand("Yes");
        var noCommand = new UICommand("No");
        var cancelCommand = new UICommand("Cancel");
        dialog.Commands.Add(yesCommand);
        dialog.Commands.Add(noCommand);
        dialog.Commands.Add(cancelCommand);

        var result = await dialog.ShowAsync();
        if (result == cancelCommand)
        {
            //cancel close by handling the event
            e.Handled = true;
        }

        if (result == yesCommand)
        {
            await VM.Save();
        }

        deferral.Complete();
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
            if (VM.SomethingChanged)
            {
                MapCanvas.Invalidate();
                VM.SomethingChanged = false;
            }

            await Task.Delay(TimeSpan.FromSeconds(1.0 / 60));
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        try
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
            if (VM.MapData == null)
                return;

            MapDataRenderer.DrawMapData(VM.MapData, VM.HexGrid, canvas, VM.Offset, VM.Zoom, Pointers, VM.SelectedTool, VM.SelectedTile);
        }
        catch (Exception ex)
        {
            var dialog = new MessageDialog(ex.Message, "Something went wrong :(");
            _ = dialog.ShowAsync();
        }
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(MapCanvas);
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
        VM.SetDirty();
    }

    private void OnPointerWheelScrolled(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(MapCanvas);
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
        var pointer = e.GetCurrentPoint(MapCanvas);
        if (pointer.PointerDevice.PointerDeviceType != PointerDeviceType.Touch && pointer.Properties.IsLeftButtonPressed)
            PaintingPointer = pointer.PointerId;
        else
            DraggingPointer = pointer.PointerId;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(MapCanvas);
        if (pointer.PointerId == DraggingPointer)
            DraggingPointer = null;

        if (pointer.PointerId == PaintingPointer)
            PaintingPointer = null;
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(MapCanvas);
        Pointers.Add(new PointerInfo(pointer.PointerId, pointer.PointerDevice.PointerDeviceType)
        {
            Position = pointer.Position.ToSKPoint()
        });
        if (e.GetCurrentPoint(MapCanvas).PointerDevice.PointerDeviceType == PointerDeviceType.Touch)
            DraggingPointer = pointer.PointerId;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint(MapCanvas);

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

        var point = e.GetPosition(MapCanvas).ToSKPoint();

        ExecuteSelectedToolAt(point);
    }

    private void ExecuteSelectedToolAt(SKPoint point, bool isDrag = false)
    {
        if (VM.SelectedTile == null)
            return;
        if (VM.SelectedTool == null)
            return;
        if (VM.MapData == null)
            return;
        if (!VM.SelectedTool.CanUseOnDrag && isDrag)
            return;
        var adjustedCursorPosition = new SKPoint((point.X - VM.Offset.X) / VM.Zoom, (point.Y - VM.Offset.Y) / VM.Zoom);
        var cubeCoordinates = VM.HexGrid.PixelToHex(adjustedCursorPosition).Round();
        var offsetCoordinates = VM.HexGrid.ToOffsetCoordinates(cubeCoordinates);
        var layer = VM.MapData.Layers[VM.MapData.SelectedLayerIndex];
        if (offsetCoordinates.Row >= 0 && offsetCoordinates.Column >= 0 && offsetCoordinates.Column < layer.Data.GetLength(0) && offsetCoordinates.Row < layer.Data.GetLength(1))
        {
            VM.SelectedTool.Execute(VM.SelectedTile, layer.Data, offsetCoordinates.Column, offsetCoordinates.Row, VM.UndoStack, isDrag)
                .ContinueWith(_ => VM.SetDirty());
        }
    }

    private void GoBack()
    {
        App.TryGoBack();
    }

    private void DeleteLayer(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is MapLayer layer)
            VM.MapData?.DeleteLayer(layer);
    }

    private void SetDirty(object sender, RoutedEventArgs e)
    {
        VM.SetDirty();
    }

    [NotifyPropertyChangedInvocator]
    [UsedImplicitly]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task NewMap()
    {
        var dialog = new NewMapDialog(VM);
        await dialog.ShowAsync();
    }

    public async Task Export()
    {
        if (VM.MapData == null)
            return;
        var canvasWidth = VM.MapData.PixelWidth + VM.MapData.HexPixelWidth * 2; // 2 Hexes Padding
        var canvasHeight = VM.MapData.PixelHeight + VM.MapData.HexPixelHeight * 2; // 2 Hexes Padding
        var bitmap = new SKBitmap((int)canvasWidth, (int)canvasHeight);
        using var canvas = new SKCanvas(bitmap);
        var offset = VM.MapData.DefaultOffset + new SKPoint(VM.MapData.HexPixelWidth, VM.MapData.HexPixelHeight); // 1 Hex extra offset for Padding
        MapDataRenderer.DrawMapData(VM.MapData, VM.HexGrid, canvas, offset, 1, null, null, null);

        var savePicker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
        };
        // Dropdown of file types the user can save the file as
        savePicker.FileTypeChoices.Add("png image", new List<string> { ".png" });
        // Default file name if the user does not type one in or select a file to replace
        savePicker.SuggestedFileName = "Exported Map";
        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            using var stream = await file.OpenStreamForWriteAsync();
            bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        }
    }

    public async Task OpenGitHub()
    {
        // The URI to launch
        var uri = new Uri(@"https://github.com/Wuschli/TutelMapper");

        // Launch the URI
        await Windows.System.Launcher.LaunchUriAsync(uri);
    }
}