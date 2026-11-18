namespace FloorPlanner.Core.Exceptions;

/// <summary>Thrown when a requested entity does not exist.</summary>
public sealed class NotFoundException(string entityName, object key) : Exception(CreateMessage(entityName, key))
{
    private static string CreateMessage(string entityName, object key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        ArgumentNullException.ThrowIfNull(key);

        return $"{entityName} with id {key} was not found.";
    }
}
