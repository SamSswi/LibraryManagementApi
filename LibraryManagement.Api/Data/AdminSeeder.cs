using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Services;

internal class AdminSeeder(IAuthService authService, AppDbContext context)
{
    protected readonly AppDbContext _context = context;
    //Seed an admin account on app startup
    // intended for immediate admin rights after cloning
    internal async Task SeedAdmin()
    {
        var dto = new CreateEditUserDTO
        {
            Name = "Admin",
            Password = "PassWord"
        };
        var user = await authService.RegisterAsync(dto);
        if(user != null)
        {
            user.Role = "Admin";
            await _context.SaveChangesAsync();
        }
    }
}