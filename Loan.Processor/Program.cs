using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Identity;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Loan.Processor;
using Loan.Processor.Consumers;
using Loan.Processor.Workflows;
using MassTransit;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is missing. Check User Secrets.");
}

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LoanRequestConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddElsa(elsa =>
{
    elsa.AddWorkflow<LoanApprovalWorkflow>();

    elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef =>
    {
        ef.UseSqlServer(connectionString);
        ef.RunMigrations = true;
    }));

    elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef => ef.UseSqlServer(connectionString)));

    elsa.UseIdentity(identity =>
    {
        identity.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlServer(connectionString);
            ef.RunMigrations = true;
        });

        identity.TokenOptions = options =>
        {
            options.SigningKey = builder.Configuration["Security:ElsaSigningKey"];

            options.AccessTokenLifetime = TimeSpan.FromHours(1);
        };

        identity.UseAdminUserProvider();
    });

    // Configure ASP.NET authentication/authorization.
    elsa.UseDefaultAuthentication(auth => auth.UseAdminApiKey());

    // Expose Elsa API endpoints.
    elsa.UseWorkflowsApi();

    // Setup a SignalR hub for real-time updates from the server.
    // elsa.UseRealTimeWorkflows();

    // Enable C# workflow expressions
    elsa.UseCSharp();

    // Enable JavaScript workflow expressions
    // elsa.UseJavaScript(options => options.AllowClrAccess = true);

    // Enable HTTP activities.
    elsa.UseHttp(options => options.ConfigureHttpOptions = httpOptions => httpOptions.BaseUrl = new("https://localhost:5001"));

    // Use timer activities.
    // elsa.UseScheduling();

    // Register custom activities from the application, if any.
    // elsa.AddActivitiesFrom<Program>();

    // Register custom workflows from the application, if any.
    // elsa.AddWorkflowsFrom<Program>();
});

builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()));


var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflowsApi();
app.UseWorkflows();


app.Run();