using System.Collections.Concurrent;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AdminServices.Infrastructure.Services;

/// <summary>
/// Singleton store that persists chat histories in memory keyed by sessionId.
/// Sessions inactive for more than 2 hours are automatically purged.
/// </summary>
public class ChatHistoryStore
{
    private readonly ConcurrentDictionary<string, SessionEntry> _sessions = new();
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(2);
    private DateTime _lastCleanup = DateTime.UtcNow;

    private sealed record SessionEntry(ChatHistory History, DateTime CreatedAt)
    {
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    }

    public ChatHistory GetOrCreate(string sessionId, string systemPrompt)
    {
        PurgeExpiredIfNeeded();

        var entry = _sessions.GetOrAdd(sessionId, _ =>
        {
            var history = new ChatHistory(systemPrompt);
            return new SessionEntry(history, DateTime.UtcNow);
        });

        entry.LastAccessed = DateTime.UtcNow;
        return entry.History;
    }

    public void Remove(string sessionId) => _sessions.TryRemove(sessionId, out _);

    public int ActiveSessions => _sessions.Count;

    private void PurgeExpiredIfNeeded()
    {
        if (DateTime.UtcNow - _lastCleanup < TimeSpan.FromMinutes(30))
            return;

        _lastCleanup = DateTime.UtcNow;
        var expired = _sessions
            .Where(kv => DateTime.UtcNow - kv.Value.LastAccessed > SessionTtl)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in expired)
            _sessions.TryRemove(key, out _);
    }
}
