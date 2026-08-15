using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTO;

public class CreateLoanItemDTO
{
    [Required]
    public int BookId {get; set;}
}