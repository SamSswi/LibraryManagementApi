namespace LibraryManagement.Api.Models;

public class User
{
    public int Id {get; private set;}
    public int Name {get; set;}
    public ICollection<Loan>? Loans {get; set;}
}