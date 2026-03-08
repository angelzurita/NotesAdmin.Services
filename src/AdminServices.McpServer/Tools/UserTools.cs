using System.ComponentModel;
using System.Text.Json;
using AdminServices.Application.Users.Queries.GetAllUsers;
using MediatR;
using ModelContextProtocol.Server;

namespace AdminServices.McpServer.Tools;

/// <summary>
/// MCP Tools for Users — allows AI assistants to query user data (read-only, no sensitive fields).
/// </summary>
[McpServerToolType]
public class UserTools
{
    private readonly IMediator _mediator;

    public UserTools(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Get all users from the database. Returns id, email, fullName, role, isActive, createdAt. Password hashes and sensitive data are excluded.")]
    public async Task<string> GetAllUsers()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
}
