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
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? throw new InvalidOperationException("RabbitMQ Username missing");
        var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ Password missing");

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
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
            options.SigningKey = builder.Configuration["Security:ElsaSigningKey"]!;

            options.AccessTokenLifetime = TimeSpan.FromHours(1);
        };

        identity.UseAdminUserProvider();
    });

    elsa.UseDefaultAuthentication(auth => auth.UseAdminApiKey());

    elsa.UseWorkflowsApi();

    elsa.UseCSharp();

    elsa.UseHttp(options => options.ConfigureHttpOptions = httpOptions => httpOptions.BaseUrl = new("https://localhost:5001"));
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "SmartLoan_";
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