#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;
using Barbar.HexGrid;
using MessagePack;
using Microsoft.Toolkit.Uwp.UI;
using SkiaSharp;
using TutelMapper.Annotations;
using TutelMapper.Data;
using TutelMapper.Tools;
using TutelMapper.Util;

namespace TutelMapper.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private MapData? _mapData;
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool SomethingChanged { get; set; }
    public float Zoom { get; set; } = 1f;
    public SKPoint Offset { get; set; }
    public HexLayout<SKPoint, SkPointPolicy> HexGrid { get; private set; }
    public AdvancedCollectionView Tiles { get; } = new(App.TileLibrary.Tiles, true);

    public MapData? MapData
    {
        get => _mapData;
        private set
        {
            if (_mapData != null)
                _mapData.Layers.CollectionChanged -= SetDirty;
            _mapData = value;
            if (_mapData != null)
                _mapData.Layers.CollectionChanged += SetDirty;
            CreateHexGrid();
            Tiles.RefreshFilter();
        }
    }

    public ITileLibraryItem? SelectedTile { get; set; }
    public ITool? SelectedTool { get; set; }
    public List<ITool> Tools { get; } = new() { new BrushTool(), new EraserTool(), new PointerTool() };
    public UndoStack UndoStack { get; } = new();

    public MainPageViewModel()
    {
        SelectedTool = Tools[0];
        Tiles.SortDescriptions.Add(new SortDescription(nameof(ITileLibraryItem.Id), SortDirection.Ascending));
        Tiles.Filter = TilesFilter;
    }

    public void SetDirty()
    {
        SomethingChanged = true;
    }

    private void SetDirty(object sender, NotifyCollectionChangedEventArgs e)
    {
        SetDirty();
    }

    public void NewMap(int width, int height, HexType hexType, int hexSize)
    {
        UndoStack.Clear();
        MapData = new MapData { Width = width, Height = height, HexType = hexType, HexSize = hexSize };
        MapData.AddLayer("Layer 1");
        MapData.SelectedLayerIndex = 0;
        Zoom = 1f;
        Offset = new SKPoint(96f, 160f);
    }

    private void CreateHexGrid()
    {
        if (MapData == null)
            return;

        switch (MapData.HexType)
        {
            case HexType.Flat:
                HexGrid = HexLayoutFactory.CreateFlatHexLayout<SKPoint, SkPointPolicy>(new SKPoint(MapData.HexSize, MapData.HexSize), new SKPoint(0, 0), Barbar.HexGrid.Offset.Even);
                break;
            case HexType.Pointy:
                HexGrid = HexLayoutFactory.CreatePointyHexLayout<SKPoint, SkPointPolicy>(new SKPoint(MapData.HexSize, MapData.HexSize), new SKPoint(0, 0), Barbar.HexGrid.Offset.Even);
                break;
        }
    }

    private bool TilesFilter(object o)
    {
        if (o is not ITileLibraryItem tile)
            return false;
        return tile.HexType == MapData?.HexType;
    }

    public async Task OpenMap()
    {
        try
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            openPicker.FileTypeFilter.Add(".tutel");
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenStreamForReadAsync();

                var mapData = await MessagePackSerializer.DeserializeAsync<MapData>(stream);
                MapData = mapData;
#if !__WASM__
                MapData.FilePath = file.Path;
#endif
#if WINDOWS_UWP
                MapData.FaToken = StorageApplicationPermissions.FutureAccessList.Add(file);
#endif
                UndoStack.Clear();
                stream.Close();
            }
        }
        catch (Exception e)
        {
            var dialog = new MessageDialog(e.Message, "Failed to load");
            await dialog.ShowAsync();
        }
    }

    public async Task Save()
    {
        if (MapData == null)
            return;
        try
        {
            if (string.IsNullOrEmpty(MapData.FilePath) && string.IsNullOrEmpty(MapData.FaToken))
            {
                await SaveAs();
                return;
            }

            StorageFile? file = null;
            if (!string.IsNullOrEmpty(MapData.FaToken))
                file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(MapData.FaToken!);
            else if (!string.IsNullOrEmpty(MapData.FilePath))
                file = await StorageFile.GetFileFromPathAsync(MapData.FilePath!);

            if (file == null)
                throw new FileNotFoundException();

            using var stream = await file.OpenStreamForWriteAsync();
            await MessagePackSerializer.SerializeAsync(stream, MapData);
            UndoStack.HasUnsavedChanges = false;
        }
        catch (Exception e)
        {
            var dialog = new MessageDialog(e.Message, "Failed to save");
            await dialog.ShowAsync();
        }
    }

    public async Task SaveAs()
    {
        if (MapData == null)
            return;

        var savePicker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
        };
        // Dropdown of file types the user can save the file as
        savePicker.FileTypeChoices.Add("Tutel Map", new List<string> { ".tutel" });
        // Default file name if the user does not type one in or select a file to replace
        savePicker.SuggestedFileName = "New Tutel Map";
        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            using var stream = await file.OpenStreamForWriteAsync();
            await MessagePackSerializer.SerializeAsync(stream, MapData);
#if !__WASM__
            MapData.FilePath = file.Path;
#endif
#if WINDOWS_UWP
            MapData.FaToken = StorageApplicationPermissions.FutureAccessList.Add(file);
#endif
            UndoStack.HasUnsavedChanges = false;
        }
    }

    public async Task Undo()
    {
        await UndoStack.Undo();
        OnPropertyChanged();
    }

    public async Task Redo()
    {
        await UndoStack.Redo();
        OnPropertyChanged();
    }

    public void AddLayer()
    {
        // TODO use UndoStack
        MapData.AddLayer("New Layer");
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}