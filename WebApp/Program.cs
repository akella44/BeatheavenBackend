using FluentValidation;
using Infrastructure.Data.DbContext;
using Infrastructure.Data.Repositories;
using Serilog;
using WebApp.ExceptionHandlers;
using WebApp.Features;
using WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<TracksRepository>();
builder.Services.AddScoped<MongoDbContext>();
builder.Services.AddScoped<TracksRepository>();

builder.Services.AddSingleton<AudioStreamsContext>();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(
    typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHub<MelodyRecognizeHub>("api/v1/recognize");

GetTrackEndpoint.Map(app);
CreateTrackEndpoint.Map(app);

app.UseMiddleware<ApiKeyAuthMiddleware>();

app.Run();