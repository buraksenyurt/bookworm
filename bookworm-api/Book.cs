namespace bookworm_api;

public record Book
(
    int Id,
    string Title,
    string Category,
    bool Read,
    DateTime Created,
    DateTime? Modified
);
