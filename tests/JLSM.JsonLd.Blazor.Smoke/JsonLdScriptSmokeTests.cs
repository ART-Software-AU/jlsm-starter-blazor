using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Bunit;
using JLSM.JsonLd;
using Microsoft.Extensions.DependencyInjection;

namespace JLSM.JsonLd.Blazor.Smoke;

/// <summary>The package, used the way a head uses it, turns a contract 2 layout into one script in the body.</summary>
public class JsonLdScriptSmokeTests : BunitContext
{
	private static readonly string Club = char.ConvertFromUtf32( 0x2663 );

	private static readonly Regex Script = new( @"<script type=""application/ld\+json"" data-source=""[^""]*"">([\s\S]*?)</script>" );

	private readonly JsonLdPage Page = new();

	public JsonLdScriptSmokeTests()
		=> Services.AddSingleton( Page );

	private static JsonObject Rendering( string uid, JsonObject template )
		=> new() {
			["uid"] = uid,
			["componentName"] = JsonLdDatasource.ComponentName,
			["fields"] = new JsonObject {
				["jsonTemplate"] = new JsonObject { ["value"] = template.ToJsonString() },
				["references"] = new JsonArray(),
				["jlsmSiteRoot"] = new JsonObject { ["value"] = "https://www.siriuscanada.com" },
				["jlsmDefaultLanguage"] = new JsonObject { ["value"] = "en" },
				["jlsmContract"] = new JsonObject { ["value"] = "2" }
			}
		};

	private static JsonObject Layout( params JsonObject[] components )
		=> new() {
			["sitecore"] = new JsonObject {
				["context"] = new JsonObject { ["pageEditing"] = false, ["itemPath"] = "/sirius/home/research/q2-report" },
				["route"] = new JsonObject {
					["name"] = "q2-report",
					["itemLanguage"] = "en",
					["fields"] = new JsonObject { ["headline"] = new JsonObject { ["value"] = "Q2 report" } },
					["placeholders"] = new JsonObject {
						["headless-main"] = new JsonArray(
							new JsonObject { ["componentName"] = "Hero", ["uid"] = "hero" },
							new JsonObject {
								["componentName"] = "Container",
								["uid"] = "container",
								["placeholders"] = new JsonObject { ["container-1"] = new JsonArray( components ) }
							} )
					}
				}
			}
		};

	private void Contribute( JsonObject layout )
	{
		foreach ( var rendering in JsonLdRendering.FindAll( layout ) )
			Page.Add( rendering.Template?.Resolve( rendering.Block ), rendering );
	}

	[Fact]
	public void EveryJsonLdRenderingOnThePageIsEmittedAsOneGraph()
	{
		Contribute( Layout(
			Rendering( "a", new JsonObject { ["@type"] = "Article", ["@id"] = $"{Club}page:id/article", ["headline"] = $"{Club}field:headline" } ),
			Rendering( "b", new JsonObject { ["@type"] = "Organization", ["@id"] = "https://www.siriusgroup.com/#organization", ["name"] = "Sirius Group" } ) ) );

		var matches = Script.Matches( Render<JsonLdScript>().Markup );

		Assert.Single( matches );

		var graph = JsonNode.Parse( matches[0].Groups[1].Value )!["@graph"]!.AsArray();

		Assert.Equal( 2, graph.Count );
		Assert.Equal( "https://www.siriuscanada.com/q2-report/#article", graph[0]!["@id"]!.GetValue<string>() );
		Assert.Equal( "Q2 report", graph[0]!["headline"]!.GetValue<string>() );
	}

	[Fact]
	public void TheHeadReadDatesFillThePageDateTokens()
	{
		foreach ( var rendering in JsonLdRendering.FindAll( Layout( Rendering( "d", new JsonObject { ["@type"] = "WebPage", ["dateModified"] = $"{Club}page:updated" } ) ) ) )
			Page.Add( rendering.Template?.Resolve( rendering.Block.WithPageDates( new PageItemDates( null, "20261104T004826Z" ) ) ), rendering );

		Assert.Contains( "\"dateModified\":\"2026-11-04T00:48:26Z\"", Render<JsonLdScript>().Markup, StringComparison.Ordinal );
	}

	[Fact]
	public void APageWithNoJsonLdRenderingEmitsNothing()
	{
		Contribute( Layout( new JsonObject { ["componentName"] = "Hero", ["uid"] = "hero" } ) );

		Assert.Equal( string.Empty, Render<JsonLdScript>().Markup.Trim() );
	}
}
