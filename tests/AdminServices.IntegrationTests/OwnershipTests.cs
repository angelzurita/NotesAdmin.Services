using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AdminServices.IntegrationTests;

/// <summary>Each user only sees and changes their own notes, categories, folders and documents</summary>
public class OwnershipTests : IClassFixture<NotesApiFactory>
{
    private readonly NotesApiFactory _factory;

    public OwnershipTests(NotesApiFactory factory) => _factory = factory;

    private static async Task<Guid> CreateAsync(HttpClient api, string url, object body)
    {
        var res = await api.PostAsJsonAsync(url, body);
        res.StatusCode.Should().Be(HttpStatusCode.Created, await res.Content.ReadAsStringAsync());
        return (await res.ReadAsync<Api<Guid>>()).Data;
    }

    [Fact]
    public async Task Notes_and_categories_are_private_to_their_owner()
    {
        var ana = await _factory.SignInAsync("ana");
        var beto = await _factory.SignInAsync("beto");

        var category = await CreateAsync(ana, "/api/categories", new { name = "Trabajo", description = "de ana" });
        var note = await CreateAsync(ana, "/api/notes", new { title = "Secreta", content = "<p>solo ana</p>", categoryId = category });

        (await ana.GetFromJsonAsync<Api<List<NoteItem>>>("/api/notes"))!.Data!.Should().ContainSingle(n => n.Id == note);
        (await beto.GetFromJsonAsync<Api<List<NoteItem>>>("/api/notes"))!.Data!.Should().BeEmpty();
        (await beto.GetFromJsonAsync<Api<List<CategoryItem>>>("/api/categories"))!.Data!.Should().BeEmpty();

        (await beto.GetAsync($"/api/notes/{note}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.PutAsJsonAsync($"/api/notes/{note}", new { id = note, title = "hack", content = "x" })).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.DeleteAsync($"/api/notes/{note}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.GetAsync($"/api/categories/{category}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.DeleteAsync($"/api/categories/{category}")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Cannot hang a note on someone else's category
        (await beto.PostAsJsonAsync("/api/notes", new { title = "x", content = "y", categoryId = category })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Ana's data is intact
        var mine = (await ana.GetFromJsonAsync<Api<NoteItem>>($"/api/notes/{note}"))!.Data!;
        mine.Title.Should().Be("Secreta");
        mine.CategoryId.Should().Be(category);
    }

    [Fact]
    public async Task Owner_can_edit_and_delete_their_note()
    {
        var api = await _factory.SignInAsync("hugo");
        var note = await CreateAsync(api, "/api/notes", new { title = "Borrador", content = "<p>v1</p>" });

        (await api.PutAsJsonAsync($"/api/notes/{note}", new { id = note, title = "Final", content = "<p>v2</p>" })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetFromJsonAsync<Api<NoteItem>>($"/api/notes/{note}"))!.Data!.Title.Should().Be("Final");

        (await api.DeleteAsync($"/api/notes/{note}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetAsync($"/api/notes/{note}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invalid_input_is_rejected_with_400()
    {
        var api = await _factory.SignInAsync("ines");
        var res = await api.PostAsJsonAsync("/api/notes", new { title = "", content = "" });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("Note title is required");

        (await api.PostAsJsonAsync("/api/categories", new { name = "" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Folders_and_documents_are_private_to_their_owner()
    {
        var ana = await _factory.SignInAsync("ana2");
        var beto = await _factory.SignInAsync("beto2");

        var folder = await CreateAsync(ana, "/api/local-storage/folders", new { name = "Facturas" });

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent("hola mundo"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(file, "file", "saludo.txt");
        var upload = await ana.PostAsync($"/api/local-storage/upload?folderId={folder}", form);
        upload.StatusCode.Should().Be(HttpStatusCode.Created, await upload.Content.ReadAsStringAsync());
        var doc = (await upload.ReadAsync<Api<DocumentItem>>()).Data!.Id;

        var download = await ana.GetAsync($"/api/local-storage/{doc}/download");
        (await download.Content.ReadAsStringAsync()).Should().Be("hola mundo");

        var anaRoot = (await ana.GetFromJsonAsync<Api<FolderContents>>("/api/local-storage/folders/contents"))!.Data!;
        anaRoot.SubFolders.Should().ContainSingle(f => f.Id == folder);
        var betoRoot = (await beto.GetFromJsonAsync<Api<FolderContents>>("/api/local-storage/folders/contents"))!.Data!;
        betoRoot.SubFolders.Should().BeEmpty();
        betoRoot.Documents.Should().BeEmpty();

        (await beto.GetAsync($"/api/local-storage/{doc}/download")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.GetAsync($"/api/local-storage/folders/contents?folderId={folder}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.DeleteAsync($"/api/local-storage/folders/{folder}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.DeleteAsync($"/api/local-storage/{doc}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await beto.PostAsJsonAsync("/api/local-storage/folders", new { name = "intrusa", parentFolderId = folder })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await beto.GetFromJsonAsync<Api<List<DocumentItem>>>("/api/local-storage"))!.Data!.Should().BeEmpty();

        // Deleting the folder removes its documents
        (await ana.DeleteAsync($"/api/local-storage/folders/{folder}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ana.GetAsync($"/api/local-storage/{doc}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
