namespace BikeTracking.Domain.FSharp

open System

module ValueObjects =
    
    type DistanceUnit = 
        | Miles
        | Kilometers
        member this.AsString() =
            match this with
            | Miles -> "miles"
            | Kilometers -> "kilometers"
        static member FromString(s: string) =
            match s.ToLowerInvariant() with
            | "miles" -> Some Miles
            | "kilometers" -> Some Kilometers
            | _ -> None

    type DeletionStatus = 
        | Active
        | MarkedForDeletion
        member this.AsString() =
            match this with
            | Active -> "active"
            | MarkedForDeletion -> "marked_for_deletion"
        static member FromString(s: string) =
            match s.ToLowerInvariant() with
            | "active" -> Some Active
            | "marked_for_deletion" -> Some MarkedForDeletion
            | _ -> None

    type CommunityStatus = 
        | Private
        | Shareable  
        | Public
        member this.AsString() =
            match this with
            | Private -> "private"
            | Shareable -> "shareable"
            | Public -> "public"
        static member FromString(s: string) =
            match s.ToLowerInvariant() with
            | "private" -> Some Private
            | "shareable" -> Some Shareable
            | "public" -> Some Public
            | _ -> None

    type WindDirection =
        | North | NorthEast | East | SouthEast
        | South | SouthWest | West | NorthWest
        member this.AsString() =
            match this with
            | North -> "N"
            | NorthEast -> "NE"
            | East -> "E"
            | SouthEast -> "SE"
            | South -> "S"
            | SouthWest -> "SW"
            | West -> "W"
            | NorthWest -> "NW"
        static member FromString(s: string) =
            match s.ToUpperInvariant() with
            | "N" | "NORTH" -> Some North
            | "NE" | "NORTHEAST" -> Some NorthEast
            | "E" | "EAST" -> Some East
            | "SE" | "SOUTHEAST" -> Some SouthEast
            | "S" | "SOUTH" -> Some South
            | "SW" | "SOUTHWEST" -> Some SouthWest
            | "W" | "WEST" -> Some West
            | "NW" | "NORTHWEST" -> Some NorthWest
            | _ -> None

    type Weather = {
        Temperature: decimal option
        Conditions: string option
        WindSpeed: decimal option
        WindDirection: WindDirection option
        Humidity: decimal option
        Pressure: decimal option
        CapturedAt: DateTime
    }
    with 
        member this.IsUnavailable = 
            this.Temperature.IsNone && this.Conditions.IsNone &&
            this.WindSpeed.IsNone && this.WindDirection.IsNone &&
            this.Humidity.IsNone && this.Pressure.IsNone
        
        static member CreateUnavailable() = {
            Temperature = None
            Conditions = None
            WindSpeed = None
            WindDirection = None
            Humidity = None
            Pressure = None
            CapturedAt = DateTime.UtcNow
        }
