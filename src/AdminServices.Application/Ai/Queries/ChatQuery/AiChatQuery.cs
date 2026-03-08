using AdminServices.Application.Common.Interfaces;
using MediatR;

namespace AdminServices.Application.Ai.Queries.ChatQuery;

/// <summary>
/// Query to chat with the AI assistant that has access to application data
/// </summary>
public record AiChatQuery(
    string Message,
    string? SessionId = null
) : IRequest<ChatResponse>;
