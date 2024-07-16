using WebWorker.Assembly;
using WebWorker.Logic;
using WebWorker.Models;
using WebWorker.Queue;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<WebWorkerAssemblyLoadContext>();
builder.Services.AddSingleton<WorkerRepo>();
builder.Services.AddSingleton<RabbitMQConnectionService>();
builder.Services.AddScoped<WorkLogic>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

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
