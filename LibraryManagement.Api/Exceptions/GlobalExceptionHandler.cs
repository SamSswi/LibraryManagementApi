namespace LibraryManagement.Api.GlobalExceptionHandler;
using Microsoft.AspNetCore.Diagnostics;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is NotFoundException)
        {
            httpContext.Response.StatusCode = 404;

            await httpContext.Response.WriteAsJsonAsync(
                new { message = exception.Message },
                cancellationToken);

            return true;
        }

        if(exception is ConflictException)
        {
            httpContext.Response.StatusCode = 409;
            
            await httpContext.Response.WriteAsJsonAsync(
                new {message = exception.Message},
                cancellationToken);
            
            return true;
        }

        if(exception is UnauthException)
        {
            httpContext.Response.StatusCode = 401;
            
            await httpContext.Response.WriteAsJsonAsync(
                new {message = exception.Message},
                cancellationToken);
            
            return true;
        }

        return false;
    }
}


