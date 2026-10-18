namespace LibraryManagement.Api.Filter;

public class LoanFilter : BaseFilter
{

    public int? UserId {get; set;}
    public string? UserName {get; set;}
    public DateOnly? BorrowedAt {get; set;}
    public string? BookTitle {get; set;}
    public LoanItemCategory? Category {get; set;}


}