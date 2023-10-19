# A full-stack F# SPA template with charts and forms

<a href="doc/Tables.png">
    <img src="doc/Tables.png" width="300"/>
</a>
<a href="doc/Charting.png">
    <img src="doc/Charting.png" width="300"/>
</a>
<a href="doc/Forms.png">
    <img src="doc/Forms.png" width="300"/>
</a>

## Requirements

* .NET SDK 6.0 or above
* npm

## Features

### Server

* WebSharper sitelets for RPC

### Client

* [WebSharper](https://websharper.com) for F# to JavaScript translation
* WebSharper remoting
* [WebSharper.Charting](https://github.com/dotnet-websharper/charting) with a ChartJs-based renderer
* [WebSharper.Forms](https://github.com/dotnet-websharper/forms) for reactive forms
* [Femto](https://github.com/Zaid-Ajaj/Femto) for managing npm dependencies
* [Vite](https://vitejs.dev/) for development server and bundling
* HTML template: [Windmill](https://github.com/estevanmaito/windmill-dashboard) - by [@estevanmaito](https://github.com/estevanmaito)
   * [Tailwind CSS](https://tailwindcss.com/)
   * [Alpine JS](https://alpinejs.dev/)

## Running it

You can run the full app in watch mode using

```
dotnet run
```

This starts `vite` on port `5173` and the server on port `5001`, listens to file changes in the entire solution and recompiles changes on the fly (TEMP: this is not entirely true as of yet: if you make changes in the `shared` project, you need to stop and re-run `dotnet run` from the root).

To view the app, navigate to

```
https://localhost:5173
```

Alternatively, you can start the server and client separately:

```
// Terminal #1
> cd src/server
> dotnet run

// Terminal #2
> cd src/client
> dotnet ws build     // or "dotnet build"
> dotnet serve
```

## Release mode (not yet fully working)

To build the solution in release mode, you can use:

```
> cd src/client
> npx vite build
```

This will build the client bundle into the top-level `dist` folder. You will need to manually copy over the `assets` folder from `src/client/assets`.

TODO: Document how to configure the server.
