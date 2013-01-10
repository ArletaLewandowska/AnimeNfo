module Mamut.AnimeNfo.Services.SitePraser

open System.Xml.Linq
open System.Linq
open System.Text.RegularExpressions

    module private Internal =

        let findDescendantBy (predicate : XElement -> bool) (doc : XDocument) =
            let rec loop (elements : XElement seq) =
                let find (element : XElement) =
                    match element with
                        | e when predicate e -> Some e
                        | e when e.HasElements -> loop (e.Elements())
                        | _ -> None

                match elements with
                    | es when Seq.isEmpty es -> None
                    | es -> 
                        match find (Seq.head es) with
                            | Some e -> Some e
                            | None -> loop (Seq.skip 1 es)

            loop (doc.Elements())

        let isAnimeYearsContainer (element : XElement) =
            (element.Name.LocalName = "div"
            && element.Attribute(XName.op_Implicit "id") <> null
            && element.Attribute(XName.op_Implicit "id").Value = "page_content")

        let isAnimeUrlContainer (element : XElement) =
            (element.Name.LocalName = "table"
            && element.Attribute(XName.op_Implicit "class") <> null
            && element.Attribute(XName.op_Implicit "class").Value = "anime_info")

        let findAnimeByYearContainer (doc : XDocument) =
            findDescendantBy isAnimeYearsContainer doc

        let findAnimeUrlsContainer (doc : XDocument) =
            findDescendantBy isAnimeUrlContainer doc

        let ns = XNamespace.Get "http://www.w3.org/1999/xhtml"
        let animeYearsUrls (element : XElement) =
            element
                .Element(ns + "strong")
                .Element(ns + "small")
                .Elements()
                |> Seq.map (fun e -> "http://www.animenfo.com/" + e.Attributes().Single().Value)

        let urlsFromContainer (urlsContainer : XElement) =
            urlsContainer.Elements()
            |> Seq.skip 1
            |> Seq.map
                (fun e ->
                    "http://www.animenfo.com/" + e
                        .Element(ns + "td")
                        .Element(ns + "a")
                        .Attributes()
                        .Single()
                        .Value)

        let isNextContainer (element : XElement) =
            let actionAttribute = element.Attribute(XName.op_Implicit "action")
            element.Name.LocalName = "form"
            && actionAttribute <> null
            && actionAttribute.Value = "/animebyyear.php"

        let regex = new Regex("(>[^<]*)&([^<]*<)")

        let normalizePage (page : string) =
            let simple = page.Replace("&nbsp;", " ").Replace("&copy;", " ")
            regex.Replace(simple, fun (m : Match)-> (m.Groups.[1].Value + "&amp;" + m.Groups.[2].Value))

open Internal

let yearUrlsFromPage (page : string) =
    let container =
        normalizePage page
        |> XDocument.Parse
        |> findAnimeByYearContainer 
    animeYearsUrls (container.Value)

let urlsFromPage (page : string) =
    let urlsContainer =
        normalizePage page
        |> XDocument.Parse
        |> findAnimeUrlsContainer
    urlsFromContainer urlsContainer.Value

let nextUrl (page : string) =
    let nextContaner =
        normalizePage page
        |> XDocument.Parse
        |> findDescendantBy isNextContainer

    let nextUrlPath = 
        nextContaner.Value
            .Element(ns + "div")
            .Element(ns + "table")
            .Element(ns + "tr")
            .Element(ns + "td")
            .Elements(ns + "a")
        |> Seq.tryFind (fun e -> e.Value = "Next")

    match nextUrlPath with
    | None -> None
    | Some uq -> Some ("http://www.animenfo.com/" + uq.Attributes().Single().Value)