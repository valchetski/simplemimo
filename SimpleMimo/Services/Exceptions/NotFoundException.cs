namespace SimpleMimo.Services.Exceptions;

public class NotFoundException(string id, string entityType)
    : Exception($"\"{entityType}\" with \"{id}\" ID was not found.")
{
    public string EntityType { get; } = entityType;
    public string Id { get; } = id;
}