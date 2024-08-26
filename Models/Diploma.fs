namespace Models

module Diploma =

    type Diploma =
        | None
        | A
        | B
        | C

    type Error =
        | InvalidDiploma of string

    // Convert a diploma string to a Diploma type
    let create (diploma: string) : Result<Diploma, Error> =
        let normalizedDiploma = Helper.makeUpperCase diploma
        match normalizedDiploma with
        | "" -> Ok None
        | "A" -> Ok A
        | "B" -> Ok B
        | "C" -> Ok C
        | _ -> Error (InvalidDiploma "Diploma must be one of '', 'A', 'B', or 'C'")

    // Convert a Diploma type to its string representation
    let value (diploma: Diploma) : string =
        match diploma with
        | None -> ""
        | A -> "A"
        | B -> "B"
        | C -> "C"

    // Get the next level of diploma
    let nextDiploma (diploma: Diploma) : Diploma =
        match diploma with
        | None -> A
        | A -> B
        | B -> C
        | C -> None

    // Get the minimum minutes required for a diploma
    let minimumMinutesRequired (diploma: Diploma) : int =
        match diploma with
        | None -> 0
        | A -> 120
        | B -> 150
        | C -> 180
