module Service

open WebSharper
open WebSharper.Sitelets
open Shared

(*
 * We use record-style RPCs to isolate client vs server-side changes.
 * Alternatively, you can mark your RPCs with the [<Rpc>] attribute
 * in the same client-server project.
 *)
let server : IApi = {
    GetValue = fun i -> async { return "hellotext" }
}

// Register the server-side handler for the RPCs.
WebSharper.Core.Remoting.AddHandler typeof<IApi> server

(*
 * Define the rest of the endpoints our application provides.
 * Here, we implement a simple CORS service group and a home page.
 *
 * This section is optional.
 *)
type ServiceEndPoint =
    | [<EndPoint "GET /ping">] Ping

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/">] EndPointWithCors of Cors<ServiceEndPoint>
    
// Handling the service APIs
let HandleApi ctx endpoint =
    match endpoint with
    | Ping ->
        Content.Json "pong"

[<Website>]
let Main =
    Application.MultiPage (fun ctx endpoint ->
        match endpoint with
        | Home ->
            Content.Text "Service version 1.0"
        | EndPointWithCors endpoint ->
            Content.Cors endpoint 
                (fun corsAllows ->
                    { corsAllows with
                        Origins = ["http://example.com"]
                    }
                )
                (HandleApi ctx)
    )
