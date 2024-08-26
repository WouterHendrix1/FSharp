namespace Models

open System

// Private type for Name to restrict direct construction
type Name = private Name of string

module Name =

    type Error =
        | InvalidName of string

    // Create a Name from a string, validating its format
    let create (name: string) : Result<Name, Error> =
        let isValidName (n: string) =
            not (String.IsNullOrWhiteSpace n) && n |> String.forall (fun c -> Char.IsLetter(c) || c = ' ')
        
        if isValidName name then
            Ok (Name name)
        else
            Error (InvalidName "Name contains invalid characters or is empty")

    // Extract the string value from a Name
    let value (Name name) : string = name

