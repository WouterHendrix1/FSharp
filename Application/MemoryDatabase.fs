namespace Application

open DatabaseInterface
open Database.InstoreDatabase
open Models

type MemoryDatabase() =
    let sessions = MockData.sessions
    let candidates = MockData.candidates
    let guardians = MockData.guardians

    interface IDatabase with
        member this.InsertGuardian(guardian: Guardian): Result<unit,InsertError> = 
            InMemoryDatabase.insert (GuardianId.value guardian.Id) (GuardianId.value guardian.Id, Name.value guardian.Name ) guardians

        member this.GetGuardian(name: string): Option<Guardian> = 
            match InMemoryDatabase.lookup name guardians with
            | None -> None
            | Some (id, name) ->
                match Guardian.build id name with
                | Ok guardian -> Some guardian
                | Error _ -> None

        member this.GetGuardians: Guardian seq = 
            InMemoryDatabase.all guardians
            |> Seq.choose (fun (id, name) ->
            match Guardian.build id name with
                | Ok guardian -> Some guardian
                | Error _ -> None)

        member this.InsertCandidate(candidate: Candidate): Result<unit,InsertError> = 
            InMemoryDatabase.insert (Name.value candidate.Name) (Name.value candidate.Name, candidate.DateOfBirth, GuardianId.value candidate.GuardianId, Diploma.value candidate.Diploma) candidates 

        member this.UpdateCandidate(candidate: Candidate): Candidate = 
            let candidateKey = Name.value candidate.Name
            let candidateData = (candidateKey, candidate.DateOfBirth, GuardianId.value candidate.GuardianId, Diploma.value candidate.Diploma)
            match InMemoryDatabase.update candidateKey candidateData candidates with
            | _ -> candidate

        member this.GetCandidate(name: string): Option<Candidate> = 
            match InMemoryDatabase.lookup name candidates with
            | None -> None
            | Some (name, bDay, gId, dpl) ->
                match Candidate.build name bDay gId dpl with
                | Ok candidate -> Some(candidate)
                | Error _ -> None

        member this.GetCandidates: Candidate seq = 
            InMemoryDatabase.all candidates
            |> Seq.choose (fun (name, bDay, gId, dpl) ->
            match Candidate.build name bDay gId dpl with
                | Ok candidate -> Some candidate
                | Error _ -> None)

        member this.InsertSessionForUser(name: string) (session: Session): Result<unit, InsertError> = 
            InMemoryDatabase.insert (name, session.Date) (name, PoolType.value session.Pool, session.Date, Minutes.value session.Minutes) sessions

        member this.GetSessionsForUser(name: string): Session seq = 
            InMemoryDatabase.filter (fun (n, pool, date, minutes) -> n = name) sessions
            |> Seq.choose (fun (_, pool, date, minutes) ->
            match Session.build pool date minutes with
            | Ok session -> Some session
            | Error _ -> None
        )
        
