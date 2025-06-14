using bookworm_api;

var builder = WebApplication.CreateBuilder(args);
var connStr = builder.Configuration.GetConnectionString("DbConStr") ?? "Data Source = books.db";
Database.Initialize(connStr);
builder.Services.AddSingleton(new BookRepository(connStr));

var app = builder.Build();

app.MapGet("/", () => "Bookworms Db Store Api");

app.MapPost("/books", (BookDto dto, BookRepository repository) =>
{
    var book = new Book(0, dto.Title, dto.Category, dto.Read, DateTime.UtcNow, null);
    repository.Add(book);
    return Results.Created($"/books", dto);
});

app.MapGet("/books", (BookRepository repository) =>
{
    var books = repository.GetAll().Select(b => new BookDto(b.Id, b.Title, b.Category, b.Read));
    return Results.Ok(books);
});

app.MapGet("/books/{title}", (string title, BookRepository repository) =>
{
    var book = repository.GetByTitle(title);
    if (book is null)
    {
        return Results.NotFound(new { message = $"Book with title '{title}' not found :/" });
    }
    return Results.Ok(new BookDto(book.Id, book.Title, book.Category, book.Read));
});

app.MapDelete("/books/{title}", (string title, BookRepository repository) =>
{
    var book = repository.GetByTitle(title);
    if (book is null)
    {
        return Results.NotFound(new { message = $"Book with title '{title}' not found :/" });
    }
    repository.Delete(book.Id);
    return Results.Ok(new { message = $"Book '{title}' removed from deppo successfully." });
});

app.Run();
