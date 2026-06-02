using FluentValidation;
using Humanizer;
using JasperFx.CodeGeneration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SmartBar.Api.Middleware;
using SmartBar.Application;
using SmartBar.Application.Behaviors;
using SmartBar.Application.Cocktails.Events;
using SmartBar.Application.Cocktails.Queries;
using SmartBar.Application.Interfaces;
using SmartBar.Domain.Entities;
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
    var cocktailCreatedQueue = "cocktail-created-queue";
    var ingredientCreatedQueue = "ingredient-created-queue";
    var cocktailOrderedQueue = "cocktail-ordered-queue";

    opts.ApplicationAssembly = typeof(Program).Assembly;
    opts.Discovery.IncludeAssembly(typeof(SmartBar.Infrastructure.Messaging.CocktailOrderedHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(SmartBar.Infrastructure.Messaging.CocktailCreatedHandler).Assembly);
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
    var rabbitConfig = opts.UseRabbitMq(new Uri("amqp://guest:guest@localhost:5672/")).AutoProvision();
    rabbitConfig.DeclareQueue(cocktailCreatedQueue);
    rabbitConfig.DeclareQueue(ingredientCreatedQueue);
    rabbitConfig.DeclareQueue(cocktailOrderedQueue);

    opts.ListenToRabbitQueue(cocktailCreatedQueue).UseDurableInbox();
    opts.ListenToRabbitQueue(ingredientCreatedQueue).UseDurableInbox();
    opts.ListenToRabbitQueue(cocktailOrderedQueue).UseDurableInbox();

    opts.PublishMessage<CocktailCreatedEvent>().ToRabbitQueue(cocktailCreatedQueue);
    opts.PublishMessage<CocktailOrderedEvent>().ToRabbitQueue(cocktailOrderedQueue);

    opts.PublishAllMessages().ToRabbitTopics("smartbar-exchange");

    opts.OnException<NullReferenceException>().MoveToErrorQueue();
    opts.OnException<InvalidOperationException>().MoveToErrorQueue();
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

await UpdateInventoryAsync(app);

app.Run();

static async Task UpdateInventoryAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var context = await contextFactory.CreateDbContextAsync();

        var existingInventoryIngredientIds = await context.Inventories
            .Select(x => x.IngredientId)
            .ToListAsync();

        var missingIngredients = await context.Ingredients
            .Where(x => !existingInventoryIngredientIds.Contains(x.IngredientId))
            .ToListAsync();

        if (missingIngredients.Any())
        {
            foreach (var ingredient in missingIngredients)
            {
                context.Inventories.Add(new Inventory(ingredient.IngredientId, 0, "ml"));
            }

            await context.SaveChangesAsync();
        }
    }
}