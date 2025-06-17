using System.Net.Http.Json;
using bookworm_cli;
using Serilog;

namespace Client;

public class BookwormApiClient(HttpClient httpClient)
    : IBookwormApiClient
{
    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("/books", book, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Error("Failed to add book: {Response}", body);
            throw new HttpRequestException($"'{response.StatusCode}' Book could not be added.");
        }
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("/books", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Book>>(cancellationToken) ?? [];
    }

    public async Task RemoveAsync(string title, CancellationToken cancellationToken)
    {
        var response = await httpClient.DeleteAsync($"/books/{Uri.EscapeDataString(title)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Error("Failed to remove book: {Response}", body);
            throw new HttpRequestException($"'{response.StatusCode}' Book could not be removed.");
        }
    }
}
