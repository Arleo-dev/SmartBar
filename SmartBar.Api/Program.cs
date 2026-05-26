using FluentValidation;
using Humanizer;
using JasperFx.CodeGeneration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Scalar.AspNetCore;
using SmartBar.Api.Middleware;
using SmartBar.Application;
using SmartBar.Application.Behaviors;
using SmartBar.Application.Cocktails.Queries;
using SmartBar.Application.Interfaces;
using SmartBar.Infrastructure;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SmartBarConnection");
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");

builder.Services.AddScoped<GetCocktailsQueryHandler>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddScoped<IRequestHandler<GetCocktailsQuery, IEnumerable<CocktailResponse>>>(provider =>
    new CachedGetCocktailsQueryHandler(
        provider.GetRequiredService<GetCocktailsQueryHandler>(),
        provider.GetRequiredService<IDistributedCache>()
    ));

builder.Services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "SmartBar_";
});

builder.Host.UseWolverine(opts =>
{
    opts.ApplicationAssembly = typeof(Program).Assembly;
    opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;

    opts.PersistMessagesWithPostgresql(connectionString);

    opts.UseEntityFrameworkCoreTransactions();

    opts.UseRabbitMq(new Uri("amqp://guest:guest@localhost:5672/")).AutoProvision();

    opts.ListenToRabbitQueue("ingredient-created-queue").UseDurableInbox();
    opts.ListenToRabbitQueue("cocktail-created-queue").UseDurableInbox();

    opts.Policies.OnException<Exception>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

    opts.PublishAllMessages().ToRabbitExchange("smartbar-exchange");
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("SmartBar API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11);
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();