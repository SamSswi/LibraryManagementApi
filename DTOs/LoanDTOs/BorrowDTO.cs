using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class BorrowDTO
{
    [Required]
    public List<CreateLoanItemDTO> LoanItems{get; set;} = [];
}