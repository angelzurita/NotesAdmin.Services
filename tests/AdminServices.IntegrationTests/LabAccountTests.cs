using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AdminServices.IntegrationTests;

/// <summary>Sign-in with the lab account (Cuenta Lab), session closing and daily goals</summary>
public class LabAccountTests : IClassFixture<NotesApiFactory>
{
    private readonly NotesApiFactory _factory;

    public LabAccountTests(NotesApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_and_config_report_the_lab_account()
    {
        var client = _factory.CreateClient();
        (await client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);

        var config = await client.GetFromJsonAsync<Dictionary<string, object>>("/api/config");
        config!["sso"].ToString().Should().Be("True");
        config.Should().NotContainKey("ai");
        config["portal"].ToString().Should().Be("https://disciplina.zuritaserver.com/#labs");
    }

    [Fact]
    public async Task Local_register_and_login_are_gone_with_the_lab_account()
    {
        var client = _factory.CreateClient();
        (await client.PostAsJsonAsync("/api/auth/register", new { email = "x@x.com", password = "12345678", fullName = "X" }))
            .StatusCode.Should().Be(HttpStatusCode.Gone);
        (await client.PostAsJsonAsync("/api/auth/login", new { email = "x@x.com", password = "12345678" }))
            .StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Sso_start_redirects_to_the_lab_account()
    {
        var res = await _factory.CreateBrowser().GetAsync("/auth/sso");
        res.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.Headers.Location!.ToString().Should().StartWith("https://cuenta.example.test/autorizar?app=notes&estado=");
    }

    [Fact]
    public async Task Callback_with_a_forged_state_does_not_sign_in()
    {
        var browser = _factory.CreateBrowser();
        await browser.GetAsync("/auth/sso");
        var res = await browser.GetAsync("/auth/sso/callback?code=ana&estado=forged-state-value");
        res.StatusCode.Should().Be(HttpStatusCode.Redirect);
        res.Headers.Location!.ToString().Should().Be("/auth/sso");
    }

    [Fact]
    public async Task Callback_with_a_code_the_lab_account_rejects_fails()
    {
        var browser = _factory.CreateBrowser();
        var start = await browser.GetAsync("/auth/sso");
        var estado = System.Text.RegularExpressions.Regex.Match(start.Headers.Location!.ToString(), "estado=([^&]+)").Groups[1].Value;
        var res = await browser.GetAsync($"/auth/sso/callback?code=malo&estado={estado}");
        res.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    [Fact]
    public async Task Signed_in_user_is_linked_to_the_same_account_every_time()
    {
        var first = await _factory.SignInAsync("carla");
        var second = await _factory.SignInAsync("carla");
        var me1 = await first.GetFromJsonAsync<Dictionary<string, string>>("/api/auth/me");
        var me2 = await second.GetFromJsonAsync<Dictionary<string, string>>("/api/auth/me");
        me1!["id"].Should().Be(me2!["id"]);
        me1["name"].Should().Be("Usuario carla");
        me1["email"].Should().Be("carla@lab.zuritaserver.com");
    }

    [Fact]
    public async Task Requests_without_token_are_unauthorized()
    {
        (await _factory.CreateClient().GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_invalidates_the_token_immediately()
    {
        var api = await _factory.SignInAsync("dani");
        (await api.GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.PostAsync("/api/auth/logout", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await api.GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Lab_account_can_close_sessions_only_with_its_key_and_not_from_internet()
    {
        var api = await _factory.SignInAsync("eli");
        var client = _factory.CreateClient();

        (await client.PostAsJsonAsync("/auth/sso/cerrar", new { clave = "wrong", sub = "sub-eli" }))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

        var fromInternet = new HttpRequestMessage(HttpMethod.Post, "/auth/sso/cerrar")
        {
            Content = JsonContent.Create(new { clave = NotesApiFactory.LogoutKey, sub = "sub-eli" })
        };
        fromInternet.Headers.Add("CF-Connecting-IP", "203.0.113.9");
        (await client.SendAsync(fromInternet)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await api.GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Closing "every device but this one" keeps the session of that device
        var keep = await client.PostAsJsonAsync("/auth/sso/cerrar", new { clave = NotesApiFactory.LogoutKey, sub = "sub-eli", excepto = "sid-eli" });
        keep.StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.OK);

        var closed = await client.PostAsJsonAsync("/auth/sso/cerrar", new { clave = NotesApiFactory.LogoutKey, sub = "sub-eli", sid = "sid-eli" });
        closed.StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetAsync("/api/notes")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Daily_goal_counts_days_with_notes_written()
    {
        var api = await _factory.SignInAsync("fer");
        (await api.PostAsJsonAsync("/api/notes", new { title = "Hoy", content = "<p>algo</p>" })).StatusCode.Should().Be(HttpStatusCode.Created);

        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"));
        var day = DateOnly.FromDateTime(today);
        var res = await _factory.CreateClient().PostAsJsonAsync("/auth/sso/metas", new
        {
            clave = NotesApiFactory.LogoutKey, sub = "sub-fer",
            desde = day.AddDays(-13).ToString("yyyy-MM-dd"), hasta = day.ToString("yyyy-MM-dd")
        });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var metas = await res.ReadAsync<Metas>();
        metas.Dias.Should().Equal(day.ToString("yyyy-MM-dd"));
        metas.Meta.Should().Be(1);

        var nobody = await _factory.CreateClient().PostAsJsonAsync("/auth/sso/metas", new
        {
            clave = NotesApiFactory.LogoutKey, sub = "sub-nadie", desde = day.ToString("yyyy-MM-dd"), hasta = day.ToString("yyyy-MM-dd")
        });
        (await nobody.ReadAsync<Metas>()).Dias.Should().BeEmpty();
    }


    [Fact]
    public async Task Unknown_api_routes_are_json_404()
    {
        var res = await _factory.CreateClient().GetAsync("/api/no-existe");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Responses_carry_security_headers()
    {
        var res = await _factory.CreateClient().GetAsync("/health");
        res.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        res.Headers.GetValues("Content-Security-Policy").Single().Should().Contain("frame-ancestors 'none'");
    }
}
