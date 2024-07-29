using System.Reflection;
using WebWorker.Assembly;
using WebWorker.Exceptions;
using WebWorker.Models;
using WebWorker.Services.MessageBroker;
using WebWorker.Services.Worker;
using WebWorkerInterfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<WorkerRepo>();
builder.Services.AddSingleton<WorkPluginRepo>();
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

LoadWorkPlugins(app.Services.GetService<WorkPluginRepo>());

app.Run();


// Load work plugins
void LoadWorkPlugins(WorkPluginRepo? repo)
{
    if (repo == null)
    {
        throw new ArgumentNullException(nameof(repo));
    }

    var workFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Work");

    foreach (var workPluginFolder in Directory.EnumerateDirectories(workFolder))
    {
        var workPlugin = Path.Combine(workFolder, workPluginFolder);

        Directory.EnumerateFiles(workPlugin, "*.dll").ToList().ForEach(file =>
        {
            var workerAssemblyLoadContext = new WebWorkerAssemblyLoadContext(file);
            var assembly = workerAssemblyLoadContext.LoadFromAssemblyPath(file);

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IWork).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is IWork result)
                    {
                        repo.AddWorkPlugin(result.GetType().Name, result);
                    }
                }
            }
        });
    }
}
