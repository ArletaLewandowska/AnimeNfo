using System.Windows;

namespace AnimeNfo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            new AnimeNfoBootstrapper().Run();
        }
    }
}
