module Mamut.AnimeNfo.Services.SiteService

open System.Net
open System.IO
open System.Xml.Linq
open System.Linq

let asyncGetHtml (url : string) = async{
    let yearsRequest = WebRequest.Create(url)
    let! rsp = yearsRequest.AsyncGetResponse()
                
    use stream = rsp.GetResponseStream()
    use reader = new StreamReader(stream)
 
    return! Async.AwaitTask(reader.ReadToEndAsync())
    }

let animeByYearPage = asyncGetHtml "http://www.animenfo.com/animebyyear.html"

let isAnimeYearsContainer (element : XElement) =
    (element.Name.LocalName = "div"
    && element.Attribute(XName.op_Implicit "id") <> null
    && element.Attribute(XName.op_Implicit "id").Value = "page_content")

let findAnimeByYearContainer (doc : XDocument) =
    let rec loop (elements : XElement seq) =
        let find (element : XElement) =
            match element with
                | e when isAnimeYearsContainer e -> Some e
                | e when e.HasElements -> loop (e.Elements())
                | _ -> None

        match elements with
            | es when Seq.isEmpty es -> None
            | es -> 
                match find (Seq.head es) with
                    | Some e -> Some e
                    | None -> loop (Seq.skip 1 es)

    loop (doc.Elements())

let ns = XNamespace.Get "http://www.w3.org/1999/xhtml"
let animeYearsUrls (element : XElement) =
    element
        .Element(ns + "strong")
        .Element(ns + "small")
        .Elements()
        |> Seq.map (fun e -> e.Attributes().Single().Value)

let yearLinks = async { 
    let! xml = animeByYearPage
    let doc = XDocument.Parse(xml.Replace("&nbsp;", " ").Replace("&copy;", " "))
    let container = findAnimeByYearContainer doc
    return animeYearsUrls (container.Value)
    }

let isAnimeUrlContainer (element : XElement) =
    (element.Name.LocalName = "table"
    && element.Attribute(XName.op_Implicit "class") <> null
    && element.Attribute(XName.op_Implicit "class").Value = "anime_info")
    

let urlsFromPage (page : string) =
    let doc = XDocument.Parse(page.Replace("&nbsp;", " ").Replace("&copy;", " "))
    let rec loop (elements : XElement seq) = 
        let find (element : XElement) =
            match element with
                | e when isAnimeUrlContainer e -> Some e
                | e when e.HasElements -> loop (e.Elements())
                | _ -> None

        match elements with
            | es when Seq.isEmpty es -> None
            | es ->
                match find (Seq.head es) with
                    | Some e -> Some e
                    | None  -> loop (Seq.skip 1 es)
    let urlsContainer = loop (doc.Elements())
    urlsContainer.Value.Elements()
        |> Seq.skip 1
        |> Seq.map
            (fun e ->
                e
                    .Element(ns + "td")
                    .Element(ns + "a")
                    .Attributes()
                    .Single()
                    .Value)

let animeByYearUrls yearUrl = async {
    let! page = asyncGetHtml yearUrl 
    return urlsFromPage page
    }