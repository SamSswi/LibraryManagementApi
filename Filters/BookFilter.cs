namespace LibraryManagement.Api.Filter;

public class BookFilter : BaseFilter
{
    public string? Search {get; set;}
    public int? PublicationYear {get; set;}
    public int? AvailableCopies {get; set;}

}