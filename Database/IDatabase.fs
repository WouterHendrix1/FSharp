module DatabaseInterface

open Models
open Database.InstoreDatabase

type IDatabase = 

    abstract GetSessionsForUser : string -> Session seq
    
    abstract InsertSessionForUser : string -> Session -> Result<unit, InsertError>

    abstract GetCandidates : Candidate seq

    abstract GetCandidate : string -> Option<Candidate>

    abstract UpdateCandidate : Candidate -> Candidate

    abstract InsertCandidate : Candidate -> Result<unit, InsertError>

    abstract GetGuardians : Guardian seq

    abstract GetGuardian : string -> Option<Guardian>

    abstract InsertGuardian : Guardian -> Result<unit, InsertError>