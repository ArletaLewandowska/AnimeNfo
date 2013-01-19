namespace Mamut.AnimeNfo.ViewModels

open Microsoft.Practices.Prism.ViewModel
open Microsoft.Practices.Prism.Commands
open System.Xml.Linq
open System.Text
open Mamut.AnimeNfo.Services.SiteService

type MainWindowViewModel() = 
    inherit NotificationObject()

    let mutable animes = Seq.empty

    member this.Animes
        with get() = animes
        and set(value) =
            animes <- value
            base.RaisePropertyChanged("Animes")


    member private this.onClick() =
        async{
            let! links =  yearUrls 
            let! urlGroups = Async.Parallel [for link in links -> animeByYearUrls link]
            let urls = urlGroups |> Seq.concat

            this.Animes <- urls}

    member x.ClickCommand with get() = new DelegateCommand(fun () ->  x.onClick() |> Async.StartImmediate)

    