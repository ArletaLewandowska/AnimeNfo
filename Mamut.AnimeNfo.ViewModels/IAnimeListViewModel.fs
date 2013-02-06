namespace Mamut.AnimeNfo.ViewModels

open Mamut.AnimeNfo.Contract
open System.Collections.ObjectModel

type IAnimeListViewModel =
    abstract Animes : ObservableCollection<Anime> with get, set

