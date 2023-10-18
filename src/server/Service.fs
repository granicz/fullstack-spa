module Service

open WebSharper
open WebSharper.Sitelets
open Shared

let Services : IApi = {
    GetValue = fun i -> async { return "hellotext" }
}

WebSharper.Core.Remoting.AddHandler typeof<IApi> Services

type ServiceEndPoint =
    | [<EndPoint "GET /ping">] Ping

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/">] EndPointWithCors of Cors<ServiceEndPoint>
    
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
