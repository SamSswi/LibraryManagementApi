namespace LibraryManagement.Api.DTO;

public class EditLoanItemDTO
{
    public int Id {get; set;}
    public DateOnly DueDate {get; set;}
    public DateOnly?  ReturnedAt {get; set;}
    public int BookId {get; set;}
}