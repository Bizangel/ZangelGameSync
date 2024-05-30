using ZangelGameSyncServer.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/check-folder", CheckFolderEndpoint.Get);


app.Run();
