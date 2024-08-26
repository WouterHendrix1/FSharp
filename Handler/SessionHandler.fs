module SessionHandler

open Models
open Services
open Giraffe
open JsonParser
open Thoth.Json.Net

let addSession (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let! body = ctx.ReadBodyFromRequestAsync()
            let sessionService = ctx.GetService<SessionService>()
            let candidateService = ctx.GetService<CandidateService>()
            let result =
                match sessionService.DecodeSession(body) with
                | Ok session -> 
                    match candidateService.GetCandidate name with
                    | Ok _ -> sessionService.AddSession(name, session)
                    | Error error -> Error error
                | Error error -> Error error
            return! respondWithJsonSingle Session.encode result next ctx
        }

let getSessions (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionService = ctx.GetService<SessionService>()
            let result = sessionService.GetSessions(name)
            return! JsonParser.respondWithJsonSeq Session.encode result next ctx
        }


let getEligibleSessions (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionService = ctx.GetService<SessionService>()
            let result = sessionService.GetEligibleSessions(name, diploma)
            return! respondWithJsonSeq Session.encode result next ctx
        }

let getTotalEligibleMinutes (name: string, diploma: string) : HttpHandler =
    fun next ctx ->
        task {
            let sessionService = ctx.GetService<SessionService>()
            let result = sessionService.GetTotalEligibleMinutes(name, diploma)
            return! respondWithJsonSingle Encode.int result next ctx
        }

let getTotalMinutes (name: string) : HttpHandler = 
    getTotalEligibleMinutes(name, Diploma.value Diploma.None)

let handlers: HttpHandler = 
    choose [
          POST >=> routef "/candidate/%s/session" addSession
          GET >=> routef "/candidate/%s/session" getSessions
          GET >=> routef "/candidate/%s/session/total" getTotalMinutes
          GET >=> routef "/candidate/%s/session/%s" getEligibleSessions
          GET >=> routef "/candidate/%s/session/%s/total" getTotalEligibleMinutes
    ]