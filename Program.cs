using DotNetEnv;
using System.IO;
using VectorLibrary;
using VectorApi;
var builder = WebApplication.CreateBuilder(args);

string configurationFile = Path.Combine(Directory.GetCurrentDirectory(),  "application.env");
Env.Load(configurationFile);

// Add services to the container.
builder.Services.AddSingleton<VectorDbService>();
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Resolve the service
var vectorDbService = app.Services.GetRequiredService<VectorDbService>();
// Perform the initialization
await vectorDbService.InitializeAsync();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();



