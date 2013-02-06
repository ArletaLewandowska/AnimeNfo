using Mamut.AnimeNfo.ViewModels;

namespace AnimeNfo
{
    /// <summary>
    /// Interaction logic for AnimeDetailsView.xaml
    /// </summary>
    public partial class AnimeDetailsView
    {
        public AnimeDetailsView(AnimeDetailsViewModel animeDetailsView)
        {
            DataContext = animeDetailsView;
            InitializeComponent();
        }
    }
}
