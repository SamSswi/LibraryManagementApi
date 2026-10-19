using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Authorization;
namespace LibraryManagement.Api.Controllers;

[ApiController] // user can view their own data
[Route("api/[controller]")]

public class UserController(AppDbContext context) : BaseController(context)
{
    [Authorize(Roles = "Admin")]
    [HttpGet ("users")]
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

    [Authorize (Roles = "Admin")]
    [HttpGet("users/{id}")]
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

    [Authorize]
    [HttpGet("me/users")]
    public async Task<ActionResult<ReadUserDTO>> GetSelf()
    {
        int userId = ExtractId();
        var currUser = await FindUserById(userId);
        var user =  new ReadUserDTO
        {
            Id = currUser.Id,
            Name = currUser.Name
        };
        return user;
    }

    [Authorize (Roles = "Admin")]
    [HttpPut ("users/{id}")]
    public async Task<IActionResult> EditUser(int id, CreateEditUserDTO dto)
    {
        var currUser = await FindUserById(id);
        currUser.Name = dto.Name;

        await _context.SaveChangesAsync();
        return Ok(currUser);
    }

    [Authorize (Roles = "Admin")]
    [HttpDelete ("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currUser = await FindUserById(id);
        _context.Remove(currUser);
        await _context.SaveChangesAsync();
        return Ok();
    }
}