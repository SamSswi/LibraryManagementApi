using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models;

public class User
{
    public int Id {get; private set;}
    [Required]
    public string Name {get; set;} = "";
    public ICollection<Loan>? Loans {get; set;}
}