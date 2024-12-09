using FluentValidation.Results;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SimpleMimo.Models;

namespace SimpleMimo.Validation;

public class CustomResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext context, ValidationResult validationResult)
    {
        return Results.BadRequest(new ErrorResponse()
        {
            Message = "Bad Request",
            Errors = validationResult.Errors.ToDictionary(k => k.PropertyName, v => v.ErrorMessage),
        });
    }
}