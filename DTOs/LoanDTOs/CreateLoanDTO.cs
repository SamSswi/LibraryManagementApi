using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateLoanDTO
{
    [Required]
    public int UserId {get; set;}
    [Required]
    public List<CreateLoanItemDTO> LoanItems{get; set;} = [];
}