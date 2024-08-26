namespace Services

open Thoth.Json.Net

// Define ServiceError with different error types
type ServiceError =
    | NotFound of string
    | InvalidData of string
    | UniquenessError of string

module ServiceError =
    // Encode a ServiceError to JSON
    let encode (error: ServiceError) : JsonValue =
        let createErrorObject errorType message =
            Encode.object [
                "type", Encode.string errorType
                "message", Encode.string message
            ]
            
        match error with
        | NotFound msg -> createErrorObject "NotFound" msg
        | InvalidData msg -> createErrorObject "InvalidData" msg
        | UniquenessError msg -> createErrorObject "UniquenessError" msg
