using System.IO;

#nullable enable
namespace TutelMapper.ViewModels;

public interface ITileLibraryItem : ITileSelectionItem
{
    string Id { get; }
    public HexType HexType { get; }
    public Stream GetPreviewImageStream();
    IDrawableTile GetDrawableTile();
    void WasPlaced();
    bool ContainsId(string? tileId);
}