module Routes

open Giraffe

let routes: HttpHandler =
    choose
        [ 
            CandidateHandler.handlers
            SessionHandler.handlers
            GuardianHandler.handlers
            DiplomaHandler.handlers
        ]
