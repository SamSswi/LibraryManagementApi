namespace LibraryManagement.Api.GlobalExceptionHandler;
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
        
    }
}