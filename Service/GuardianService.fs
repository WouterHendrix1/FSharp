namespace Services

open Models
open Database.InstoreDatabase
open Thoth.Json.Net

// Service for managing guardian-related operations
type GuardianService(database: DatabaseInterface.IDatabase) =

    // Retrieve all guardians
    member this.GetAllGuardians() : Guardian seq =
        database.GetGuardians

    // Retrieve a specific guardian by name
    member this.GetGuardian(name: string) : Result<Guardian, ServiceError> =
        match database.GetGuardian name with
        | Some guardian -> Ok guardian
        | None -> Error (ServiceError.NotFound "Guardian not found")

    // Add a new guardian
    member this.AddGuardian(guardian: Guardian) : Result<Guardian, ServiceError> =
        match database.InsertGuardian guardian with
        | Ok () -> Ok guardian
        | Error (UniquenessError msg) -> Error (ServiceError.UniquenessError msg)

    // Decode a guardian from JSON
    member this.DecodeGuardian(json: string) : Result<Guardian, ServiceError> =
        match Decode.fromString Guardian.decode json with
        | Ok (Ok guardian) -> Ok guardian
         | Ok (Error err) -> 
            let errorMessage =
                match err with
                | Guardian.InvalidName _ -> "Invalid name"
                | Guardian.InvalidId _ -> "Invalid Guardian Id"
                | Guardian.MissingField msg -> "Missing field " + msg
            Error (ServiceError.InvalidData errorMessage)
        | Error err -> Error (ServiceError.InvalidData err)
