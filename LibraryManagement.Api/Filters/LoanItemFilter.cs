namespace LibraryManagement.Api.Filter;

public class LoanItemFilter : BaseFilter
{
    public string? BookTitle {get; set;}
    
    public LoanItemCategory? Category {get; set;}
}