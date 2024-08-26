module CandidateHandler

open JsonParser
open Models
open Services
open Giraffe

let getCandidates: HttpHandler =
    fun next ctx ->
        task {
            let candidateService = ctx.GetService<CandidateService>()
            let result = Ok(candidateService.GetAllCandidates())
            return! respondWithJsonSeq Candidate.encode result next ctx
        }

let getCandidate (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let candidateService = ctx.GetService<CandidateService>()
            let result = candidateService.GetCandidate(name)
            return! respondWithJsonSingle Candidate.encode result next ctx
        }

let addCandidate: HttpHandler =
    fun next ctx ->
        task {
            let! body = ctx.ReadBodyFromRequestAsync()
            let candidateService = ctx.GetService<CandidateService>()
            let guardianService = ctx.GetService<GuardianService>()
            let result =
                match candidateService.DecodeCandidate(body) with
                | Ok candidate -> 
                    match guardianService.GetGuardian (GuardianId.value candidate.GuardianId) with
                        | Ok _ -> candidateService.AddCandidate candidate
                        | Error error -> Error error 
                | Error error -> Error error
            return! respondWithJsonSingle Candidate.encode result next ctx
        }

let nextDiploma (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let candidateService = ctx.GetService<CandidateService>()
            let sessionService = ctx.GetService<SessionService>()
            let result = 
                match candidateService.GetCandidate(name) with
                | Ok candidate -> 
                    match sessionService.GetSessions name with
                    | Ok sessions -> 
                        match (Candidate.upgradeDiploma candidate (Diploma.nextDiploma candidate.Diploma) sessions) with
                        | Ok newCandidate -> candidateService.UpdateCandidate newCandidate
                        | Error _ -> Error (ServiceError.InvalidData "cannot upgrade")
                    | Error error -> Error error
                | Error error -> Error error
                
            return! respondWithJsonSingle Candidate.encode result next ctx
        }

let handlers: HttpHandler = 
    choose [
          POST >=> route "/candidate" >=> addCandidate
          GET >=> route "/candidate" >=> getCandidates
          PUT >=> routef "/candidate/%s/upgrade" nextDiploma
          GET >=> routef "/candidate/%s" getCandidate
    ]