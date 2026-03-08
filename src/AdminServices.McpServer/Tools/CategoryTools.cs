using System.ComponentModel;
using System.Text.Json;
using AdminServices.Application.Categories.Commands.CreateCategory;
using AdminServices.Application.Categories.Commands.DeleteCategory;
using AdminServices.Application.Categories.Commands.UpdateCategory;
using AdminServices.Application.Categories.Queries.GetAllCategories;
using AdminServices.Application.Categories.Queries.GetCategoryById;
using MediatR;
using ModelContextProtocol.Server;

namespace AdminServices.McpServer.Tools;

/// <summary>
/// MCP Tools for Categories — allows AI assistants to query and manage categories directly from the DB.
/// </summary>
[McpServerToolType]
public class CategoryTools
{
    private readonly IMediator _mediator;

    public CategoryTools(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Get all categories stored in the database. Returns id, name, description, isActive, createdAt.")]
    public async Task<string> GetAllCategories()
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery());

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Get a single category by its GUID id.")]
    public async Task<string> GetCategoryById(
        [Description("The GUID of the category to retrieve.")] Guid id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Create a new category in the database. Returns the GUID of the created category.")]
    public async Task<string> CreateCategory(
        [Description("Name of the category.")] string name,
        [Description("Optional description for the category.")] string? description = null)
    {
        var command = new CreateCategoryCommand
        {
            Name = name,
            Description = description
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
            return $"Error: {result.Message}";

        return $"Category created successfully. Id: {result.Data}";
    }

    [McpServerTool]
    [Description("Update an existing category by its GUID id.")]
    public async Task<string> UpdateCategory(
        [Description("The GUID of the category to update.")] Guid id,
        [Description("New name for the category.")] string name,
        [Description("New description for the category.")] string? description = null)
    {
        var command = new UpdateCategoryCommand
        {
            Id = id,
            Name = name,
            Description = description
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
            return $"Error: {result.Message}";

        return "Category updated successfully.";
    }

    [McpServerTool]
    [Description("Delete a category permanently from the database by its GUID id.")]
    public async Task<string> DeleteCategory(
        [Description("The GUID of the category to delete.")] Guid id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id));

        if (!result.Success)
            return $"Error: {result.Message}";

        return "Category deleted successfully.";
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
}
