using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FloorPlanner.Tests.Api;

// These tests check that the whole application starts and serves the demo page (the other tests check separate classes).
//
// WebApplicationFactory<Program> runs the application in memory, without a real port or a browser, and the tests send
// HTTP requests to it. In the "Testing" environment the migration and demo data flags stay off, so no database is needed.
//
// They catch: UseDefaultFiles or UseStaticFiles removed from Program.cs, a file of wwwroot renamed or lost, and an
// application that no longer starts. They do NOT check how app.js works or how the page looks: that is checked in a browser.

/// <summary>Tests that the application serves the demo page and the static files.</summary>
public sealed class StaticPageTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    // The "Testing" environment: the flags of the startup behavior stay false, so no migration and no demo data are run
    // and the tests need no database.
    private readonly HttpClient _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();

    /// <summary>The address "/" returns the HTML page with the title of the demo.</summary>
    [Fact]
    public async Task Root_ReturnsHtmlWithTitle()
    {
        HttpResponseMessage response = await _client.GetAsync("/");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("FloorPlanner API demo", body);
    }

    /// <summary>The script and the styles of the page are served.</summary>
    [Theory]
    [InlineData("/app.js")]
    [InlineData("/style.css")]
    public async Task StaticFiles_AreServed(string path)
    {
        HttpResponseMessage response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>The health endpoint answers 200.</summary>
    [Fact]
    public async Task Health_Returns200()
    {
        HttpResponseMessage response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
