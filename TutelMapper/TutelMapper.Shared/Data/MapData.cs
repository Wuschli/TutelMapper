using System.Collections.ObjectModel;
using System.ComponentModel;
using MessagePack;
using TutelMapper.Annotations;

namespace TutelMapper.Data;

[MessagePackObject]
public class MapData : INotifyPropertyChanged
{
    private int _selectedLayerIndex;

    [UsedImplicitly]
    public event PropertyChangedEventHandler PropertyChanged;

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
}

[MessagePackObject]
public class MapLayer : INotifyPropertyChanged
{
    [UsedImplicitly]
    public event PropertyChangedEventHandler PropertyChanged;

    [Key(0)]
    public string DisplayName { get; set; }

    [Key(1)]
    public bool IsVisible { get; set; }

    [Key(2)]
    public string[,] Data { get; set; }
}