// Snippet from https://try.websharper.com/snippet/adam.granicz/00004H
module Charts

open WebSharper
open WebSharper.Charting
open WebSharper.UI

[<JavaScript>]
let Chart01 () =
    let labels =
        [| "Eating"; "Drinking"; "Sleeping";
           "Designing"; "Coding"; "Cycling"; "Running" |]
    let dataset1 = [|28.0; 48.0; 40.0; 19.0; 96.0; 27.0; 100.0|]
    let dataset2 = [|65.0; 59.0; 90.0; 81.0; 56.0; 55.0; 40.0|]

    let chart =
        Chart.Combine [
            Chart.Radar(Array.zip labels dataset1)
                .WithTitle("Day 1")
                .WithFillColor(Color.Rgba(151, 187, 205, 0.2))
                .WithStrokeColor(Color.Name "blue")
                .WithPointColor(Color.Name "darkblue")

            Chart.Radar(Array.zip labels dataset2)
                .WithTitle("Day 2")
                .WithFillColor(Color.Rgba(220, 220, 220, 0.2))
                .WithStrokeColor(Color.Name "green")
                .WithPointColor(Color.Name "darkgreen")
        ]
    Renderers.ChartJs.Render(chart, Size = Size(400, 300))
    
open System

[<JavaScript>]
let Chart02 () =
    let chart =
        [-5.*Math.PI .. 0.1 .. 5.*Math.PI]
        |> Seq.map (fun x -> (if Math.Abs(x-0.1)<0.1 then "0" else ""), Math.Sin(x)/2./x)
        |> Chart.Line
    Renderers.ChartJs.Render(chart, Size = Size(400, 300))
