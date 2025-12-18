namespace BikeTracking.Domain.FSharp

open System
open System.Threading
open System.Threading.Tasks
open BikeTracking.Domain.FSharp.ValueObjects

module Services =
    
    type IWeatherService =
        abstract GetHistoricalWeatherAsync: 
            latitude:decimal -> 
            longitude:decimal -> 
            rideDate:DateOnly -> 
            hour:int -> 
            cancellationToken:CancellationToken -> 
            Task<Weather option>
