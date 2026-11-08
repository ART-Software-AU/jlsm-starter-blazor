# JSON-LD Schema Manager — Blazor add-on

Adds the JSON-LD Schema Manager renderer to a Blazor rendering host built on the PINGWorks SitecoreAI Blazor
SDK.

JSON-LD Schema Manager compiles a schema.org document onto the datasource behind a `JSON-LD` rendering.
Sitecore puts that datasource on the page's layout. The `JSON-LD` component in this package binds to that
rendering by name, fills the document in, and the `JsonLdScript` component writes every document on the page
as one `<script type="application/ld+json">` in the **body**, server-side, in the raw HTML.

## What is here

| Path | Holds |
|---|---|
| `snippets/` | The edits a head needs |
| `tests/JLSM.JsonLd.Blazor.Smoke` | A bUnit test that uses only the package |

`JLSM.JsonLd.Blazor` comes from nuget.org, with the `JLSM.JsonLd.Core` engine bundled inside it.

## Install

1. Add the package to the rendering host (`snippets/YourHead.csproj.snippet`):

   ```
   dotnet add package JLSM.JsonLd.Blazor
   ```

2. Register it in `Program.cs` (`snippets/Program.cs.snippet`):

   ```csharp
   builder.Services.AddJsonLd();
   ```

3. Add `@using JLSM.JsonLd` to `_Imports.razor`.
4. Place the script once in the layout, after the body (`snippets/MainLayout.razor.snippet`):

   ```razor
   @Body

   <JsonLdScript />
   ```

There is nothing to map: `JsonLd` carries `[SitecoreComponent( "JSON-LD" )]`. There is no site configuration:
the site root and the default language arrive on the layout.

## Page dates

The page's created and updated dates are the one thing the layout cannot carry, so the head reads them from
Experience Edge, once per page, held five minutes. It uses the Sitecore SDK's `SitecoreSettings:EdgeContextId`,
or `JsonLd:Head:ContextId` when that is set (`snippets/appsettings.json.snippet`). A read that fails or takes
more than two seconds leaves the dates out and the rest of the document is emitted.

## Editing mode

In the Pages editor the `JSON-LD` component shows an inline `{ } JSON-LD` chip and the page emits no script.

## Contract

This package reads contract **2**: the datasource carries `jsonTemplate`, `references`, `jlsmSiteRoot`,
`jlsmDefaultLanguage` and `jlsmContract`. A datasource on another contract is logged once at warning level
and emits nothing. Upgrade the app, republish every graph, then upgrade the package.

## Checking it

```
dotnet build tests/JLSM.JsonLd.Blazor.Smoke
tests/JLSM.JsonLd.Blazor.Smoke/bin/Debug/net10.0/JLSM.JsonLd.Blazor.Smoke.exe
```

The test project is an xunit.v3 executable; running it directly works where `dotnet test` reports that zero
tests ran.

On a published page, view the source: there should be exactly one `application/ld+json` script, in the body,
with `data-source` naming JSON-LD Schema Manager.

## Licence

This repository and the `JLSM.JsonLd.*` packages are MIT; see `LICENSE`. `PINGWorks.SitecoreAI.BlazorSDK` is
proprietary and is not part of this repository.
