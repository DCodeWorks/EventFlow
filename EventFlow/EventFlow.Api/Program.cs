using EventFlow.Application.CommandHandlers;
using EventFlow.Infrastructure.Messaging;
using EventFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommandHandler).Assembly);
});

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

//DB setup
var connectionString = builder.Configuration.GetConnectionString("EventFlowDb");
builder.Services.AddDbContext<EventFlowDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddScoped<ITaskRepository, SqlTaskRepository>();

// Register the background service for read model consumption
builder.Services.AddHostedService<TaskReadModelConsumer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EventFlowDbContext>();
    if (!dbContext.Database.CanConnect())
    {
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
