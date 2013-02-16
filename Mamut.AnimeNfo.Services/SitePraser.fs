﻿module Mamut.AnimeNfo.Services.SitePraser

open System.Xml.Linq
open System.Linq
open System.Text.RegularExpressions
open Mamut.AnimeNfo.Contract

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

        let ns = XNamespace.Get "http://www.w3.org/1999/xhtml"
        let animeYearsUrls (element : XElement) =
            element
                .Element(ns + "strong")
                .Element(ns + "small")
                .Elements()
                |> Seq.map (fun e -> "http://www.animenfo.com/" + e.Attributes().Single().Value)

        let regex = new Regex("(>[^<]*)&([^<]*<)")

        let normalizePage (page : string) =
            let simple = page.Replace("&nbsp;", " ").Replace("&copy;", " ")
            regex.Replace(simple, fun (m : Match)-> (m.Groups.[1].Value + "&amp;" + m.Groups.[2].Value))

        let findAnimeDetailsTable (page : string) =
            let animeDetailsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline)
            animeDetailsTableRegex.Match(page).Value

        let getAnimeDataFromTable animeDetailsTable =
            let animeDataRegex = new Regex("<td.*?<b>(?<key>.*?)</b>.*?<td.*?>((<a.*?>(?<value>.*?)</a>)|(?<value>.*?))</td>", RegexOptions.Singleline)
            animeDataRegex.Matches(animeDetailsTable)
                |> Seq.cast<Match>
                |> Seq.map (fun m -> m.Groups.["key"].Value, m.Groups.["value"].Value)
                |> Map.ofSeq

        let mapAnimeDataToAnime (animeData : Map<string, string>) = {  
            Title = animeData.["Title"];
            JapaneseTitle = animeData.["Japanese Title"];
            OfficialSite = animeData.["Official Site"];
            Category = animeData.["Category"];
            TotalEpisodes = animeData.["Total Episodes"];
            Genres = animeData.["Genres"];
            YearPublished = animeData.["Year Published"];
            ReleaseDate = animeData.["Release Date"];
            Broadcaster = animeData.["Broadcaster"];
            Studio = animeData.["Studio"];
            USDistribution = animeData.["US Distribution"];
            UserRating = animeData.["User Rating"];
            Updated = animeData.["Updated"];
            }

        let findAnimeUrlsTable page =
            let animeUrlsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline)
            animeUrlsTableRegex.Match(page).Value

        let getUrlsDataFromTable table =
            (new Regex(@"href=""(?<url>.+?)""")).Matches(table)
            |> Seq.cast<Match>
            |> Seq.map (fun m -> "http://www.animenfo.com/" + m.Groups.["url"].Value)
            
open Internal

let yearUrlsFromPage (page : string) =
    let container =
        normalizePage page
        |> XDocument.Parse
        |> findAnimeByYearContainer 
    animeYearsUrls (container.Value)

let urlsFromPage page =
    findAnimeUrlsTable page |> getUrlsDataFromTable

let nextUrl page =
    let nextContainer =
        (new Regex(@"<form[^>]*action='/animebyyear.php'[^>]*>.+?</form>", RegexOptions.Singleline)).Match(page).Value

    let nextUrlPath = 
        (new Regex(@"<a[^>]*?href='(?<value>[^']+)'[^>]*>Next</a>", RegexOptions.Singleline)).Match(nextContainer)
    
    if (nextUrlPath.Success)
        then Some("http://www.animenfo.com" + nextUrlPath.Groups.["value"].Value.Replace("&amp;", "&"))
    else None

let animeFromPage (page : string) =
    findAnimeDetailsTable page |> getAnimeDataFromTable |> mapAnimeDataToAnime

