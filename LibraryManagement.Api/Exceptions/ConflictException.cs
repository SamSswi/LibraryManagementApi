namespace LibraryManagement.Api.GlobalExceptionHandler;
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
        
    }
}