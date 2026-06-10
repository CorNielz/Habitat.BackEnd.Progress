using Habitat.BackEnd.Progress.Application;
using Habitat.BackEnd.Progress.Application.Authorization;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure;
using Habitat.BackEnd.Progress.WebApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(UserRole.Admin.ToString()));

    options.AddPolicy(AuthorizationPolicies.CommonOrAdmin, policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole(UserRole.Common.ToString(), UserRole.Admin.ToString()));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
