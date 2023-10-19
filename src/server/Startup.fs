open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open WebSharper.AspNetCore
open Shared

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    
    // Add services to the container.
    builder.Services
        .AddWebSharper()
        .AddWebSharperRemoting<IApi>(Service.server)
    |> ignore

    let app = builder.Build()

    // Configure the HTTP request pipeline.
    if not (app.Environment.IsDevelopment()) then
        app.UseExceptionHandler("/Error")
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            .UseHsts()
        |> ignore

    WebSharper.Web.Remoting.DisableCsrfProtection ()
    WebSharper.Web.Remoting.AddAllowedOrigin "*"

    app.UseHttpsRedirection()
        .UseWebSharper(fun bld -> 
            bld
                .UseRemoting(true)
                .Sitelet(Service.Main)
            |> ignore)
    |> ignore
    
    app.Run()

    0 // Exit code
