module Mamut.AnimeNfo.Services.SiteService

open Mamut.AnimeNfo.Services.SitePraser
open System.Net
open System.IO
open System.Linq


let asyncGetHtml (url : string) = async{
    let yearsRequest = WebRequest.Create(url)
    let! rsp = yearsRequest.AsyncGetResponse()
                
    use stream = rsp.GetResponseStream()
    use reader = new StreamReader(stream)
 
    return! Async.AwaitTask(reader.ReadToEndAsync())
    }

let animeByYearPage = asyncGetHtml "http://www.animenfo.com/animebyyear.html"

let yearUrls = async { 
    let! page = animeByYearPage
    return yearUrlsFromPage page
    }

//todo: make it tail recursive
let rec animeByYearUrls yearUrl = async {
    let! page = asyncGetHtml yearUrl
    try
        let urls = urlsFromPage page
        let nextUrlQuery = nextUrl page

        match nextUrlQuery with
            | None  -> return urls
            | Some uq ->
                let! a = animeByYearUrls uq
                return Seq.append urls a
    with
        | ex ->
            printfn "%s" (ex.ToString())
            return Seq.empty
    
    }

let anime animeUrl = async {
    let! animePage = asyncGetHtml animeUrl
    return animeFromPage animePage
    }