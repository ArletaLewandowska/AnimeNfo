namespace Mamut.AnimeNfo.Contract

open System

type Genre =
    | Action
    | Adventure
    | Bishojo
    | Bishounen
    | Comedy
    | Cooking
    | DailyLife
    | Drama
    | Ecchi
    | Education
    | Family
    | Fantasy
    | Game
    | Harem
    | HistoricalSettings
    | Horror
    | Josei
    | Kids
    | LightNovel
    | Literature
    | Magic
    | MartialArts
    | Mature
    | Mecha
    | Music
    | Mystery
    | Novel
    | Parody
    | Romance
    | SchoolLife
    | ScienceFiction
    | ShounenAi
    | SliceOfLife
    | Sports
    | SuperPower
    | Supernatural
    | War
    | Yaoi
    | Yuri
    static member Parse (source : string) =
        match source with
        | "Action" -> Action
        | "Adventure" -> Adventure
        | "Bishojo" -> Bishojo
        | "Bishounen" -> Bishounen
        | "Comedy" -> Comedy
        | "Cooking" -> Cooking
        | "Daily Life" -> DailyLife
        | "Drama" -> Drama
        | "Ecchi" -> Ecchi
        | "Education" -> Education
        | "Family" -> Family
        | "Fantasy" -> Fantasy
        | "Game" -> Game
        | "Harem" -> Harem
        | "Historical Settings" -> HistoricalSettings
        | "Horror" -> Horror
        | "Josei" -> Josei
        | "Kids" -> Kids
        | "Light Novel" -> LightNovel
        | "Literature" -> Literature
        | "Magic" -> Magic
        | "Martial Arts" -> MartialArts
        | "Mature" -> Mature
        | "Mecha" -> Mecha
        | "Music" -> Music
        | "Mystery" -> Mystery
        | "Novel" -> Novel
        | "Parody" -> Parody
        | "Romance" -> Romance
        | "School Life" -> SchoolLife
        | "Science-Fiction" -> ScienceFiction
        | "Shounen Ai" -> ShounenAi
        | "Slice of Life" -> SliceOfLife
        | "Sports" -> Sports
        | "Super Power" -> SuperPower
        | "Supernatural" -> Supernatural
        | "War" -> War
        | "Yaoi" -> Yaoi
        | "Yuri" -> Yuri
        | _ -> failwithf "unknown genre: '%s'" source