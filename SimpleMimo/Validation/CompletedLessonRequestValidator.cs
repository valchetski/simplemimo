using FluentValidation;
using SimpleMimo.Models;

namespace SimpleMimo.Validation;

public class CompletedLessonRequestValidator : AbstractValidator<CompletedLessonRequest>
{
    public CompletedLessonRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.CompleteDate).NotEmpty();
    }
}