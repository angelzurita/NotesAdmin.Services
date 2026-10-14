using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AdminServices.Application.Common.Interfaces;
using AdminServices.Domain.Entities;
using AdminServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdminServices.WebApi;

// Despliegue en el laboratorio (zuritaserver): entrar con la Cuenta Lab, mismo flujo que las demás apps.
//  GET  /auth/sso           → estado aleatorio en cookie y redirección a Cuenta Lab /autorizar
//  GET  /auth/sso/callback  → canjea el código servidor a servidor, crea/enlaza el usuario y entrega el token
//                             a la web en el fragmento (#lab=…), que nunca viaja al servidor
//  GET  /auth/salir         → cierra la sesión central (Cuenta Lab avisa de vuelta para cerrar la de aquí)
//  POST /auth/sso/cerrar    → aviso de Cuenta Lab (servidor a servidor): cerrar sesiones
//  POST /auth/sso/metas     → días con notas creadas o editadas (para el hábito ligado en Disciplina)
//  GET  /api/config         → qué está activo (cuenta del lab) para que la web se adapte
public sealed record SsoConfig(string App, string Secret, string Interno, string Publico, string? LogoutHash)
{
    public static SsoConfig? From(IConfiguration cfg)
    {
        var secret = cfg["Sso:Secret"];
        if (string.IsNullOrWhiteSpace(secret)) return null;
        var interno = cfg["Sso:Interno"] ?? throw new InvalidOperationException("Sso:Interno is required with Sso:Secret");
        var publico = cfg["Sso:Publico"] ?? throw new InvalidOperationException("Sso:Publico is required with Sso:Secret");
        return new SsoConfig(cfg["Sso:App"] ?? "notes", secret, interno.TrimEnd('/'), publico.TrimEnd('/'), cfg["Sso:LogoutHash"]);
    }
}

public sealed record CuentaReq(string? Clave, string? Sub, string? Sid, string? Excepto, string? Desde, string? Hasta);

public static partial class LabModule
{
    const string EstadoCookie = "notes_sso";
    static readonly HashSet<string> SoloLocal = ["/api/auth/register", "/api/auth/login"];

