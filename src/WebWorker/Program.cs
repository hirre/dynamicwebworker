using WebWorker.Assembly;
using WebWorker.Logic;
using WebWorker.MessageBroker;
using WebWorker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<WebWorkerAssemblyLoadContext>();
builder.Services.AddSingleton<WorkerRepo>();
builder.Services.AddSingleton<RabbitMQConnectionService>();
builder.Services.AddScoped<WorkerLogic>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseStatusCodePages();
app.UseExceptionHandler();

app.Run();
