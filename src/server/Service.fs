module Service

open WebSharper
open WebSharper.Sitelets
open Shared
open Shared.DTO

(*
 * We use record-style RPCs to isolate client vs server-side changes.
 * Alternatively, you can mark your RPCs with the [<Rpc>] attribute
 * in the same client-server project.
 *)
let server : IApi = {
    GetAllTransactions = fun () ->
        let randomClient() =
            let rnd = System.Random()
            let oneOf lst = List.item (rnd.Next(0, List.length lst)) lst
            {
                FirstName = oneOf ["John"; "Paul"; "Steven"; "Joseph"; "Ariel"; "Eve"; "Margaret"]
                LastName = oneOf ["Smith"; "Reeds"; "Wolfram"; "Heureka"; "Burns"; "Weston"; "Price"]
                Position = oneOf ["Contractor"; "Developer"; "Actress/Actor"; "Influencer"; "Singer"; "Designer"]
                AvatarUrl = oneOf [
                    "https://images.unsplash.com/photo-1551006917-3b4c078c47c9?ixlib=rb-1.2.1&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;ixid=eyJhcHBfaWQiOjE3Nzg0fQ"
                    "https://images.unsplash.com/photo-1546456073-6712f79251bb?ixlib=rb-1.2.1&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;ixid=eyJhcHBfaWQiOjE3Nzg0fQ"
                    "https://images.unsplash.com/photo-1502720705749-871143f0e671?ixlib=rb-0.3.5&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;s=b8377ca9f985d80264279f277f3a67f5"
                    "https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?ixlib=rb-1.2.1&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;ixid=eyJhcHBfaWQiOjE3Nzg0fQ"
                    "https://images.unsplash.com/flagged/photo-1570612861542-284f4c12e75f?ixlib=rb-1.2.1&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;ixid=eyJhcHBfaWQiOjE3Nzg0fQ"
                    "https://images.unsplash.com/flagged/photo-1570612861542-284f4c12e75f?ixlib=rb-1.2.1&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;ixid=eyJhcHBfaWQiOjE3Nzg0fQ"
                    "https://images.unsplash.com/photo-1566411520896-01e7ca4726af?ixlib=rb-1.2.1&amp;q=80&amp;fm=jpg&amp;crop=entropy&amp;cs=tinysrgb&amp;w=200&amp;fit=max&amp;ixid=eyJhcHBfaWQiOjE3Nzg0fQ"
                ]
            }
        let randomStatus() =
            let rnd = System.Random()
            match rnd.Next(0, 3) with
            | 0 -> Approved
            | 1 -> Pending
            | 2 -> Denied
            | 3 | _ -> Expired
        let randomTransaction() =
            let rnd = System.Random()
            {
                To = randomClient()
                Amount = float(rnd.Next(0, 999))+float(rnd.Next(0, 99)) / 100.
                Status = randomStatus()
                Date = System.DateTime.Today.Subtract(System.TimeSpan(rnd.Next(0, 100), 0, 0, 0, 0))
            }
        async {
            return [| for i in 1 .. 9 -> randomTransaction() |]
        }
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
