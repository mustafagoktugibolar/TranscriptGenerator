using TranscriptGenerator.Server.Services.Interfaces;
using TranscriptGenerator.Server.Services;
using TranscriptGenerator.Server.Helpers;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ITranscriptService, TranscriptService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var pythonServiceUrl = config["PythonServiceUrl"] ?? "http://localhost:8000";
    client.BaseAddress = new Uri(pythonServiceUrl);
    client.Timeout = TimeSpan.FromMinutes(30); // Long timeout for CPU-based transcription
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100_000_000;
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100_000_000; 
});

var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LogHelper.Configure(loggerFactory);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
