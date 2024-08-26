namespace Models

// Private type for GuardianId to restrict direct construction
type GuardianId = private GuardianId of string

module GuardianId =

    type Error =
        | InvalidGuardianId of string

    // Create a GuardianId from a string, validating its format
    let create (guardianId: string) : Result<GuardianId, Error> =
        if System.Text.RegularExpressions.Regex.IsMatch(guardianId, @"^\d{3}-[A-Z]{4}$") then
            Ok (GuardianId guardianId)
        else
            Error (InvalidGuardianId "GuardianId format is invalid")

    // Extract the string value from a GuardianId
    let value (GuardianId id) : string = id

