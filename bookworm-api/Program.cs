using bookworm_api;

var builder = WebApplication.CreateBuilder(args);
var connStr = builder.Configuration.GetConnectionString("DbConStr") ?? "Data Source = books.db";
Database.Initialize(connStr);

var app = builder.Build();

app.MapGet("/", () => "Bookworms Db Store Api");

app.Run();
