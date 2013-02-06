using Mamut.AnimeNfo.ViewModels;

namespace AnimeNfo
{
    /// <summary>
    /// Interaction logic for AnimeListView.xaml
    /// </summary>
    public partial class AnimeListView
    {
        public AnimeListView(AnimeListViewModel animeListViewModel)
        {
            DataContext = animeListViewModel;
            InitializeComponent();
        }
    }
}
