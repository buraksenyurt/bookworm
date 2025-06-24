using System.Net.Http.Json;
using bookworm_cli;
using Serilog;

namespace Client;

public class BookwormApiClient(HttpClient httpClient)
    : IBookwormApiClient
{
    public async Task<bool> AddAsync(Book book, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("/books", book, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Error("Failed to add book: {Response}", body);
            return false;
        }
        return true;
    }

    public async Task<bool> AddAsync(Book book, Guid accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/v2/books")
        {
            Content = JsonContent.Create(book)
        };
        request.Headers.Add("X-Access-Token", accessToken.ToString());
        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Error("Failed to add book with token: {Response}", body);
            if (response.Headers.TryGetValues("X-Invalid-Access-Token", out var _))
            {
                Log.Warning("Invalid token sent");
            }
            if (response.Headers.TryGetValues("X-Expired-Access-Token", out var _))
            {
                Log.Warning("Expired token sent");
            }
            return false;
        }
        return true;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("/books", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<Book>>(cancellationToken) ?? [];
    }

    public async Task<bool> RemoveAsync(string title, CancellationToken cancellationToken)
    {
        var response = await httpClient.DeleteAsync($"/books/{Uri.EscapeDataString(title)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Log.Error("Failed to remove book: {Response}", body);
            return false;
        }
        return true;
    }
}
