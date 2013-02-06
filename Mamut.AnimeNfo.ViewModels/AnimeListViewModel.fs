namespace Mamut.AnimeNfo.ViewModels

open Microsoft.Practices.Prism.ViewModel
open Microsoft.Practices.Prism.Commands
open Microsoft.Practices.Prism.Regions
open Microsoft.Practices.Prism
open System
open System.Xml.Linq
open System.Text
open Mamut.AnimeNfo.Services.SiteService
open System.Collections.ObjectModel
open Mamut.AnimeNfo.Contract

type AnimeListViewModel(regionManager : IRegionManager) = 
    inherit NotificationObject()

    let mutable animes = new ObservableCollection<Anime>()
    member this.Animes
        with get() = animes
        and set value =
            animes <- value
            base.RaisePropertyChanged("Animes")


    interface IAnimeListViewModel with
        member this.Animes
            with get() = this.Animes
            and set value = this.Animes <- value

    member private this.OnFetchAnime() = async {
        let! links =  yearUrls 
        let! urlGroups = Async.Parallel [for link in links -> animeByYearUrls link]
        let urls = urlGroups |> Seq.concat
        for link in urls do
            let! anime = anime link
            this.Animes.Add anime
        }

    member this.FetchAnimeCommand
        with get() =
            new DelegateCommand(fun () -> this.OnFetchAnime() |> Async.StartImmediate)

    member this.GoToDetailsCommand
        with get() =
            new DelegateCommand<string>(fun title ->
                let uriQuery = new UriQuery()
                uriQuery.Add(AnimeDetailsViewModel.TitleParameter, title)
                let uri = new Uri(typeof<IAnimeDetailsView>.FullName + uriQuery.ToString(), UriKind.Relative)
                regionManager.RequestNavigate("MainRegion", uri))