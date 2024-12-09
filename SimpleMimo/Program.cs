using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SimpleMimo.Data.Entities;
using SimpleMimo.Models;
using SimpleMimo.Services;
using SimpleMimo.Services.Exceptions;
using SimpleMimo.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddMimoDatabase(builder.Configuration)
    .AddScoped<IValidator<CompletedLessonRequest[]>, CompletedLessonRequestsValidator>()
    .AddFluentValidationAutoValidation(options => options.OverrideDefaultResultFactoryWith<CustomResultFactory>())
    .AddScoped<IUserService, UserService>()
    .AddScoped<IUserProgressService, UserProgressService>()
    .AddScoped<IUserAchievementService, UserAchievementService>();

var app = builder.Build();

app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MimoDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(exceptionHandlerApp 
    => exceptionHandlerApp.Run(context
        =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<SimpleMimo.Program>>();
        switch (exception)
        {
            case NotFoundException:
                return Results.NotFound(new ErrorResponse() { Message = exception.Message }).ExecuteAsync(context);
            case BadHttpRequestException:
                return Results.BadRequest(new ErrorResponse() { Message = exception.Message }).ExecuteAsync(context);
            default:
                logger.LogError(exception, "Unhandled exception");
                return Results.InternalServerError(new ErrorResponse() { Message = "An unexpected error occurred." }).ExecuteAsync(context);
        }
    }));

app.MapPost(
    "/user/lessons",
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    async (
        IUserService userService,
        IUserProgressService userProgressService,
        CancellationToken cancellationToken,
        [FromBody] CompletedLessonRequest[] completedLessons) =>
    {
        var currentUserId = userService.GetCurrentUserId();
        await userProgressService.TrackAsync(currentUserId, completedLessons, cancellationToken);

        return Results.NoContent();
    }).AddFluentValidationAutoValidation();

app.MapGet(
    "/user/achievements",
    async (
        IUserService userService,
        IUserAchievementService userAchievementService,
        CancellationToken cancellationToken) =>
    {
        var currentUserId = userService.GetCurrentUserId();
        return (await userAchievementService
            .GetUserAchievementsAsync(currentUserId, cancellationToken))
            .Select(x => new AchievementResponse()
            {
                Id = x.Id,
                Completed = x.Completed,
                Progress = x.Progress,
            });
    })
    .WithDescription("Returns only those achievements that user started tracking.");

app.Run();

namespace SimpleMimo
{
    public partial class Program { }
}