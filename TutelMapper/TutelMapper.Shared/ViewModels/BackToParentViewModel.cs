using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using TutelMapper.Util;

namespace TutelMapper.ViewModels;

public class BackToParentViewModel : ITileSelectionItem
{
    public ITileSource Parent { get; }
    public string DisplayName => "Back";
    public ImageSource PreviewImage { get; } = new BitmapImage(new Uri("ms-appx:///Assets/Icons/back.png"));

    public BackToParentViewModel(ITileSource parent)
    {
        Parent = parent;
    }
}