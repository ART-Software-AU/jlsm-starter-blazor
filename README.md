# JSON-LD Schema Manager — Blazor add-on

Adds the JSON-LD Schema Manager renderer to a Blazor rendering host built on the PINGWorks SitecoreAI Blazor
SDK.

JSON-LD Schema Manager (JLSM) compiles each graph onto the datasource of a `JSON-LD` rendering and places that
rendering on pages through a partial design, in the `jlsm-jsonld` placeholder. Your page templates render
that placeholder. The `JSON-LD` component in the `JLSM.JsonLd.Blazor` package binds to the rendering by name
and fills its document in, and the `JsonLdScript` component writes every document on the page as one
`<script type="application/ld+json">` in the **head**, server-side, in the raw HTML.

The Blazor SDK differs from the Content SDK heads in `sitecore-xmcloud-starter-js`: the renderer is a NuGet
package rather than copied files, and components register themselves by attribute, so there is no component
map to edit.

## What is in this repository

`snippets/` holds the edits a host needs. `JLSM.JsonLd.Blazor` comes from nuget.org, with the
`JLSM.JsonLd.Core` engine bundled inside it.

## 1. Add the placeholder and the script

Render a placeholder with the key `jlsm-jsonld` in every page template (the Razor components that inherit
`SitecoreTemplate`), before the header (`snippets/PageTemplate.razor.snippet`). The key is set in JLSM
Settings on the dashboard; if you change it there, use the same key here.

```razor
<ScPlaceholder Name="jlsm-jsonld" />

<header>…</header>
<main>…</main>
<footer>…</footer>
```

In the Pages editor the `{ } JSON-LD` chip shows above the header, where authors see it.

Then place the script once in the layout that renders `@Body`, inside the section your `App.razor` renders
in `<head>` (`snippets/MainLayout.razor.snippet`):

```razor
<SectionContent SectionName="HeadLayout">
	@* … the layout's existing links and scripts … *@
	<JsonLdScript />
</SectionContent>

@Body
```

- **Use the layout's existing block.** If the layout already has a `SectionContent` for that section, add
  `<JsonLdScript />` inside it. Blazor shows only the last `SectionContent` rendered for a section, so a
  second block replaces the first, and the layout's links disappear from `<head>`.
- **Match an outlet.** The section name must match a `<SectionOutlet SectionName="…" />` in the `<head>` of
  `App.razor`. If there is none, add `<SectionOutlet SectionName="HeadLayout" />` there; without an outlet
  the script is not rendered.
- The `JSON-LD` renderings in the placeholder add their documents to the page, and `JsonLdScript` re-renders
  as they do, so it writes them all wherever it sits in the layout.

## 2. Add the package

1. Add the package to the rendering host (`snippets/YourHeadProject.csproj.snippet`):

   ```
   dotnet add package JLSM.JsonLd.Blazor
   ```

2. Register it in `Program.cs` (`snippets/Program.cs.snippet`):

   ```csharp
   using JLSM.JsonLd;

   builder.Services.AddJsonLd();
   ```

3. Add `@using JLSM.JsonLd` to `_Imports.razor` (`snippets/_Imports.razor.snippet`).

## 3. No component map

There is nothing to map: the package's `JsonLd` component carries `[SitecoreComponent( "JSON-LD" )]`, which
binds it to the rendering's component name.

## 4. The rendering in Sitecore

JLSM's Plugin Setup already creates and registers everything Sitecore needs. There is nothing to add by hand:

| Item | Path |
|---|---|
| The `JSON-LD` rendering (Json Rendering, component name `JSON-LD`) | `/sitecore/layout/Renderings/Feature/JSON-LD/JSON-LD` |
| The placeholder settings for `jlsm-jsonld`, allowing only the `JSON-LD` rendering | `/sitecore/layout/Placeholder Settings/Feature/JSON-LD/JSON-LD` |
| `jlsm-jsonld` in the Headless Layout's Layout Service Placeholders, so every page returns it | `/sitecore/layout/Layouts/Foundation/JSS Experience Accelerator/Presentation/JSS Layout` |

Publish the layout and the placeholder settings folder after Plugin Setup runs. On a tenant without the
Headless Layout, Plugin Setup asks you to add `jlsm-jsonld` to your own layout's placeholders.

> **Do not rename the rendering or change its Component Name.** The package binds to the name `JSON-LD`.
> Under any other name the rendering has no component and the page emits no JSON-LD.

## Page dates

The page's created and updated dates are the one thing the layout does not carry, so the host reads them
from Experience Edge, once per page, held five minutes. It uses the Sitecore SDK's
`SitecoreSettings:EdgeContextId`, or `JsonLd:Head:ContextId` when that is set
(`snippets/appsettings.json.snippet`). A read that fails or takes more than two seconds leaves the dates out
and the rest of the document is emitted.

We're working on a mechanism to avoid this round-trip in an upcoming version. Today, the extra round-trip
is only executed if your schema includes the `♣page:created` or `♣page:updated` tokens. We advise avoiding use
of the `♣page:created` token in particular for the time being.

## Editing mode

In the Pages editor the `JSON-LD` component shows an inline `{ } JSON-LD` chip and the page emits no script.

## Streaming rendering

The script is in the raw HTML because the host renders pages statically: Blazor sends the response once every
component, including the `JSON-LD` components and their date read, has finished. A page or layout marked
`[StreamRendering]` sends its first chunk before they finish, and the script arrives in a later chunk that
Blazor's JavaScript applies, so crawlers that read only the raw HTML miss it. Leave streaming rendering off on
pages that carry JSON-LD.

## Contract

This package reads contract **2**: the datasource carries `jsonTemplate`, `references`, `jlsmSiteRoot`,
`jlsmDefaultLanguage` and `jlsmContract`. A datasource on another contract is logged once at warning level
and emits nothing. Upgrade the app, republish every graph, then upgrade the package.

## Checking it

On a published page, view the source: there should be exactly one `application/ld+json` script, in the
`<head>`, with `data-source` naming JSON-LD Schema Manager.

## Licence

This repository and the `JLSM.JsonLd.*` packages are MIT; see `LICENSE`. `PINGWorks.SitecoreAI.BlazorSDK` is
proprietary and is not part of this repository.
