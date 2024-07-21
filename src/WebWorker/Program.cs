using WebWorker.Assembly;
using WebWorker.Exceptions;
using WebWorker.Services;
using WebWorker.Models;
using WebWorker.Services.MessageBroker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<WebWorkerAssemblyLoadContext>();
builder.Services.AddSingleton<WorkerRepo>();
builder.Services.AddSingleton<RabbitMQConnectionService>();
builder.Services.AddScoped<WorkerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddLogging();
builder.Services.AddExceptionHandler<ExceptionsHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseStatusCodePages();

app.Run();
