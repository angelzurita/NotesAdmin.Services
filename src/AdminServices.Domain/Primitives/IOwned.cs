namespace AdminServices.Domain.Primitives;

/// <summary>
/// Entity that belongs to a single user. The owner is set on insert from the authenticated user and
/// every query is filtered by it, so a user never reads or changes someone else's data.
/// </summary>
public interface IOwned
{
    string OwnerId { get; }
}
