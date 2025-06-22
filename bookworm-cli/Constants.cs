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

public static class Messages
{
    public static class ValidationMessages
    {
        public const string TitleCannotBeEmpty = "Title cannot be null or empty.";
        public const string TitleExceedsMaxLength = "Title cannot exceed 50 characters.";
        public const string FilePathCannotBeEmpty = "File path cannot be null or empty.";
        public const string FileMustHaveJsonExtension = "File must have a json extension.";
    }
    public static class AddCommandMessages
    {

        public const string BookAddedSuccessfully = "Book added successfully.";
        public const string BookCouldNotBeAdded = "Book could not be added.";
    }
    public static class ListCommandMessages
    {
        public const string NoBooksFound = "No books found.";
        public const string BooksFound = "Books found.";
    }
    public static class RemoveCommandMessages
    {
        public const string BookRemovedSuccessfully = "Book removed successfully.";
        public const string BookCouldNotBeRemoved = "Book could not be removed.";
    }
    public static class ExportCommandMessages
    {
        public const string ExportedToFile = "Books exported successfully.";
        public const string NoBooksExported = "No books could be exported.";
        public const string ThereIsNoBooksToExport = "There is no books in store for exporting";
    }
    public static class ImportCommandMessages
    {
        public const string ImportSuccessfully = "Books imported successfully.";
        public const string NoBooksAdded = "No books could be added.";
        public const string NoBooksFoundInFile = "No books found in the file.";
    }
}
