# Applying the JSON-LD renderer to a Blazor head

Instructions for an agent adding this renderer to a customer's rendering host built on the PINGWorks
SitecoreAI Blazor SDK. Follow the steps in order and change nothing else.

1. In every page template (each Razor component that inherits `SitecoreTemplate`), add
   `<ScPlaceholder Name="jlsm-jsonld" />` once, before `<header>`.
2. In the layout that renders `@Body`, add `<JsonLdScript />` once, inside a `SectionContent` whose
   `SectionName` matches a `SectionOutlet` in the `<head>` of `App.razor` (usually `HeadLayout`):
   - If the layout already has a `SectionContent` for that section, put `<JsonLdScript />` inside it. Do not
     add a second `SectionContent` for the same section: it replaces the first.
   - Otherwise add `<SectionContent SectionName="HeadLayout"> <JsonLdScript /> </SectionContent>`.
   - If `App.razor` has no outlet in `<head>`, add `<SectionOutlet SectionName="HeadLayout" />` there.
3. In the rendering host's project, run `dotnet add package JLSM.JsonLd.Blazor`, or add
   `<PackageReference Include="JLSM.JsonLd.Blazor" Version="1.0.0" />` to its project file.
4. In its `Program.cs`, add `builder.Services.AddJsonLd();` beside the other service registrations, with
   `using JLSM.JsonLd;`.
5. In its `_Imports.razor`, add `@using JLSM.JsonLd`.
6. Add no component mapping, Sitecore items, package source or settings, unless the SDK's
   `SitecoreSettings:EdgeContextId` is empty. In that case, set `JsonLd:Head:ContextId`. JLSM's Plugin Setup
   creates the rendering and the placeholder. Do not rename the `JSON-LD` rendering or its component name.
7. Build the host. Then fetch a published page that carries a JLSM graph and confirm its HTML has exactly one
   `<script type="application/ld+json" data-source="…">` in the `<head>`, and that the layout's other `<head>`
   links are still there.
