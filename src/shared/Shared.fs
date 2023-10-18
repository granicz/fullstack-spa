namespace Shared

open WebSharper

(*
 * A record type to hold our RPCs.
 * For discovering these RPCs, they need to be marked
 * with the [<Rpc>] attribute.
 *)
type IApi = {
    [<Rpc>]
    GetValue : int -> Async<string>
}
