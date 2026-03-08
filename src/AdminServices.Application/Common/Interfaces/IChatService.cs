namespace AdminServices.Application.Common.Interfaces;

/// <summary>
/// Service for natural language chat using AI with access to application data
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Sends a user message and receives an AI response that can query/modify data
    /// </summary>
    /// <param name="message">User's natural language message</param>
    /// <param name="sessionId">Optional session ID to maintain conversation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI response with the requested information or confirmation of action</returns>
    Task<ChatResponse> ChatAsync(string message, string? sessionId = null, CancellationToken cancellationToken = default);
}

public record ChatResponse(
    bool Success,
    string Message,
    string? Error = null,
    string? SessionId = null
);
