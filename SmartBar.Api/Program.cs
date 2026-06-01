using FluentValidation;
using Humanizer;
using JasperFx.CodeGeneration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SmartBar.Api.Middleware;
using SmartBar.Application;
using SmartBar.Application.Behaviors;
using SmartBar.Application.Cocktails.Queries;
using SmartBar.Application.Interfaces;
using SmartBar.Infrastructure;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SmartBarConnection");
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");

builder.Services.AddScoped<GetCocktailsQueryHandler>();
builder.Services.AddScoped<CachedGetCocktailsQueryHandler>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "SmartBar_";
});

builder.Host.UseWolverine(opts =>
{
    opts.ApplicationAssembly = typeof(Program).Assembly;
    opts.Discovery.IncludeAssembly(typeof(SmartBar.Infrastructure.Messaging.CocktailCreatedHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(SmartBar.Infrastructure.Messaging.DecreaseInventoryHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(SmartBar.Infrastructure.Messaging.IngredientCreatedHandler).Assembly);

    if (builder.Environment.IsDevelopment())
    {
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
    }
    else
    {
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;
    }

    opts.PersistMessagesWithPostgresql(connectionString ?? throw new InvalidOperationException("Connection string not found."));
    opts.UseRabbitMq(new Uri("amqp://guest:guest@localhost:5672/")).AutoProvision();

    opts.ListenToRabbitQueue("cocktail-created-queue").UseDurableInbox();
    opts.ListenToRabbitQueue("ingredient-created-queue").UseDurableInbox();
    opts.ListenToRabbitQueue("inventory-cocktail-created-queue").UseDurableInbox();

    opts.PublishAllMessages().ToRabbitTopics("smartbar-exchange");
    opts.OnException<NullReferenceException>().MoveToErrorQueue();
    opts.OnException<ArgumentNullException>().MoveToErrorQueue();

    opts.OnException<Exception>()
        .RetryWithCooldown(2.Seconds(), 5.Seconds(), 10.Seconds());
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