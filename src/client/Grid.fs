namespace MyApp

open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Client

[<JavaScript>]
module Templates =
    type MainTemplate = Template<"index.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Grid =
    (*
     * A grid/table showing client transactions fetched from the server.
     *)
    let ClientTransactionsGrid(server: Shared.IApi) =
        Templates.MainTemplate.ClientTransactionsTableWithActions()
            .Title("Contracting fees")
            .Rows(
                async {
                    // Fetch transactions from server
                    let! txns = server.GetAllTransactions()
                    return
                        // Render each into a table row
                        txns
                        |> Array.map (fun txn ->
                            printfn "%A" txn
                            Templates.MainTemplate.ClientTransactionRow()
                                .AvatarUrl(txn.To.AvatarUrl)
                                .Name($"{txn.To.FirstName} {txn.To.LastName}")
                                .Position(txn.To.Position)
                                .Amount(string txn.Amount)
                                .Status(
                                    match txn.Status with
                                    | Shared.DTO.Status.Approved ->
                                        Templates.MainTemplate.ApprovedStatus().Doc()
                                    | Shared.DTO.Status.Pending ->
                                        Templates.MainTemplate.PendingStatus().Doc()
                                    | Shared.DTO.Status.Denied ->
                                        Templates.MainTemplate.DeniedStatus().Doc()
                                    | Shared.DTO.Status.Expired ->
                                        Templates.MainTemplate.ExpiredStatus().Doc()
                                )
                                .Date(txn.Date)
                                .Doc()
                        )
                        |> Doc.Concat
                }
                |> Doc.Async
            )
            .Doc()
