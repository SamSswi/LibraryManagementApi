using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class EditLoanDTO
{
    [Required]
    public int UserId {get; set;}
    [Required]
    public List<EditLoanItemDTO> LoanItems{get; set;} = [];
}