namespace MyApp

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Sitelets

[<JavaScript>]
module Client =

    // Our SPA endpoints
    type EndPoint =
        | [<EndPoint "/">] Home
        | [<EndPoint "/charting">] Charting
        | [<EndPoint "/forms">] Forms

    // Create a router for our endpoints
    let router = Router.Infer<EndPoint>()
    // Install our client-side router and track the current page
    let currentPage = Router.InstallHash Home router

    // The SPA's `index.html` is its own template source
    type MainTemplate = Template<"index.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>

    // Custom widgets created from inner templates
    module Widgets =
        let Menu() =
            // We use hashed subpages, such as /#/charting. This way each page can be refreshed.
            [
                Home, "Dashboard", MainTemplate.TablesIcon().Doc(), "/#" + router.Link(Home)
                Charting, "Charting", MainTemplate.ChartsIcon().Doc(), "/#" + router.Link(Charting)
                Forms, "Forms", MainTemplate.FormsIcon().Doc(), "/#" + router.Link(Forms)
            ]
            |> List.map (fun (ep, title, icon, targetUrl) ->
                MainTemplate.MenuItem()
                    .ActiveIndicator(
                        currentPage.View.Map(fun cp ->
                            // If the current page is active, add an active menu item indicator
                            if ep = cp then
                                Templates.MainTemplate.MenuItemActiveIndicator().Doc()
                            else
                                Doc.Empty
                        ) |> Doc.EmbedView
                    )
                    .ExtraCssClasses(
                        currentPage.View.Map(fun cp ->
                            // If the current page is active, raise the font color
                            if ep = cp then
                                "text-gray-800 dark:text-gray-100"
                            else
                                ""
                        )
                    )
                    .Title(title)
                    .Icon(icon)
                    .TargetUrl(targetUrl)
                    .Doc()
            )

        let MobileMenu() =
            // We use hashed subpages, such as /#/charting. This way each page can be refreshed.
            [
                "Dashboard", MainTemplate.TablesIcon().Doc(), "/#" + router.Link(Home)
                "Charting", MainTemplate.ChartsIcon().Doc(), "/#" + router.Link(Charting)
                "Forms", MainTemplate.FormsIcon().Doc(), "/#" + router.Link(Forms)
            ]
            |> List.map (fun (title, icon, targetUrl) ->
                MainTemplate.MobileMenuItem()
                    .Title(title)
                    .Icon(icon)
                    .TargetUrl(targetUrl)
                    .Doc()
            )

    let HomePage() =
        let server = Remote<Shared.IApi>
        Templates.MainTemplate.Tables()
            .Title("Tables")
            .TableContainer(Grid.ClientTransactionsGrid(server))
            .Doc()
    
    let ChartingPage goto =
        MainTemplate.Charts()
            .ChartsContainer([
                MainTemplate.Chart()
                    .Title("Random chart")
                    .Chart(Charts.Chart01())
                    .Doc()
                MainTemplate.Chart()
                    .Title("Heartbeat")
                    .Chart(Charts.Chart02())
                    .Doc()
            ]).Doc()
    
    let FormsPage go =
        Templates.MainTemplate.Forms()
            .FormContainer([
                Forms.CreditRequest()
                Forms.ValidatedForm()
            ]).Doc()

    [<SPAEntryPoint>]
    let Main =
        let renderInnerPage (currentPage: Var<EndPoint>) =
            currentPage.View.Map (fun endpoint ->
                match endpoint with
                | Home      -> HomePage()
                | Charting  -> ChartingPage()
                | Forms     -> FormsPage()
            )
            |> Doc.EmbedView
        MainTemplate()
            .AppTitle("My WebSharper App")
            .Menu(Widgets.Menu())
            .MobileMenu(Widgets.MobileMenu())
            .PageContent(renderInnerPage currentPage)
            .Bind()
