using Windows.UI.Xaml.Media;

namespace TutelMapper.ViewModels;

public interface ITileSelectionItem
{
    string DisplayName { get; }
    ImageSource PreviewImage { get; }
}