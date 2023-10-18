namespace Shared

open WebSharper

type IApi = {
    [<Remote>]
    GetValue : int -> Async<string>
}
