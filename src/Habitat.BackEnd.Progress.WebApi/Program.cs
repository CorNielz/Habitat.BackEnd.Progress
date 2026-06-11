using System.Text.Json.Serialization;
using Habitat.BackEnd.Progress.Application;
using Habitat.BackEnd.Progress.Infrastructure;
using Habitat.BackEnd.Progress.WebApi.Configuration;
using Habitat.BackEnd.Progress.WebApi.Middleware;
using Habitat.BackEnd.Progress.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        var problem = ProblemDetailsFactory.Create(context.HttpContext, StatusCodes.Status400BadRequest, "Bad Request", "The request contains invalid data.");
        problem.Extensions["errors"] = errors;
        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.AddHabitatSwagger();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Habitat: Progress API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler();
}

if (app.Configuration.GetValue<bool>("Security:UseHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
