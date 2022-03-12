namespace TutelMapper.Data;

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