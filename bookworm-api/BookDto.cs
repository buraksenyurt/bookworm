namespace bookworm_api;

public record BookDto
(
    int Id,
    string Title,
    string Category,
    bool Read
);
