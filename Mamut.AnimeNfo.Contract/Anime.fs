namespace Mamut.AnimeNfo.Contract

open System

type Anime = {
    Title : string;
    JapaneseTitle : string;
    OfficialSite : string;
    Category : AnimeCategory option;
    TotalEpisodes : string;
    Genres : Genre list;
    YearPublished : int;
    ReleaseDate : DateTime option;
    Broadcaster : string;
    Studio : string;
    USDistribution : string;
    UserRating : Rating option;
    Updated :  DateTime;
    }