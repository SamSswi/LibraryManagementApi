using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class ReturnLoanItemDTO
{
    [Required]
    public DateOnly ReturnedAt {get; set;}
}