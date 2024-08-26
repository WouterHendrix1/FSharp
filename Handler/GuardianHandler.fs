module GuardianHandler

open JsonParser
open Models
open Services
open Giraffe

let addCandidatesToGuardian (candidateService: CandidateService) (guardian: Guardian) : Guardian =
    let guardianId = guardian.Id
    let candidates =
        candidateService.GetAllCandidatesByGuardian(guardianId) 
        |> Seq.toList
    Guardian.addCandidates guardian candidates

let getGuardians: HttpHandler =
    fun next ctx ->
        task {
            let guardianService = ctx.GetService<GuardianService>()
            let candidateService = ctx.GetService<CandidateService>()

            let result = Ok (
                guardianService.GetAllGuardians()
                |> Seq.map (addCandidatesToGuardian candidateService))

            return! respondWithJsonSeq Guardian.encode result next ctx
        }

let getGuardian (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let guardianService = ctx.GetService<GuardianService>()
            let candidateService = ctx.GetService<CandidateService>()

            let result = 
                match guardianService.GetGuardian(name) with
                | Ok guardian -> 
                    let guardianWithCandidates = addCandidatesToGuardian candidateService guardian
                    Ok guardianWithCandidates
                | Error error -> Error error

            return! respondWithJsonSingle Guardian.encode result next ctx
        }


let addGuardian: HttpHandler =
    fun next ctx ->
        task {
            let! body = ctx.ReadBodyFromRequestAsync()
            let guardianService = ctx.GetService<GuardianService>()
            let result =
                match guardianService.DecodeGuardian(body) with
                | Ok guardian -> guardianService.AddGuardian guardian
                | Error error -> Error error
            return! respondWithJsonSingle Guardian.encode result next ctx
        }

let handlers: HttpHandler = 
    choose [
          GET >=> route "/guardian" >=> getGuardians
          GET >=> routef "/guardian/%s" getGuardian
          POST >=> route "/guardian" >=> addGuardian
    ]