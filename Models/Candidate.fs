namespace Models

open Thoth.Json.Net
open System

// Define Candidate as a record type
type Candidate =
    { Name: Name
      DateOfBirth: DateTime
      GuardianId: GuardianId
      Diploma: Diploma.Diploma }

module Candidate =

    type Error =
        | InvalidName of Name.Error
        | InvalidGuardianId of GuardianId.Error
        | InvalidDiploma of Diploma.Error
        | MissingField of string
        | UpgradeNotAllowed of string

    // Build a Candidate from given parameters, validating inputs
    let build (name: string) (dateOfBirth: DateTime) (guardianId: string) (diploma: string) : Result<Candidate, Error> =
        match Name.create name, GuardianId.create guardianId, Diploma.create diploma with
        | Ok n, Ok g, Ok d -> Ok { Name = n; GuardianId = g; Diploma = d; DateOfBirth = dateOfBirth }
        | Error e, _, _ -> Error (InvalidName e)
        | _, Error e, _ -> Error (InvalidGuardianId e)
        | _, _, Error e -> Error (InvalidDiploma e)

    // Check if the candidate can upgrade to the desired diploma
    let canUpgradeToDiploma (desiredDiploma: Diploma.Diploma) (sessions: Session seq) : bool =
        let totalMinutes =
            sessions
            |> Seq.filter (Session.isApplicableForDiploma desiredDiploma)
            |> Seq.sumBy (fun session -> Minutes.value session.Minutes)
        totalMinutes >= Diploma.minimumMinutesRequired desiredDiploma

    // Upgrade the candidate's diploma if eligible
    let upgradeDiploma (candidate: Candidate) (desiredDiploma: Diploma.Diploma) (sessions: Session seq) : Result<Candidate, Error> =
        if canUpgradeToDiploma desiredDiploma sessions then
            Ok { candidate with Diploma = desiredDiploma }
        else
            Error (UpgradeNotAllowed "Candidate does not meet the requirements for the desired diploma")

    // Encode a Candidate to JSON
    let encode (candidate: Candidate) : JsonValue =
        Encode.object [
            "name", Encode.string (Name.value candidate.Name)
            "date_of_birth", Encode.datetime candidate.DateOfBirth
            "guardian_id", Encode.string (GuardianId.value candidate.GuardianId)
            "diploma", Encode.string (Diploma.value candidate.Diploma)
        ]

    // Decode a Candidate from JSON
    let decode : Decoder<Result<Candidate, Error>> =
        Decode.object (fun get ->
            let name = get.Optional.Field "name" Decode.string
            let dateOfBirth = get.Optional.Field "date_of_birth" Decode.datetime
            let guardianId = get.Optional.Field "guardian_id" Decode.string
            let diploma = get.Optional.Field "diploma" Decode.string
            
            match name, dateOfBirth, guardianId, diploma with
            | Some n, Some dob, Some g, Some d ->
                match build n dob g d with
                | Ok candidate -> Ok candidate
                | Error e -> Error e
            | None, _, _, _ -> Error (MissingField "name")
            | _, None, _, _ -> Error (MissingField "date_of_birth")
            | _, _, None, _ -> Error (MissingField "guardian_id")
            | _, _, _, None -> Error (MissingField "diploma"))
