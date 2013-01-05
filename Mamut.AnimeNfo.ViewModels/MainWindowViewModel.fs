namespace Mamut.AnimeNfo.ViewModels

open Microsoft.Practices.Prism.ViewModel
open Microsoft.Practices.Prism.Commands
open System.Xml.Linq
open System.Text
open Mamut.AnimeNfo.Services.SiteService

type MainWindowViewModel() = 
    inherit NotificationObject()

    let mutable text = ""
    member this.Text
        with get() = text
        and set(value) =
            text <- value
            base.RaisePropertyChanged("Text")

    member private this.onClick() =
        async{
            let! links =  yearLinks 
            let! urls = animeByYearUrls ("http://www.animenfo.com/" + Seq.head links)
            let sb = urls |> Seq.fold (fun (acc : StringBuilder) s -> acc.AppendLine(s)) (new StringBuilder())

            this.Text <- sb.ToString()}
                |> Async.RunSynchronously

    member x.ClickCommand with get() = new DelegateCommand(fun () ->  x.onClick())

    