using AdminServices.Application.Common.Interfaces;
using MediatR;

namespace AdminServices.Application.Ai.Queries.ChatQuery;

public class AiChatQueryHandler : IRequestHandler<AiChatQuery, ChatResponse>
{
    private readonly IChatService _chatService;

    public AiChatQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ChatResponse> Handle(AiChatQuery request, CancellationToken cancellationToken)
    {
        return await _chatService.ChatAsync(request.Message, request.SessionId, cancellationToken);
    }
}
