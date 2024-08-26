namespace Models

open Thoth.Json.Net
open System

// Define PoolType as a discriminated union
type PoolType = 
    | Deep
    | Shallow

module PoolType = 
    
    type Error =
        | InvalidPoolType of string

    // Create a PoolType from a string, ensuring it's in uppercase
    let create (poolType: string) : Result<PoolType, Error> =
        match Helper.makeUpperCase poolType with
        | "DEEP" -> Ok Deep
        | "SHALLOW" -> Ok Shallow
        | _ -> Error (InvalidPoolType "Invalid pool type")

    // Convert PoolType to its string representation
    let value (poolType: PoolType) : string = 
        match poolType with
        | Deep -> "DEEP"
        | Shallow -> "SHALLOW"

// Define Minutes as a private type with bounds checking
type Minutes = private Minutes of int

module Minutes =

    type Error =
        | InvalidMinutes of string

    // Create Minutes from an integer with validation
    let create (minutes: int) : Result<Minutes, Error> =
        if minutes >= 0 && minutes <= 30 then
            Ok (Minutes minutes)
        else
            Error (InvalidMinutes "Minutes must be between 0 and 30")

    // Extract the integer value from Minutes
    let value (Minutes minutes) : int = minutes

// Define the Session record type
type Session =
    { Pool: PoolType
      Date: DateTime
      Minutes: Minutes }

module Session =

    type Error =
        | InvalidMinutes of Minutes.Error
        | InvalidPoolType of PoolType.Error
        | MissingField of string

    // Build a Session from parameters, handling potential errors
    let build (pool: string) (date: DateTime) (minutes: int) : Result<Session, Error> =
        match PoolType.create pool, Minutes.create minutes with
        | Ok poolType, Ok minutes -> Ok { Pool = poolType; Date = date; Minutes = minutes }
        | Error e, _ -> Error (InvalidPoolType e)
        | _, Error e -> Error (InvalidMinutes e)

    // Check if a session is applicable for a given diploma
    let isApplicableForDiploma (diploma: Diploma.Diploma) (session: Session) : bool =
        let minutesValue = Minutes.value session.Minutes
        match diploma with
        | Diploma.None -> true
        | Diploma.A -> (session.Pool = Shallow || session.Pool = Deep) && (minutesValue >= 1)
        | Diploma.B -> session.Pool = Deep && (minutesValue >= 10)
        | Diploma.C -> session.Pool = Deep && (minutesValue >= 15)

    // Encode a Session to JSON
    let encode (session: Session) : JsonValue =
        Encode.object [
            "pool", Encode.string (PoolType.value session.Pool)
            "date", Encode.datetime session.Date
            "minutes", Encode.int (Minutes.value session.Minutes)
        ]

    // Decode a Session from JSON
    let decode : Decoder<Result<Session, Error>> =
        Decode.object (fun get ->
            let poolType = get.Optional.Field "pool" Decode.string
            let date = get.Optional.Field "date" Decode.datetime
            let minutes = get.Optional.Field "minutes" Decode.int
            match poolType, date, minutes with
            | Some p, Some d, Some m -> build p d m
            | None, _, _ -> Error (MissingField "pool")
            | _, None, _ -> Error (MissingField "date")
            | _, _, None -> Error (MissingField "minutes"))
