namespace MyApp

open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating

[<JavaScript>]
module Templates =
    type MainTemplate = Templating.Template<"index.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

[<JavaScript>]
module Forms =
    open WebSharper.Forms
    open WebSharper.JavaScript
    open WebSharper.UI.Client

    let CreditRequest() =
        Form.Return (fun name requestedLimit options message pp ->
            name, requestedLimit, options, message, pp)
        <*> (Form.Yield "" |> Validation.IsNotEmpty "Can't be empty")
        <*> Form.Yield ""
        <*> Form.Yield ""
        <*> Form.Yield ""
        <*> Form.Yield false
        |> Form.WithSubmit
        |> Form.Run (fun ((name, requestedLimit, options, message, pp) as data) ->
            JS.Alert $"You submitted: {data}"
        )
        |> Form.Render (fun name requestedLimit options message pp submitter ->
            Templates.MainTemplate.Form1()
                .Title("Elements")
                .Name(name)
                .RequestedLimit(requestedLimit)
                .Options(options)
                .Message(message)
                .PrivacyPolicy(pp)
                .OnSubmit(fun e -> submitter.Trigger())
                .Doc()
        )

    [<AutoOpen>]
    module Widgets =
        type ValidatedTextboxArgs =
            {
                Title: string
                Placeholder: string
            }

        let ValidatedTextbox (v: Var<_>) (submitter: Submitter<Result<_>>) args =
            let errorMessage, cssTextbox =
                submitter.View.Through v
                |> View.Map (function
                    | Result.Success _
                    | Result.Failure [] ->
                        Console.Log "success"
                        "", ""
                    | Result.Failure errs ->
                        let errors =
                            errs
                            |> List.map (fun e -> e.Text)
                            |> List.reduce (fun e1 e2 -> e1 + "," + e2)
                        errors, "border-red-600 focus:border-red-400 focus:shadow-outline-red"
                )
                |> fun view ->
                    view.Map fst,
                    view.Map snd
            Templates.MainTemplate.TextboxWithValidation()
                .Title(args.Title)
                .Placeholder(args.Placeholder)
                .Input(v)
                .ErrorMessagePlaceholder(
                    errorMessage.Doc(fun msg ->
                        if msg = "" then Doc.Empty else
                            Templates.MainTemplate.TextboxError()
                                .ErrorMessage(errorMessage)
                                .Doc()
                    )
                )
                .ExtraCssClassesForTextbox(cssTextbox)
                .Doc()

    let ValidatedForm() =
        Form.Return (fun name address -> name, address)
        <*> (Form.Yield "" |> Validation.Is
                (fun name -> name.Length > 1)
                "You must enter a valid name")
        <*> (Form.Yield "" |> Validation.IsNotEmpty
                "You must enter an address")
        |> Form.WithSubmit
        |> Form.Run (fun (name, address) -> ())
        |> Form.Render (fun name address submitter ->
            Templates.MainTemplate.FormSection()
                .Title("Validated form")
                .Content([
                    ValidatedTextbox name submitter
                        {
                            Title = "Name"
                            Placeholder = "Enter your name"
                        }
                    ValidatedTextbox address submitter
                        {
                            Title = "Address"
                            Placeholder = "Enter your address"
                        }
                    Templates.MainTemplate.SubmitButton()
                        .OnSubmit(fun e -> submitter.Trigger())
                        .Doc()
                ])
                .Doc()
        )