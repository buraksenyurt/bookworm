namespace bookworm_cli;

public class Constants
{
    public const string AppName = "BookwormCLI";
    public const string AppDescription = "Manage your book collection with ease.";
    public const string DefaultCategory = "Uncategorized";
    public static readonly string[] Categories =
    [
        "Fiction",
        "Non-Fiction",
        "Science",
        "History",
        "Biography",
        "Fantasy",
        "Mystery",
        "Romance",
        "Horror",
        "Technical-Books",
        DefaultCategory
    ];
}
