namespace Mamut.AnimeNfo.ViewModels

open Microsoft.Practices.Prism.ViewModel
open Microsoft.Practices.Prism.Commands
open System.Xml.Linq
open System.Text
open Mamut.AnimeNfo.Services.SiteService
open System.Collections.ObjectModel
open Mamut.AnimeNfo.Contract

type AnimeListViewModel() = 
    inherit NotificationObject()

    let mutable animes = new ObservableCollection<Anime>()

    member this.Animes
        with get() = animes
        and set(value) =
            animes <- value
            base.RaisePropertyChanged("Animes")

    member private this.onClick() = async {
        let! links =  yearUrls 
        let! urlGroups = Async.Parallel [for link in links -> animeByYearUrls link]
        let urls = urlGroups |> Seq.concat
        for link in urls do
            let! anime = anime link
            this.Animes.Add anime
        }

    member x.ClickCommand with get() = new DelegateCommand(fun () ->  x.onClick() |> Async.StartImmediate)

    