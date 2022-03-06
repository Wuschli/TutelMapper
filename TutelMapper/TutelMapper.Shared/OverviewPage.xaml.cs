using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TutelMapper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverviewPage : Page
    {
        public OverviewPage()
        {
            InitializeComponent();
        }

        public void NewMap(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditorPage), null, new DrillInNavigationTransitionInfo());
        }
    }
}