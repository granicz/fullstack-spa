namespace Shared

open WebSharper

module DTO =
    type Client =
        {
            FirstName: string
            LastName: string
            Position: string
            AvatarUrl: string
        }

    type Status =
        | Approved
        | Pending
        | Denied
        | Expired

    type Transaction =
        {
            To: Client
            Amount: float
            Status: Status
            Date: string
        }

(*
 * A record type to hold our RPCs.
 * For discovering these RPCs, they need to be marked
 * with the [<Rpc>] attribute.
 *)
type IApi = {
    [<Rpc>]
    GetAllTransactions : unit -> Async<DTO.Transaction array>
}
