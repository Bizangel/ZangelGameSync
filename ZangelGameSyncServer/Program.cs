using ZangelGameSyncServer.Endpoints;
using ZangelGameSyncServer.Interfaces;
using ZangelGameSyncServer.Options;
using ZangelGameSyncServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Dependency Injections
builder.Services.Configure<LocalSaveOptions>(
    builder.Configuration.GetSection(LocalSaveOptions.SECTION_NAME)
);

builder.Services.AddSingleton<ILocalFolderService, LocalFolderService>();

var app = builder.Build();

// Map Routes
app.MapGet("/check-folder", CheckFolderEndpoint.Get);


app.Run();
