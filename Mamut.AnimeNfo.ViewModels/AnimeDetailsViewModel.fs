namespace Mamut.AnimeNfo.ViewModels

open Microsoft.Practices.Prism.Commands
open Microsoft.Practices.Prism.ViewModel
open Microsoft.Practices.Prism.Regions
open Mamut.AnimeNfo.Contract

type AnimeDetailsViewModel (regionManager : IRegionManager, animeListViewModel : IAnimeListViewModel) =
    inherit NotificationObject()
    
    let mutable anime : Anime option = None

    static member TitleParameter = "title"

    member this.Anime
        with get() = anime
        and set value =
            anime <- value
            base.RaisePropertyChanged("Anime")

    interface INavigationAware with
        member this.OnNavigatedTo navigationContext =
            let title = navigationContext.Parameters.["title"]
            this.Anime <-
                match animeListViewModel.Animes |> Seq.filter (fun a -> a.Title = title) |> List.ofSeq with
                | x::xs -> Some x
                | _ -> None

        member this.IsNavigationTarget navigationContext = true
        member this.OnNavigatedFrom navigationContext = ()
    
    member this.GoBackCommand with get() = new DelegateCommand(fun () -> regionManager.RequestNavigate("MainRegion", typeof<IAnimeListView>.FullName))
