module MockData

open Database.InstoreDatabase
open Models
open System

let guardians = 
    [ ("456-WXYZ", "Lucas Lindstrom")
      ("789-QRST", "Clara Johansson")
      ("321-ABCD", "Pieter De Vries") ]
    |> Seq.choose (fun (id, name) ->
        match Guardian.build id name with
        | Ok guardian -> Some (GuardianId.value guardian.Id, (GuardianId.value guardian.Id, Name.value guardian.Name))
        | _ -> None)
    |> InMemoryDatabase.ofSeq
            
let candidates = 
    [ ("Sophie", DateTime(2017, 2, 15), "456-WXYZ", "B")
      ("Finn", DateTime(2016, 10, 20), "789-QRST", "A")
      ("Emma", DateTime(2019, 5, 25), "321-ABCD", "B+") ]
    |> Seq.choose (fun (n, bd, gi, dpl) ->
        match Candidate.build n bd gi dpl with
        | Ok candidate -> Some (Name.value candidate.Name, (Name.value candidate.Name, bd, GuardianId.value candidate.GuardianId, Diploma.value candidate.Diploma))
        | Error _ -> None)
    |> InMemoryDatabase.ofSeq

let sessions = 
    [ ("Sophie", "Shallow", DateTime(2024, 1, 10), 5)
      ("Sophie", "Shallow", DateTime(2024, 2, 15), 10)
      ("Sophie", "Deep", DateTime(2024, 3, 1), 25)
      ("Sophie", "Deep", DateTime(2024, 4, 5), 15)
      ("Finn", "Shallow", DateTime(2023, 5, 12), 20)
      ("Finn", "Deep", DateTime(2023, 6, 8), 15)
      ("Finn", "Deep", DateTime(2023, 7, 3), 25)
      ("Emma", "Shallow", DateTime(2024, 7, 20), 10)
      ("Emma", "Shallow", DateTime(2024, 8, 15), 20)
      ("Emma", "Deep", DateTime(2024, 9, 10), 30)
      ("Emma", "Deep", DateTime(2024, 10, 5), 15) ]
    |> Seq.choose (fun (n, pool, date, min) ->
        match Session.build pool date min with
        | Ok session -> Some ((n, date), (n, PoolType.value session.Pool, session.Date, Minutes.value session.Minutes))
        | Error _ -> None)
    |> InMemoryDatabase.ofSeq
