using System.ComponentModel;
using System.Text.Json;
using AdminServices.Application.Categories.Commands.CreateCategory;
using AdminServices.Application.Categories.Queries.GetAllCategories;
using AdminServices.Application.Categories.Queries.GetCategoryById;
using AdminServices.Application.Common.Interfaces;
using AdminServices.Application.LocalStorage.Queries.GetAllDocuments;
using AdminServices.Application.LocalStorage.Queries.GetDocumentById;
using AdminServices.Application.LocalStorage.Queries.GetFolderContents;
using AdminServices.Application.LocalStorage.Queries.GetFolderTree;
using AdminServices.Application.Notes.Commands.CreateNote;
using AdminServices.Application.Notes.Commands.DeleteNote;
using AdminServices.Application.Notes.Commands.UpdateNote;
using AdminServices.Application.Notes.Queries.GetAllNotes;
using AdminServices.Application.Notes.Queries.GetNoteById;
using AdminServices.Application.Users.Queries.GetAllUsers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AdminServices.Infrastructure.Services;

/// <summary>
/// AI Chat service using Semantic Kernel with Google Gemini and direct database access via MediatR
/// </summary>
public class ChatService : IChatService
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatService> _logger;
    private readonly ChatHistoryStore _historyStore;

    public ChatService(IMediator mediator, IConfiguration configuration, ILogger<ChatService> logger, ChatHistoryStore historyStore)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _historyStore = historyStore;
    }

    public async Task<ChatResponse> ChatAsync(string message, string? sessionId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["Ai:GeminiApiKey"];
            var modelId = _configuration["Ai:ModelId"] ?? "gemini-1.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
                return new ChatResponse(false, string.Empty, "AI service is not configured. Please set Ai:GeminiApiKey in appsettings.");

            // Build Semantic Kernel using Gemini's OpenAI-compatible endpoint
            var kernelBuilder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey,
                    endpoint: new Uri("https://generativelanguage.googleapis.com/v1beta/openai/"));

            var kernel = kernelBuilder.Build();

            // Register database tools as SK plugin
            var plugin = KernelPluginFactory.CreateFromObject(new DatabasePlugin(_mediator), "AdminServices");
            kernel.Plugins.Add(plugin);

            // Use or create persistent session history
            var activeSessionId = sessionId ?? Guid.NewGuid().ToString();
            var history = _historyStore.GetOrCreate(activeSessionId, """
                Eres un asistente inteligente con acceso a la base de datos de AdminServices.
                Puedes consultar y gestionar notas, categorías, documentos y usuarios.
                Responde siempre en el mismo idioma que usa el usuario.
                Cuando el usuario pida datos, usa las herramientas disponibles para obtenerlos en tiempo real.
                Sé conciso, claro y útil.
                """);

            history.AddUserMessage(message);

            // Execute with automatic function calling (Gemini decides which tools to use)
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatService.GetChatMessageContentAsync(
                history,
                executionSettings,
                kernel,
                cancellationToken);

            // Persist assistant response in history for next turn
            history.AddAssistantMessage(response.Content ?? string.Empty);

            return new ChatResponse(
                Success: true,
                Message: response.Content ?? "No response from AI",
                SessionId: activeSessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI chat request");
            return new ChatResponse(false, string.Empty, $"Error processing request: {ex.Message}");
        }
    }

    // ─── Database Plugin ─────────────────────────────────────────────────────────

    private sealed class DatabasePlugin
    {
        private readonly IMediator _mediator;

        public DatabasePlugin(IMediator mediator) => _mediator = mediator;

        // Notes

        [KernelFunction, Description("Gets all notes from the database")]
        public async Task<string> GetAllNotes(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllNotesQuery(), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Gets a specific note by its unique identifier")]
        public async Task<string> GetNoteById(
            [Description("The GUID identifier of the note")] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetNoteByIdQuery(id), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Creates a new note in the database")]
        public async Task<string> CreateNote(
            [Description("Title of the note")] string title,
            [Description("HTML content of the note")] string content,
            [Description("Optional category GUID to assign the note to")] Guid? categoryId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new CreateNoteCommand
            {
                Title = title,
                Content = content,
                CategoryId = categoryId
            }, cancellationToken);

            return result.Success
                ? $"Note created successfully with ID: {result.Data}"
                : $"Error creating note: {result.Message}";
        }

        [KernelFunction, Description("Updates an existing note")]
        public async Task<string> UpdateNote(
            [Description("The GUID identifier of the note to update")] Guid id,
            [Description("New title for the note")] string title,
            [Description("New HTML content for the note")] string content,
            [Description("Optional category GUID")] Guid? categoryId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new UpdateNoteCommand
            {
                Id = id,
                Title = title,
                Content = content,
                CategoryId = categoryId
            }, cancellationToken);

            return result.Success
                ? "Note updated successfully"
                : $"Error updating note: {result.Message}";
        }

        [KernelFunction, Description("Deletes a note by its identifier")]
        public async Task<string> DeleteNote(
            [Description("The GUID identifier of the note to delete")] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteNoteCommand(id), cancellationToken);
            return result.Success ? "Note deleted successfully" : $"Error: {result.Message}";
        }

        // Categories

        [KernelFunction, Description("Gets all categories from the database")]
        public async Task<string> GetAllCategories(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllCategoriesQuery(), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Gets a specific category by its identifier")]
        public async Task<string> GetCategoryById(
            [Description("The GUID identifier of the category")] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Creates a new category")]
        public async Task<string> CreateCategory(
            [Description("Name of the category")] string name,
            [Description("Optional description for the category")] string? description = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new CreateCategoryCommand
            {
                Name = name,
                Description = description
            }, cancellationToken);

            return result.Success
                ? $"Category created with ID: {result.Data}"
                : $"Error: {result.Message}";
        }

        // Documents & Folders

        [KernelFunction, Description("Gets all documents stored in the database")]
        public async Task<string> GetAllDocuments(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllDocumentsQuery(), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Gets a specific document by its identifier")]
        public async Task<string> GetDocumentById(
            [Description("The GUID identifier of the document")] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetDocumentByIdQuery(id), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Gets the complete folder tree structure")]
        public async Task<string> GetFolderTree(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetFolderTreeQuery(), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        [KernelFunction, Description("Gets the contents of a specific folder (subfolders and documents)")]
        public async Task<string> GetFolderContents(
            [Description("The GUID of the folder, or null for root folder")] Guid? folderId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetFolderContentsQuery(folderId), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }

        // Users

        [KernelFunction, Description("Gets all users from the database (no passwords or sensitive data)")]
        public async Task<string> GetAllUsers(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
            return JsonSerializer.Serialize(result.Data);
        }
    }
}
