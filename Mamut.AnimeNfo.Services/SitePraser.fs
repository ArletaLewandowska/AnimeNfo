module Mamut.AnimeNfo.Services.SitePraser

open System
open System.Globalization
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

        let userRatingRegex = new Regex(@"(?<rating>\d\.\d).+?\((?<reviewCount>\d+)")
        let genresRegex = new Regex("<a href.+?>(?<genre>.+?)</a>")
        let linkUrlRegex = new Regex(@"href=""(?<url>.+?)""[^>]*>\s*(?<text>.+?)<", RegexOptions.Singleline)
        let getLinkUrl link = linkUrlRegex.Match(link).Groups.["url"].Value
        let getLinkText link = linkUrlRegex.Match(link).Groups.["text"].Value

        let mapAnimeDataToAnime (animeData : Map<string, string>) = {  
            Title = animeData.["Title"];
            JapaneseTitle = animeData.["Japanese Title"];
            OfficialSite = animeData.["Official Site"];
            Category =
                match animeData.["Category"] with
                | "OVA" -> Some OVA
                | "TV" -> Some TV
                | "Movie" -> Some Movie
                | "Internet" -> Some Internet
                | "Special" -> Some Special
                | "OAD" -> Some OAD
                | "Mobile Phone" -> Some MobilePhone
                | "-" -> None
                | t -> failwithf "Unknown type: '%s'" t;
            TotalEpisodes = animeData.["Total Episodes"];
            Genres =
                genresRegex.Matches animeData.["Genres"]
                |> Seq.cast<Match>
                |> Seq.map (fun m -> Genre.Parse m.Groups.["genre"].Value )
                |> List.ofSeq
            YearPublished = Int32.Parse(animeData.["Year Published"]);
            ReleaseDate =
                match DateTime.TryParseExact(animeData.["Release Date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None) with
                | (true, result) -> Some result
                | _ -> None;
            Broadcaster = animeData.["Broadcaster"];
            Studio = getLinkText animeData.["Studio"];
            USDistribution = getLinkText animeData.["US Distribution"];
            UserRating =
                match animeData.["User Rating"] with
                | "N/A" -> None
                | ur -> 
                    let m = userRatingRegex.Match animeData.["User Rating"]
                    Some {
                        Value = Decimal.Parse(m.Groups.["rating"].Value, CultureInfo.InvariantCulture);
                        ReviewCount = Int32.Parse(m.Groups.["reviewCount"].Value);
                    }
            Updated = Convert.ToDateTime animeData.["Updated"];
            }

        let findAnimeUrlsTable page =
            let animeUrlsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline)
            animeUrlsTableRegex.Match(page).Value

        let getUrlsDataFromTable table =
            linkUrlRegex.Matches(table)
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

