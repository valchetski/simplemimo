using FluentValidation;
using SimpleMimo.Models;

namespace SimpleMimo.Validation;

public class CompletedLessonRequestsValidator : AbstractValidator<CompletedLessonRequest[]>
{
    public CompletedLessonRequestsValidator()
    {
        RuleForEach(x => x)
            .SetValidator(new CompletedLessonRequestValidator());
    }
}