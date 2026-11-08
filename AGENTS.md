# Applying the JSON-LD renderer to a Blazor head

Instructions for an agent adding this renderer to a customer's rendering host built on the PINGWorks
SitecoreAI Blazor SDK. Follow the steps in order and change nothing else.

1. In the rendering host's project, run `dotnet add package JLSM.JsonLd.Blazor`, or add
   `<PackageReference Include="JLSM.JsonLd.Blazor" Version="1.0.0" />` to its project file.
2. In its `Program.cs`, add `builder.Services.AddJsonLd();` beside the other service registrations, with
   `using JLSM.JsonLd;`.
3. In its `_Imports.razor`, add `@using JLSM.JsonLd`.
4. In the layout that renders `@Body`, add `<JsonLdScript />` once, after `@Body`.
5. Add no component mapping, package source or settings, unless the SDK's `SitecoreSettings:EdgeContextId`
   is empty. In that case, set `JsonLd:Head:ContextId`.
6. Build the host. Then fetch a published page and confirm its HTML has exactly one
   `<script type="application/ld+json" data-source="…">` in the body.
