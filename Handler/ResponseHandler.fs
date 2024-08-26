module JsonParser

open Services
open Giraffe
open Thoth.Json.Net
open Thoth.Json.Giraffe
open Microsoft.AspNetCore.Http

let private jsonErrorResponse (statusCode: int) (errorJson: string) (ctx: HttpContext) =
    ctx.Response.StatusCode <- statusCode
    ctx.Response.Headers.["Content-Type"] <- "application/json"
    ctx.Response.WriteAsync(errorJson) |> ignore

let private handleServiceError (error: ServiceError) (next: HttpFunc) (ctx: HttpContext) =
    let errorJson = Encode.toString 0 (ServiceError.encode error)
    let statusCode =
        match error with
        | ServiceError.InvalidData _ -> 400
        | ServiceError.NotFound _ -> 404
        | ServiceError.UniquenessError _ -> 409
    jsonErrorResponse statusCode errorJson ctx
    next ctx


let respondWithJsonSeq encode data next ctx =
    match data with
    | Ok seqData -> ThothSerializer.RespondJsonSeq seqData encode next ctx
    | Error error -> handleServiceError error next ctx

let respondWithJsonSingle encode data next ctx =
    match data with
    | Ok singleData -> ThothSerializer.RespondJson singleData encode next ctx
    | Error error -> handleServiceError error next ctx
