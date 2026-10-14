using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AdminServices.IntegrationTests;

/// <summary>Without the lab account the API keeps its own register/login, hardened</summary>
public class LocalAuthTests : IClassFixture<LocalAuthApiFactory>
{
    private readonly LocalAuthApiFactory _factory;

    public LocalAuthTests(LocalAuthApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Self_registration_cannot_grant_admin()
    {
        var client = _factory.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/register", new { email = "mala@x.com", password = "password123", fullName = "Mala", role = "Admin" }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "mala@x.com", password = "password123" });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await login.ReadAsync<Api<Dictionary<string, object>>>();
        body.Data!["role"].ToString().Should().Be("User");

        var api = _factory.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Data["token"].ToString());
        (await api.GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetAsync("/api/users")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Wrong_password_and_invalid_register_are_rejected()
    {
        var client = _factory.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/register", new { email = "ok@x.com", password = "password123", fullName = "Ok" })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await client.PostAsJsonAsync("/api/auth/login", new { email = "ok@x.com", password = "otra-cosa" })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/api/auth/register", new { email = "no-es-correo", password = "corta", fullName = "" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetAsync("/auth/sso")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
