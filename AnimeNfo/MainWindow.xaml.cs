using Mamut.AnimeNfo.Contract;
using Microsoft.Practices.Prism.Regions;

namespace AnimeNfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow(IRegionManager regionManager)
        {
            InitializeComponent();

            Loaded += (sender, args) => regionManager.RequestNavigate("MainRegion", typeof (IAnimeListView).FullName);
        }
    }
}
