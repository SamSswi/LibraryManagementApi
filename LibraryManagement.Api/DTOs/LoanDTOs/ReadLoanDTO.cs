namespace LibraryManagement.Api.DTO;

public class ReadLoanDTO
{
    public int Id {get; set;}
    public int UserId {get; set;}
    public string? UserName{get; set;}
    public DateOnly BorrowedAt {get; set;}
    public ICollection<ReadLoanItemDTO> LoanItems {get; set;} = [];
}