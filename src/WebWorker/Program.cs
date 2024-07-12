using WebWorker.Assembly;
using WebWorker.Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<WebWorkerAssemblyLoadContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var cancellationTokenSource = new CancellationTokenSource();
RegisterWorkerServices();

app.Run();


void RegisterWorkerServices()
{
    var wwAssemblyContext = app.Services.GetRequiredService<WebWorkerAssemblyLoadContext>();
    wwAssemblyContext.InitWorkDirectory();

    var workerServiceList = LoadWorkerServices();

    var hostApplicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    hostApplicationLifetime.ApplicationStarted.Register(() =>
    {
        foreach (var workerService in workerServiceList)
        {
            Task.Factory.StartNew(() => workerService.StartAsync(cancellationTokenSource.Token),
                CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    });

    hostApplicationLifetime.ApplicationStopping.Register(() =>
    {
        foreach (var workerService in workerServiceList)
        {
            workerService.StopAsync(cancellationTokenSource.Token);
        }
    });
}

List<AssemblyWorker> LoadWorkerServices()
{
    var serviceList = new List<AssemblyWorker>();

    var nrOfWorkerSection = app.Configuration.GetSection("NrOfWorkers");
    var nrOfWorkers = 0;

    if (!nrOfWorkerSection.Exists())
    {
        nrOfWorkers = 1;
    }
    else
    {
        nrOfWorkers = nrOfWorkerSection.Get<int>();
    }

    // Get number of worker services to load from configuration
    for (int i = 0; i < nrOfWorkers; i++)
    {
        //var service = ActivatorUtilities.CreateInstance<AssemblyWorker>(app.Services);
        var service = new AssemblyWorker(app.Services.GetRequiredService<ILogger<AssemblyWorker>>(),
            app.Services.GetRequiredService<WebWorkerAssemblyLoadContext>());
        serviceList.Add(service);
    }

    return serviceList;
}
