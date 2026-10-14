using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace AdminServices.IntegrationTests;

/// <summary>
/// Runs the real API against a throw-away PostgreSQL database and a fake Cuenta Lab.
/// Needs NOTES_TEST_DB (default: the local test container on 127.0.0.1:55441).
/// </summary>
public class NotesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string LogoutKey = "test-logout-key";
    public static readonly string LogoutHash = Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(LogoutKey))).ToLowerInvariant();

    private readonly string _database = $"notes_test_{Guid.NewGuid():N}";
    private readonly string _server = Environment.GetEnvironmentVariable("NOTES_TEST_DB")
        ?? "Host=127.0.0.1;Port=55441;Username=notes_test;Password=notes_test";
    private WebApplication? _cuenta;
    private string _cuentaUrl = "";

    /// <summary>When false the app runs without the lab account (local register/login)</summary>
    protected virtual bool WithSso => true;

    public string ConnectionString => $"{_server};Database={_database}";

    public async Task InitializeAsync()
    {
        await using (var conn = new NpgsqlConnection($"{_server};Database=postgres"))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        // Fake Cuenta Lab: the code is the user ("ana", "beto"...); sid = "sid-<code>"
        var b = WebApplication.CreateSlimBuilder();
        b.WebHost.UseUrls("http://127.0.0.1:0");
        _cuenta = b.Build();
        _cuenta.MapPost("/api/token", (TokenReq r) =>
            r.App == "notes" && r.Secret == "test-sso-secret" && !string.IsNullOrEmpty(r.Code) && r.Code != "malo"
                ? Results.Ok(new Dictionary<string, string> { ["sub"] = $"sub-{r.Code}", ["usuario"] = r.Code, ["nombre"] = $"Usuario {r.Code}", ["sid"] = $"sid-{r.Code}" })
                : Results.BadRequest(new { error = "código inválido" }));
        await _cuenta.StartAsync();
        _cuentaUrl = _cuenta.Urls.First();

        Environment.SetEnvironmentVariable("NOTES_SECRETS", "/nonexistent/notes.json");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // UseSetting (not ConfigureAppConfiguration): Program reads these before Build()
        {
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = ConnectionString,
                ["Jwt:SecretKey"] = "integration-tests-signing-key-0123456789abcdef",
            };
            if (WithSso)
            {
                values["Sso:Secret"] = "test-sso-secret";
                values["Sso:Interno"] = _cuentaUrl;
                values["Sso:Publico"] = "https://cuenta.example.test";
                values["Sso:LogoutHash"] = LogoutHash;
            }
            foreach (var (key, value) in values)
                builder.UseSetting(key, value);
        }
    }

    public HttpClient CreateBrowser() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    /// <summary>Signs in through the full lab-account flow and returns an API client with the token</summary>
    public async Task<HttpClient> SignInAsync(string code)
    {
        var browser = CreateBrowser();
        var start = await browser.GetAsync("/auth/sso");
        Assert.Equal(HttpStatusCode.Redirect, start.StatusCode);
        var estado = Regex.Match(start.Headers.Location!.ToString(), "estado=([^&]+)").Groups[1].Value;

        var callback = await browser.GetAsync($"/auth/sso/callback?code={code}&estado={estado}");
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        var location = callback.Headers.Location!.ToString();
        Assert.StartsWith("/#lab=", location);

        var api = CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Uri.UnescapeDataString(location["/#lab=".Length..]));
        return api;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        if (_cuenta is not null) await _cuenta.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        await using var conn = new NpgsqlConnection($"{_server};Database=postgres");
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private sealed record TokenReq(string? App, string? Secret, string? Code);
}

/// <summary>Same API without the lab account: local register/login</summary>
public class LocalAuthApiFactory : NotesApiFactory
{
    protected override bool WithSso => false;
}

public static class HttpExtensions
{
    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<T>())!;
}
