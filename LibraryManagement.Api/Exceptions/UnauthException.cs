namespace LibraryManagement.Api.GlobalExceptionHandler;
public class UnauthException : Exception
{
    public UnauthException(string message) : base(message)
    {
        
    }
}