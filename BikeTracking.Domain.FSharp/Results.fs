namespace BikeTracking.Domain.FSharp

open System

module Results =
    
    type ErrorSeverity = 
        | Warning
        | Error
        | Critical

    type Error = {
        Code: string
        Message: string
        Severity: ErrorSeverity
    }
    with 
        static member ValidationFailed msg = 
            { Code = "VALIDATION_FAILED"; Message = msg; Severity = Warning }
        static member NotFound msg = 
            { Code = "NOT_FOUND"; Message = msg; Severity = Warning }
        static member Conflict msg = 
            { Code = "CONFLICT"; Message = msg; Severity = Warning }
        static member Unexpected msg = 
            { Code = "UNEXPECTED"; Message = msg; Severity = Error }
        static member Critical msg = 
            { Code = "CRITICAL"; Message = msg; Severity = Critical }
        static member Unauthorized msg = 
            { Code = "UNAUTHORIZED"; Message = msg; Severity = Warning }
        static member Forbidden msg = 
            { Code = "FORBIDDEN"; Message = msg; Severity = Warning }

    type Result<'T> = 
        | Success of 'T
        | Failure of Error

    module Result =
        let map f = function
            | Success value -> Success (f value)
            | Failure error -> Failure error
        
        let bind f = function
            | Success value -> f value
            | Failure error -> Failure error
        
        let tap f = function
            | Success value as result -> f value; result
            | other -> other
        
        let tapFailure f = function
            | Failure error as result -> f error; result
            | other -> other
        
        let getValueOrDefault defaultValue = function
            | Success value -> value
            | Failure _ -> defaultValue
        
        let getErrorOrNull = function
            | Failure error -> Some error
            | Success _ -> None
        
        let requireNotNull paramName value =
            if isNull (box value) then
                Failure (Error.ValidationFailed $"{paramName} cannot be null.")
            else
                Success value
        
        let require condition error =
            if condition then Success ()
            else Failure error
        
        let combine (results: Result<unit> list) =
            results
            |> List.tryPick (function | Failure err -> Some (Failure err) | _ -> None)
            |> Option.defaultValue (Success ())
        
        let sequence (results: Result<'T> seq) =
            let folder state item =
                match state, item with
                | Failure err, _ -> Failure err
                | _, Failure err -> Failure err
                | Success values, Success value -> Success (value :: values)
            
            results
            |> Seq.fold folder (Success [])
            |> map List.rev

    // Computation expression for Result
    type ResultBuilder() =
        member _.Return(x) = Success x
        member _.ReturnFrom(x: Result<'T>) = x
        member _.Bind(x, f) = Result.bind f x
        member _.Zero() = Success ()
        member _.Delay(f) = f
        member _.Run(f) = f()
        
        member _.Combine(r1, r2) =
            match r1 with
            | Success () -> r2()
            | Failure err -> Failure err
        
        member _.TryWith(body, handler) =
            try body()
            with ex -> handler ex
        
        member _.TryFinally(body, compensation) =
            try body()
            finally compensation()

    let result = ResultBuilder()
