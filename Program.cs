global using AiSearch;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.Configure<RazorViewEngineOptions>(o =>
{
    o.ViewLocationFormats.Add(
        "/Controllers/{1}/{0}" + RazorViewEngine.ViewExtension
    );
    o.ViewLocationFormats.Add(
        "/Controllers/{1}/Views/{0}" + RazorViewEngine.ViewExtension
    );
    o.ViewLocationFormats.Add(
        "/Controllers/Views/{0}" + RazorViewEngine.ViewExtension
    );
    o.ViewLocationFormats.Add(
        "/Controllers/Views/Shared/{0}" + RazorViewEngine.ViewExtension
    );
});

var oai = new Groq(
    Environment.GetEnvironmentVariable("GROQ_API_KEY")
        ?? throw new Exception("no api key")
);
var searcher = new Searcher(
    Environment.GetEnvironmentVariable("SERER_API_KEY")
        ?? throw new Exception("no api key")
);
var convos = new Dictionary<Guid, ConversationState>();
var taskQueue = new BackgroundTaskQueue(1000);
var converser = new Converser(oai, searcher, convos, taskQueue);

builder.Services.AddSingleton<IBackgroundTaskQueue>(taskQueue);
builder.Services.AddSingleton(converser);
builder.Services.AddHostedService<QueuedHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(
    async (ctx, next) =>
    {
        try
        {
            await next.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"----\n{ex.Demystify()}\n----");
            throw;
        }
    }
);
app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
