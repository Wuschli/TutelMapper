using Windows.UI.Xaml.Controls;
using TutelMapper.ViewModels;


namespace TutelMapper.Dialogs
{
    public sealed partial class NewMapDialog : ContentDialog
    {
        private readonly MainPageViewModel _mainPageViewModel;
        public int MapWidth { get; set; } = 40;
        public int MapHeight { get; set; } = 40;
        public int HexSize { get; set; } = 64;
        public HexType HexType { get; set; } = HexType.Flat;

        public NewMapDialog(MainPageViewModel mainPageViewModel)
        {
            _mainPageViewModel = mainPageViewModel;
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _mainPageViewModel.NewMap(MapWidth, MapHeight, HexType, HexSize);
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}