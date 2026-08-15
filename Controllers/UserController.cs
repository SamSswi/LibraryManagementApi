using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Filter;
namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class UserController(AppDbContext context) : BaseController
{
    private readonly AppDbContext _context = context;

    private async Task<User> FindUserById(int id)
    {
        var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException("User Not Found");
        return user;
    }

    [HttpGet ("all_users")]
    public async Task<ActionResult<IEnumerable<ReadUserDTO>>> GetAllUsers([FromQuery] StringFilter filter)
    {
        var query = _context.User.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(u => u.Name.ToLower().Contains(filter.Search.ToLower()));
        }

        var users = query.Select(u => new ReadUserDTO
        {
            Id = u.Id,
            Name = u.Name
        });

        var totalUsers = await users.CountAsync();

        Dictionary<string, int> normData = NormalizePageData(filter.Page, filter.PageSize, totalUsers);

        var usersPerPage = await users
            .OrderBy(u => u.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();
            
        return Ok(usersPerPage);
        
    }

    [HttpGet("get_user_by_id")]
    public async Task<ActionResult<ReadUserDTO>> GetUserById(int id)
    {
        var currUser = await FindUserById(id);
        var user =  new ReadUserDTO
        {
            Id = currUser.Id,
            Name = currUser.Name
        };
        return user;
    }

    [HttpPost ("add_user")]
    public async Task<IActionResult> AddUser([FromForm] CreateEditUserDTO dto)
    {
        var user = new User
        {
            Name = dto.Name
        };
        _context.User.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPut ("edit_user")]
    public async Task<IActionResult> EditUser(int id, CreateEditUserDTO dto)
    {
        var currUser = await FindUserById(id);
        currUser.Name = dto.Name;

        await _context.SaveChangesAsync();
        return Ok(currUser);
    }

    [HttpDelete ("delete_user")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currUser = await FindUserById(id);
        _context.Remove(currUser);
        await _context.SaveChangesAsync();
        return Ok();
    }
    // [HttpPost ("seed-user")]
    // public async Task<IActionResult> SeedUser()
    // {
    //     for(int i = 0; i < 100; i++)
    //     {
    //         _context.User.Add(new User
    //         {
    //             Name = $"Name{i}"
    //         });
    //     }
    //     await _context.SaveChangesAsync();
    //     return Ok();
    // }

}