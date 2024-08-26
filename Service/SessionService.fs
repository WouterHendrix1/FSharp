namespace Services

open Models
open Database.InstoreDatabase
open Thoth.Json.Net

// Service for managing session-related operations
type SessionService(database: DatabaseInterface.IDatabase) =

    // Helper function to get sessions for a specific user
    let getSessionsForUser (name: string) : seq<Session> =
        database.GetSessionsForUser name

    // Add a new session for a user
    member this.AddSession(name: string, session: Session) : Result<Session, ServiceError> =
        match database.InsertSessionForUser name session with
        | Ok () -> Ok session
        | Error (UniquenessError msg) -> Error (ServiceError.UniquenessError msg)

    // Get all sessions for a user
    member this.GetSessions(name: string) : Result<seq<Session>, ServiceError> =
        Ok (getSessionsForUser name)

    // Get sessions eligible for a specific diploma
    member this.GetEligibleSessions(name: string, diploma: string) : Result<seq<Session>, ServiceError> =
        match Diploma.create diploma with
        | Ok diploma ->
            let sessions = getSessionsForUser name
            let eligibleSessions = sessions |> Seq.filter (fun session -> Session.isApplicableForDiploma diploma session)
            Ok eligibleSessions
        | Error _ -> Error (ServiceError.InvalidData "Invalid diploma")

    // Get the total minutes of eligible sessions for a specific diploma
    member this.GetTotalEligibleMinutes(name: string, diploma: string) : Result<int, ServiceError> =
        match this.GetEligibleSessions(name, diploma) with
        | Ok sessions ->
            let totalMinutes = 
                sessions
                |> Seq.map (fun session -> Minutes.value session.Minutes)
                |> Seq.sum
            Ok totalMinutes
        | Error error -> Error error

    // Decode a session from JSON
    member this.DecodeSession(json: string) : Result<Session, ServiceError> =
        match Decode.fromString Session.decode json with
        | Ok (Ok session) -> Ok session
        | Ok (Error err) ->
            let errorMessage =
                match err with
                | Session.InvalidMinutes _ -> "Invalid minutes"
                | Session.InvalidPoolType _ -> "Invalid pool type"
                | Session.MissingField msg -> "Missing field: " + msg
            Error (ServiceError.InvalidData errorMessage)
        | Error err -> Error (ServiceError.InvalidData (err.ToString()))
