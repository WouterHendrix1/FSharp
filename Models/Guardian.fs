namespace Models

open Thoth.Json.Net
open Models

// Define Guardian as a record type
type Guardian =
    { Id: GuardianId
      Name: Name
      Candidates: Candidate seq }

module Guardian =

    type Error =
        | InvalidId of GuardianId.Error
        | InvalidName of Name.Error
        | MissingField of string

    // Create a Guardian from id and name, initializing with no candidates
    let build (id: string) (name: string) : Result<Guardian, Error> =
        match GuardianId.create id, Name.create name with
        | Ok i, Ok n -> Ok { Id = i; Name = n; Candidates = Seq.empty }
        | Error e, _ -> Error (InvalidId e)
        | _, Error e -> Error (InvalidName e)

    // Add candidates to an existing Guardian
    let addCandidates (guardian: Guardian) (candidates: Candidate seq) : Guardian =
        { guardian with Candidates = candidates }

    // Encode a Guardian to JSON
    let encode (guardian: Guardian) : JsonValue =
        Encode.object [
            "id", Encode.string (GuardianId.value guardian.Id)
            "name", Encode.string (Name.value guardian.Name)
            "candidates", Encode.seq (Seq.map Candidate.encode guardian.Candidates)
        ]

    // Decode a Guardian from JSON
    let decode : Decoder<Result<Guardian, Error>> =
        Decode.object (fun get ->
            let id = get.Optional.Field "id" Decode.string
            let name = get.Optional.Field "name" Decode.string
            
            match id, name with
            | Some i, Some n -> build i n
            | None, _ -> Error (MissingField "id")
            | _, None -> Error (MissingField "name"))
