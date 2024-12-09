namespace SimpleMimo.Services.Exceptions;

/// <summary>
/// Use in the situation when information about not found entity cannot be exposed
/// in the API response. This exception happens when entity not found as a result of
/// unexpected error (for example: database corruption) and not related to the data send to the API.
/// </summary>
/// <param name="id"></param>
/// <param name="entityType"></param>
public class InternalNotFoundException(string id, string entityType)
    : NotFoundException(id, entityType)
{
    
}