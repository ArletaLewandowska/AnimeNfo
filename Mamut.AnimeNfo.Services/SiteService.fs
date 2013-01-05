module Mamut.AnimeNfo.Services.SiteService

open System.Net
open System.IO
open System.Xml.Linq
open System.Linq

let animeByYearPage = 
    async{
        let yearsRequest = WebRequest.Create("http://www.animenfo.com/animebyyear.html")
        let! rsp = yearsRequest.AsyncGetResponse()
                
        use stream = rsp.GetResponseStream()
        use reader = new StreamReader(stream)
 
        return! Async.AwaitTask(reader.ReadToEndAsync())}

let isContentElement (element : XElement) =
    (element.Name.LocalName = "div"
    && element.Attribute(XName.op_Implicit "id") <> null
    && element.Attribute(XName.op_Implicit "id").Value = "page_content")

let findContentElement (doc : XDocument) =
    let rec loop (elements : XElement seq) =
        let find (element : XElement) =
            match element with
                | e when isContentElement e -> Some e
                | e when e.HasElements -> loop (e.Elements())
                | _ -> None

        match elements with
            | e when Seq.isEmpty e -> None
            | e -> 
                match find (Seq.head e) with
                    | Some e -> Some e
                    | None -> loop (Seq.skip 1 e)

    loop (doc.Elements())

let animePerYearLinks (element : XElement) =
    let ns = XNamespace.Get "http://www.w3.org/1999/xhtml"
    element
        .Element(ns + "strong")
        .Element(ns + "small")
        .Elements()
        |> Seq.map (fun e -> e.Attributes().Single().Value)

let yearLinks = async { 
    let! xml = animeByYearPage
    let doc = XDocument.Parse(xml.Replace("&nbsp;", " ").Replace("&copy;", " "))
    let contentElement = findContentElement doc
    return animePerYearLinks (contentElement.Value)
    }

