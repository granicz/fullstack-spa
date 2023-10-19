open Fake.Core
open Fake.IO
open Fake.DotNet
open Fake.Core.TargetOperators

let initializeContext () =
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
    Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

initializeContext ()

let runOrDefault args =
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | _ -> Target.runOrDefault "Run"
        0
    with e ->
        printfn $"%A{e}"
        1

let npm arg directory =
    let npmPath = ProcessUtils.tryFindFileOnPath "npm"
    match npmPath with
    | None -> failwith "Could not find npm"
    | Some npmPath ->
        CreateProcess.fromRawCommandLine npmPath arg
        |> CreateProcess.withWorkingDirectory directory
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore

let vite arg directory =
    let npxPath = ProcessUtils.tryFindFileOnPath "npx"
    match npxPath with
    | None -> failwith "Could not find npx"
    | Some npxPath ->
        CreateProcess.fromRawCommandLine npxPath ("vite " + arg)
        |> CreateProcess.withWorkingDirectory directory
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore

let viteRun directory =
    let npxPath = ProcessUtils.tryFindFileOnPath "npx"
    match npxPath with
    | None -> failwith "Could not find npx"
    | Some npxPath ->
        CreateProcess.fromRawCommandLine npxPath "vite"
        |> CreateProcess.withWorkingDirectory directory
        |> CreateProcess.ensureExitCode

let wsWatch directory =
    CreateProcess.fromRawCommandLine "dotnet" "ws watch"
    |> CreateProcess.withWorkingDirectory directory
    |> CreateProcess.ensureExitCode

let serverWatch directory =
    CreateProcess.fromRawCommandLine "dotnet" "watch"
    |> CreateProcess.withWorkingDirectory directory
    |> CreateProcess.ensureExitCode

module Proc =
    module Parallel =
        open System

        let locker = obj()

        let colors =
            [| ConsoleColor.Blue
               ConsoleColor.Yellow
               ConsoleColor.Magenta
               ConsoleColor.Cyan
               ConsoleColor.DarkBlue
               ConsoleColor.DarkYellow
               ConsoleColor.DarkMagenta
               ConsoleColor.DarkCyan |]

        let print color (colored: string) (line: string) =
            lock locker
                (fun () ->
                    let currentColor = Console.ForegroundColor
                    Console.ForegroundColor <- color
                    Console.Write colored
                    Console.ForegroundColor <- currentColor
                    Console.WriteLine line)

        let onStdout index name (line: string) =
            let color = colors.[index % colors.Length]
            if isNull line then
                print color $"{name}: --- END ---" ""
            else if String.isNotNullOrEmpty line then
                print color $"{name}: " line

        let onStderr name (line: string) =
            let color = ConsoleColor.Red
            if isNull line |> not then
                print color $"{name}: " line

        let redirect (index, (name, createProcess)) =
            createProcess
            |> CreateProcess.redirectOutputIfNotRedirected
            |> CreateProcess.withOutputEvents (onStdout index name) (onStderr name)

        let printStarting indexed =
            for (index, (name, c: CreateProcess<_>)) in indexed do
                let color = colors.[index % colors.Length]
                let wd =
                    c.WorkingDirectory
                    |> Option.defaultValue ""
                let exe = c.Command.Executable
                let args = c.Command.Arguments.ToStartInfo
                print color $"{name}: {wd}> {exe} {args}" ""

        let run cs =
            cs
            |> Seq.toArray
            |> Array.indexed
            |> fun x -> printStarting x; x
            |> Array.map redirect
            |> Array.Parallel.map Proc.run

let runParallel processes =
    processes
    |> Proc.Parallel.run
    |> ignore

let continueOrExitOnFail (pr: ProcessResult) =
    if pr.OK then
        ()
    else
        failwith (pr.Errors |> String.concat System.Environment.NewLine)

let (</>) path1 path2 = Path.combine path1 path2

let deployDir = "dist"
let serverDir = Path.getFullName "src/server"
let server = Path.getFullName (serverDir </> "Server.fsproj")
let clientDir = Path.getFullName "src/client"
let client = Path.getFullName (clientDir </> "Client.fsproj")
let sharedDir = Path.getFullName "src/shared"
let shared = Path.getFullName (sharedDir </> "Shared.fsproj")

let bld = DotNet.build (fun o ->  
    {o with
        NoLogo = true
        MSBuildParams = {o.MSBuildParams with DisableInternalBinLog = true}})

let testPath = ""
let libPath = None
let packPath = Path.getFullName "packages"

let doesCommandExist p = 
    ProcessUtils.tryFindFileOnPath p

let cleanProject (p: string) =
    DotNet.exec id "clean" $"{p}"

let projects =
    [
        "client", client
        "server", server
    ]

Target.create "Clean" (fun _ ->
    cleanProject server |> continueOrExitOnFail
    cleanProject client |> continueOrExitOnFail
    cleanProject shared |> continueOrExitOnFail
    Shell.cleanDir deployDir
)

Target.create "Prepare" (fun _ ->
    DotNet.exec id "tool" "restore" |> continueOrExitOnFail
    npm "install" clientDir
)

Target.create "Run" (fun _ ->
    [
        // "server", wsWatch serverDir
        "server", serverWatch serverDir
        "client", wsWatch clientDir
        "vite", viteRun clientDir
    ]
    |> runParallel
)

Target.create "PreBuild" (fun _ ->
    [|
        fun () -> bld serverDir
        fun () -> bld clientDir
    |]
    |> Array.Parallel.iter (fun act -> act())
)

Target.create "Bundle" (fun _ ->
    vite "bundle" clientDir
)

Target.create "BundleDebug" (fun _ ->
    vite "bundle" clientDir
    ()
)
Target.create "Client" (fun _ ->
    bld clientDir
    vite "bundle" clientDir
)
Target.create "Server" (fun _ ->
    bld serverDir
)

Target.create "Format" (fun _ ->
    DotNet.exec id "fantomas" $"{serverDir}" |> continueOrExitOnFail
    DotNet.exec id "fantomas" $"{clientDir}" |> continueOrExitOnFail
    DotNet.exec id "fantomas" $"{sharedDir}" |> continueOrExitOnFail
)
Target.create "Test" (fun _ ->
    if System.IO.Directory.Exists testPath then
        DotNet.exec id "run" testPath |> continueOrExitOnFail
    else ()
)
Target.create "Pack" (fun _ ->
    match libPath with
    | Some p -> DotNet.exec id "pack" $"{p} -c Release -o \"{packPath}\"" |> continueOrExitOnFail
    | None -> ()
)


"Prepare" ==> "PreBuild" ==> "Run" |> ignore
"Clean" ==> "Prepare" ==> "PreBuild" ==> "Bundle" |> ignore
"Clean" ==> "PreBuild" ==> "BundleDebug" |> ignore
"Prepare" ==> "Client" |> ignore
"Prepare" ==> "Server" |> ignore

[<EntryPoint>]
let main args =
    runOrDefault args
