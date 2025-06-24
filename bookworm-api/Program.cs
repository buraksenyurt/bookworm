using bookworm_api;
using bookworm_api.Auth;
using bookworm_api.Repository;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.File("logs/errors.log", restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();

var connStr = builder.Configuration.GetConnectionString("DbConStr") ?? "Data Source = books.db";
Database.Initialize(connStr);
builder.Services.AddSingleton<IBookRepository>(new BookRepository(connStr));
builder.Services.AddScoped<ITokenValidator, TokenValidator>();

var app = builder.Build();

app.UseExceptionHandler(app =>
{
    app.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = "Something went wrong. Please try again later."
        });
    });
});

app.MapGet("/", () => "Bookworms Db Store Api");

app.MapPost("/books", async (BookDto dto, IBookRepository repository) =>
{
    var isExist = await repository.GetByTitleAsync(dto.Title);
    if (isExist != null)
    {
        return Results.BadRequest(new { message = $"Book with title '{dto.Title}' already exist" });
    }
    var book = new Book(0, dto.Title, dto.Category, dto.Read, DateTime.UtcNow, null);
    await repository.AddAsync(book);
    Log.Information($"'{dto.Title}' added to deppo successfully");
    return Results.Created($"/books", dto);
});

app.MapPost("/v2/books", async ([FromHeader(Name = "X-Access-Token")] string userToken, BookDto dto, ITokenValidator tokenValidator, IBookRepository repository, HttpContext httpContext) =>
{
    if (!Guid.TryParse(userToken, out var token))
    {
        Log.Warning("Invalid user token format");
        return Results.BadRequest(new { message = "Invalid user token format" });
    }
    if (!tokenValidator.IsValid(token))
    {
        Log.Warning("Invalid user token");
        httpContext!.Response.Headers["X-Invalid-Access-Token"] = token.ToString();
        return Results.Unauthorized();
    }
    if (tokenValidator.IsExpired(token))
    {
        Log.Warning("User token is expired");
        httpContext!.Response.Headers["X-Expired-Access-Token"] = token.ToString();
        return Results.Unauthorized();
    }

    var isExist = await repository.GetByTitleAsync(dto.Title);
    if (isExist != null)
    {
        return Results.BadRequest(new { message = $"Book with title '{dto.Title}' already exist" });
    }
    var book = new Book(0, dto.Title, dto.Category, dto.Read, DateTime.UtcNow, null);
    await repository.AddAsync(book);
    Log.Information($"'{dto.Title}' added to deppo successfully");
    return Results.Created($"/books", dto);
});

app.MapGet("/books", async (IBookRepository repository) =>
{
    var books = await repository.GetAllAsync();
    var dtos = books.Select(b => new BookDto(b.Id, b.Title, b.Category, Convert.ToBoolean(b.Read)));
    Log.Information($"Fetching '{dtos.Count()}' books");
    return Results.Ok(books);
});

app.MapGet("/books/{title}", async (string title, IBookRepository repository) =>
{
    var book = await repository.GetByTitleAsync(title);
    if (book is null)
    {
        Log.Warning($"Book with title '{title}' not found");
        return Results.NotFound(new { message = $"Book with title '{title}' not found :/" });
    }
    Log.Information($"Returning '{book.Title}'");
    return Results.Ok(new BookDto(book.Id, book.Title, book.Category, book.Read));
});

app.MapDelete("/books/{title}", async (string title, IBookRepository repository) =>
{
    var book = await repository.GetByTitleAsync(title);
    if (book is null)
    {
        Log.Warning($"Book with title '{title}' not found");
        return Results.NotFound(new { message = $"Book with title '{title}' not found :/" });
    }
    await repository.DeleteAsync(book.Id);
    Log.Information($"Book Id {book.Id},'{book.Title}' has been deleted");
    return Results.Ok(new { message = $"Book '{title}' removed from deppo successfully." });
});

app.Run();
