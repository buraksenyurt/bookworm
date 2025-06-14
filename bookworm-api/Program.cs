using bookworm_api;

var builder = WebApplication.CreateBuilder(args);
var connStr = builder.Configuration.GetConnectionString("DbConStr") ?? "Data Source = books.db";
Database.Initialize(connStr);
builder.Services.AddSingleton<IBookRepository>(new BookRepository(connStr));

var app = builder.Build();

app.MapGet("/", () => "Bookworms Db Store Api");

app.MapPost("/books", async (BookDto dto, IBookRepository repository) =>
{
    var book = new Book(0, dto.Title, dto.Category, dto.Read, DateTime.UtcNow, null);
    await repository.AddAsync(book);
    return Results.Created($"/books", dto);
});

app.MapGet("/books", async (IBookRepository repository) =>
{
    var books = await repository.GetAllAsync();
    var dtos = books.Select(b => new BookDto(b.Id, b.Title, b.Category, Convert.ToBoolean(b.Read)));
    return Results.Ok(books);
});

app.MapGet("/books/{title}", async (string title, IBookRepository repository) =>
{
    var book = await repository.GetByTitleAsync(title);
    if (book is null)
    {
        return Results.NotFound(new { message = $"Book with title '{title}' not found :/" });
    }
    return Results.Ok(new BookDto(book.Id, book.Title, book.Category, book.Read));
});

app.MapDelete("/books/{title}", async (string title, IBookRepository repository) =>
{
    var book = await repository.GetByTitleAsync(title);
    if (book is null)
    {
        return Results.NotFound(new { message = $"Book with title '{title}' not found :/" });
    }
    await repository.DeleteAsync(book.Id);
    return Results.Ok(new { message = $"Book '{title}' removed from deppo successfully." });
});

app.Run();
