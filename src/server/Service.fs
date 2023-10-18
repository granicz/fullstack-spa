module Service

open WebSharper
open WebSharper.Sitelets
open Shared

[<Remote>]
let Services : IApi = {
    GetValue = fun i -> async { return "hellotext" }
}

WebSharper.Core.Remoting.AddHandler typeof<IApi> Services

type EndPointWithCors =
    | [<EndPoint "GET /ping">] Ping

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/">] EndPointWithCors of Cors<EndPointWithCors>
    
let HandleApi ctx endpoint =
    match endpoint with
    | Ping ->
        Content.Json "pong"

[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        
        match endpoint with
        | Home -> Content.Text "Service version 1.0"
        | EndPointWithCors endpoint ->
            Content.Cors endpoint 
                (fun corsAllows ->
                    { corsAllows with
                        Origins = ["http://example.com"]
                    }
                )
                (HandleApi ctx)
    )

