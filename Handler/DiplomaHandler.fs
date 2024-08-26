module DiplomaHandler

open JsonParser
open Models
open Services
open Giraffe

let getUpgradableCandidates: HttpHandler =
    fun next ctx ->
        task {
            let candidateService = ctx.GetService<CandidateService>()
            let sessionService = ctx.GetService<SessionService>()
            let result = 
                candidateService.GetAllCandidates() 
                    |> Seq.filter (fun candidate -> 
                        match Diploma.nextDiploma candidate.Diploma with
                        | Diploma.A | Diploma.B | Diploma.C -> true
                        | Diploma.None -> false
                    )
                    |> Seq.filter (fun candidate ->
                        match sessionService.GetSessions (Name.value candidate.Name) with
                        | Ok sessions -> Candidate.canUpgradeToDiploma (Diploma.nextDiploma candidate.Diploma) sessions
                        | Error _ -> false
                    )
            return! respondWithJsonSeq Candidate.encode (Ok result) next ctx
        }

let handlers: HttpHandler = 
    choose [
          GET >=> route "/diploma/upgradable" >=> getUpgradableCandidates
    ]