    static bool Igual(string a, string b) => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
    static string Sha256(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();

    [GeneratedRegex("[^a-z0-9._-]")]
    private static partial Regex NoPermitido();

    // Con la cuenta del lab se apagan el registro/login locales.
    public static void UseLabGuards(this WebApplication app, SsoConfig? sso)
    {
        app.Use(async (ctx, next) =>
        {
            var path = (ctx.Request.Path.Value ?? "").TrimEnd('/');
            if (sso is not null && SoloLocal.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                ctx.Response.StatusCode = StatusCodes.Status410Gone;
                await ctx.Response.WriteAsJsonAsync(new { error = "Notes usa tu cuenta del laboratorio: entra desde Disciplina." });
                return;
            }
            await next();
        });
    }

    public static void MapLab(this WebApplication app, SsoConfig? sso)
    {
        // portal = a dónde regresa el botón "Disciplina" (el portal del laboratorio)
        app.MapGet("/api/config", (IConfiguration c) => Results.Ok(new { sso = sso is not null, cuenta = sso?.Publico, portal = c["Lab:PortalUrl"] }))
            .ExcludeFromDescription();
        if (sso is null) return;

        // Secure whenever the visitor came over https (the tunnel forwards X-Forwarded-Proto)
        static CookieOptions Opts(HttpContext ctx, TimeSpan maxAge) => new() { HttpOnly = true, Secure = ctx.Request.IsHttps, SameSite = SameSiteMode.Lax, Path = "/auth", MaxAge = maxAge };

        app.MapGet("/auth/sso", (HttpContext ctx) =>
        {
            var estado = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            ctx.Response.Cookies.Append(EstadoCookie, estado, Opts(ctx, TimeSpan.FromMinutes(10)));
            return Results.Redirect($"{sso.Publico}/autorizar?app={Uri.EscapeDataString(sso.App)}&estado={estado}");
        }).ExcludeFromDescription();

        app.MapGet("/auth/sso/callback", async (HttpContext ctx, string? code, string? estado, IHttpClientFactory http,
            ApplicationDbContext db, ISessionService sessions, ILoggerFactory lf, CancellationToken ct) =>
        {
            var guardado = ctx.Request.Cookies[EstadoCookie] ?? "";
            ctx.Response.Cookies.Delete(EstadoCookie, Opts(ctx, TimeSpan.Zero));
            if (guardado.Length < 10 || estado is null || !Igual(guardado, estado) || string.IsNullOrEmpty(code))
                return Results.Redirect("/auth/sso");

            Dictionary<string, string?> d;
            try
            {
                using var res = await http.CreateClient("cuenta").PostAsJsonAsync($"{sso.Interno}/api/token", new { app = sso.App, secret = sso.Secret, code }, ct);
                d = res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<Dictionary<string, string?>>(ct) ?? new() : new();
                if (!res.IsSuccessStatusCode) lf.CreateLogger("Lab").LogWarning("canje de Cuenta Lab fallido: {Status}", (int)res.StatusCode);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                lf.CreateLogger("Lab").LogWarning(ex, "Cuenta Lab no respondió al canje");
                d = new();
            }
            var sub = d.GetValueOrDefault("sub");
            if (string.IsNullOrEmpty(sub))
                return Results.Content("<!doctype html><meta charset=utf-8><title>Notes</title><h1>No se pudo validar tu inicio de sesión</h1><p><a href=\"/auth/sso\">Intentar de nuevo</a></p>", "text/html; charset=utf-8", statusCode: 502);

            var usuario = NoPermitido().Replace((d.GetValueOrDefault("usuario") ?? "usuario").ToLowerInvariant(), "");
            if (usuario.Length == 0) usuario = "usuario";
            var nombre = string.IsNullOrWhiteSpace(d.GetValueOrDefault("nombre")) ? usuario : d["nombre"]!.Trim();
            if (nombre.Length > 200) nombre = nombre[..200];

            var user = await db.Users.FirstOrDefaultAsync(u => u.SsoSub == sub, ct);
            if (user is null)
            {
                var email = $"{usuario}@lab.zuritaserver.com";
                if (await db.Users.AnyAsync(u => u.Email == email, ct)) email = $"{usuario}.{sub}@lab.zuritaserver.com";
                user = User.FromSso(sub, email, nombre);
                db.Users.Add(user);
            }
            else
            {
                if (!user.IsActive) return Results.Content("<!doctype html><meta charset=utf-8><title>Notes</title><h1>Tu cuenta de Notes está desactivada</h1>", "text/html; charset=utf-8", statusCode: 403);
                user.UpdateFullName(nombre);
            }
            await db.SaveChangesAsync(ct);

            var token = await sessions.SignInAsync(user, d.GetValueOrDefault("sid"), ct);
            return Results.Redirect($"/#lab={Uri.EscapeDataString(token.Token)}");
        }).ExcludeFromDescription();

        app.MapGet("/auth/salir", () => Results.Redirect($"{sso.Publico}/salir?app={Uri.EscapeDataString(sso.App)}"))
            .ExcludeFromDescription();

        // Llamadas de Cuenta Lab: solo por la red local (sin cabecera de Cloudflare) y con la clave de la app.
        bool DeCuenta(HttpContext ctx, CuentaReq req) =>
            !ctx.Request.Headers.ContainsKey("CF-Connecting-IP") && !string.IsNullOrEmpty(sso.LogoutHash)
            && !string.IsNullOrEmpty(req.Sub) && Igual(Sha256(req.Clave ?? ""), sso.LogoutHash);

        app.MapPost("/auth/sso/cerrar", async (HttpContext ctx, CuentaReq req, ISessionService sessions, CancellationToken ct) =>
        {
            if (!DeCuenta(ctx, req)) return Results.Json(new { error = "No autorizado." }, statusCode: 404);
            var cerradas = await sessions.RevokeSsoAsync(req.Sub!, req.Sid, req.Excepto, ct);
            return Results.Ok(new { ok = true, cerradas });
        }).ExcludeFromDescription();

        app.MapPost("/auth/sso/metas", async (HttpContext ctx, CuentaReq req, ApplicationDbContext db, CancellationToken ct) =>
        {
            if (!DeCuenta(ctx, req)) return Results.Json(new { error = "No autorizado." }, statusCode: 404);
            if (!DateOnly.TryParse(req.Desde, out var desde) || !DateOnly.TryParse(req.Hasta, out var hasta) || hasta < desde || hasta.DayNumber - desde.DayNumber > 62)
                return Results.BadRequest(new { error = "Fechas inválidas." });

            var uid = await db.Users.Where(u => u.SsoSub == req.Sub).Select(u => (Guid?)u.Id).FirstOrDefaultAsync(ct);
            if (uid is null) return Results.Ok(new { dias = Array.Empty<string>(), meta = 1 });

            var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
            var inicio = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(-1);
            var fin = hasta.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc).AddDays(1);
            var owner = uid.Value.ToString();
            // Esta llamada no trae usuario autenticado: se filtra por dueño a mano.
            var notas = await db.Notes.IgnoreQueryFilters().Where(n => n.OwnerId == owner && !n.IsDeleted)
                .Where(n => (n.CreatedAt >= inicio && n.CreatedAt <= fin) || (n.UpdatedAt >= inicio && n.UpdatedAt <= fin))
                .Select(n => new { n.CreatedAt, n.UpdatedAt }).ToListAsync(ct);

            DateOnly Local(DateTime f) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(f, DateTimeKind.Utc), tz));
            var dias = notas.SelectMany(n => n.UpdatedAt is { } u ? new[] { Local(n.CreatedAt), Local(u) } : new[] { Local(n.CreatedAt) })
                .Where(f => f >= desde && f <= hasta).Distinct().Order().Select(f => f.ToString("yyyy-MM-dd"));
            // "Meta" en Notes = escribir o editar al menos una nota ese día.
            return Results.Ok(new { dias, meta = 1 });
        }).ExcludeFromDescription();
    }
}
