using Mamut.AnimeNfo.ViewModels;
using Mamut.AnimeNfo.Contract;

namespace AnimeNfo
{
    /// <summary>
    /// Interaction logic for AnimeListView.xaml
    /// </summary>
    public partial class AnimeListView : IAnimeListView
    {
        public AnimeListView(AnimeListViewModel animeListViewModel)
        {
            DataContext = animeListViewModel;
            InitializeComponent();
        }
    }
}
