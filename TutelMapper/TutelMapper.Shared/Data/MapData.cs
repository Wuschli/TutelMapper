using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TutelMapper.Data;

public class MapData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public string FilePath { get; set; }
    public string DisplayName { get; set; } = "New Map";
    public int Width { get; set; }
    public int Height { get; set; }
    public ObservableCollection<MapLayer> Layers { get; set; } = new();
}

public static class MapDataExtensions
{
    public static void AddLayer(this MapData map, string layerName)
    {
        map.Layers.Add(new MapLayer
        {
            DisplayName = layerName,
            Data = new string[map.Width, map.Height]
        });
    }
}

public class MapLayer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public string DisplayName { get; set; }
    public string[,] Data { get; set; }
}