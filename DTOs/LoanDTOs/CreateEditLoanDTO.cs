using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateEditLoanDTO
{
    [Required]
    public int UserId {get; set;}
    [Required]
    public DateOnly BorrowedAt {get; set;}
    [Required]
    public List<CreateEditLoanItemDTO> LoanItems{get; set;} = [];
}