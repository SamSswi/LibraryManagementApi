namespace LibraryManagement.Api.DTO;

public class ReadLoanItemDTO
{
    public int Id {get; set;}
    public DateOnly DueDate {get; set;}
    public DateOnly? ReturnedAt {get; set;}
    public int BookId {get; set;}
    public string BookTitle {get; set;} = "";

}