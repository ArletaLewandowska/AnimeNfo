using System.Windows;
using Mamut.AnimeNfo.Contract;
using Mamut.AnimeNfo.ViewModels;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace AnimeNfo
{
    public class AnimeNfoBootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.GetInstance<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow = (Window) Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            Container.RegisterType<object, AnimeListView>(typeof (IAnimeListView).FullName);
            Container.RegisterType<IAnimeListViewModel, AnimeListViewModel>(new ContainerControlledLifetimeManager());

            Container.RegisterType<object, AnimeDetailsView>(typeof (IAnimeDetailsView).FullName);
            Container.RegisterType<AnimeDetailsViewModel>(new ContainerControlledLifetimeManager());
        }
    }
}
