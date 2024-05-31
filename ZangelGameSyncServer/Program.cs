using Microsoft.Extensions.Options;
using ZangelGameSyncServer.Endpoints;
using ZangelGameSyncServer.Exceptions;
using ZangelGameSyncServer.Interfaces;
using ZangelGameSyncServer.Options;
using ZangelGameSyncServer.Services;

var builder = WebApplication.CreateBuilder(args);

// setup logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(config =>
{
    config.SingleLine = true;
    config.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
});

// Register Dependency Injections
builder.Services.AddOptions<LocalSaveOptions>()
    .Bind(builder.Configuration.GetSection(LocalSaveOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<LocalSaveOptions>, LocalSaveOptionsValidator>();
builder.Services.AddSingleton<IFolderLockService, FolderLockService>();
builder.Services.AddSingleton<ILocalFolderService, LocalFolderService>();
builder.Services.AddSingleton<IBackupService, ResticBackupService>();


var app = builder.Build();
// Add the exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

// Map Routes
app.MapGet("/check-folder", CheckFolderEndpoint.Get);
app.MapPost("/acquire-folder-lock", AcquireLockEndpoint.Post);
app.MapPost("/release-folder-lock", ReleaseLockEndpoint.Post);
app.MapPost("/create-folder", CreateFolderEndpoint.Post);

app.Run();
