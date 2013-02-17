namespace Mamut.AnimeNfo.Contract

open System

type AnimeCategory =
    | OVA
    | TV
    | Movie
    | Internet
    | Special
    | OAD
    | MobilePhone
    override x.ToString() =
        match x with
            | OVA -> "OVA"
            | TV -> "TV"
            | Movie -> "Movie"
            | Internet -> "Internet"
            | Special -> "Special"
            | OAD -> "OAD"
            | MobilePhone -> "Mobile Phone"