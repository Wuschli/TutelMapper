namespace TutelMapper.Data;

public static class MapDataExtensions
{
    public static void AddLayer(this MapData map, string layerName)
    {
        var layer = new MapLayer
        {
            DisplayName = layerName,
            Data = new string[map.Width, map.Height],
            IsVisible = true,
        };
        map.Layers.Insert(map.SelectedLayerIndex, layer);
        layer.DisplayName = layerName;
    }

    public static void MoveLayerUp(this MapData map, MapLayer layer)
    {
        var index = map.Layers.IndexOf(layer);
        if (index <= 0)
            return;
        map.Layers.Move(index, index - 1);
        map.SelectedLayerIndex = index - 1;
    }

    public static void MoveLayerDown(this MapData map, MapLayer layer)
    {
        var index = map.Layers.IndexOf(layer);
        if (index < 0 || index >= map.Layers.Count - 1)
            return;
        map.Layers.Move(index, index + 1);
        map.SelectedLayerIndex = index + 1;
    }

    public static void DeleteLayer(this MapData map, MapLayer layer)
    {
        map.Layers.Remove(layer);
    }
}