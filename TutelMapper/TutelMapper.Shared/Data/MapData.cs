#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using MessagePack;
using TutelMapper.Annotations;
using TutelMapper.ViewModels;

namespace TutelMapper.Data;

[MessagePackObject]
public class MapData : INotifyPropertyChanged
{
    private int _selectedLayerIndex;

    [UsedImplicitly]
    public event PropertyChangedEventHandler? PropertyChanged;

    [IgnoreMember]
    public string? FilePath { get; set; }

    [IgnoreMember]
    public string? FaToken { get; set; }

    [Key(0)]
    public string DisplayName { get; set; } = "New Map";

    [Key(1)]
    public int Width { get; set; }

    [Key(2)]
    public int Height { get; set; }

    [Key(3)]
    public ObservableCollection<MapLayer> Layers { get; set; } = new();

    [Key(4)]
    public int SelectedLayerIndex
    {
        get => _selectedLayerIndex;
        set
        {
            if (value < 0 || value >= Layers.Count)
                return;
            _selectedLayerIndex = value;
        }
    }

    [Key(5)]
    public HexType HexType { get; set; } = HexType.Flat;

    [Key(6)]
    public int HexSize { get; set; } = 64;


    public void EnableEditing(object sender, RoutedEventArgs e)
    {
        if (!(e.OriginalSource is ListViewItemPresenter))
            return;
        var selectedLayer = Layers[SelectedLayerIndex];
        selectedLayer.EnableEditing();
    }
}

[MessagePackObject]
public class MapLayer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    [IgnoreMember]
    public bool IsEditingDisplayName { get; set; }

    [Key(0)]
    public string? DisplayName { get; set; }

    [Key(1)]
    public bool IsVisible { get; set; }

    [Key(2)]
    public string?[,] Data { get; set; } = new string?[0, 0];

    public void GotFocus(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    public void EnableEditing()
    {
        IsEditingDisplayName = true;
    }

    public void DisableEditing()
    {
        IsEditingDisplayName = false;
    }

    [NotifyPropertyChangedInvocator]
    [UsedImplicitly]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}