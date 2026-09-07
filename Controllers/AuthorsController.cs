using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagement.Api.Controllers;

[ApiController] // all authorized besides view
[Route("api/[controller]")]

public class AuthorController(AppDbContext context) : BaseController
{
    private readonly AppDbContext _context = context;

    private async Task<Author> FindAuthorById(int id)
    {
        var currAuth = await _context.Author.FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException("Author Not Found");
        return currAuth;
    }

    [HttpGet ("all_authors")]
    public async Task<ActionResult<IEnumerable<ReadAuthorDTO>>> GetAllAuthors([FromQuery] StringFilter filter)
    {
        var query = _context.Author.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(a => a.Name.ToLower().Contains(filter.Search.ToLower()));
        }

        var authors = query.Select(a => new ReadAuthorDTO
            {
                Id = a.Id,
                Name = a.Name
            });
        
        var totalAuthors = await authors.CountAsync();
        
        Dictionary<string, int> normData = NormalizePageData(filter.Page, filter.PageSize, totalAuthors);

        var authorsPerPage = await authors
            .OrderBy(a => a.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();

        return Ok(authorsPerPage);
    }
    
    [Authorize (Roles = "Admin")]
    [HttpGet("get_author_by_id")]
    public async Task<ActionResult<ReadAuthorDTO>> GetAuthorById(int id)
    {
        var currAuthor = await FindAuthorById(id);
        var author =  new ReadAuthorDTO
        {
            Id = currAuthor.Id,
            Name = currAuthor.Name
        };
        return author;
    }

    [Authorize (Roles = "Admin")]
    [HttpPost ("add_author")]
    public async Task<IActionResult> AddAuthor([FromForm] CreateEditAuthorDTO dto)
    {
        var author = new Author
        {
            Name = dto.Name
        };
        _context.Author.Add(author);
        await _context.SaveChangesAsync();
        return Ok(author);
    }

    [Authorize (Roles = "Admin")]
    [HttpPut ("edit_author")]
    public async Task<IActionResult> EditAuthor(int id, CreateEditAuthorDTO dto)
    {
        var currAuth = await FindAuthorById(id);
        currAuth.Name = dto.Name;

        await _context.SaveChangesAsync();
        return Ok(currAuth);
    }

    [Authorize (Roles = "Admin")]
    [HttpDelete ("delete_author")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var currAuth = await FindAuthorById(id);
        _context.Remove(currAuth);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // [HttpPost ("seed-author")]
    // public async Task<IActionResult> SeedAuthor()
    // {
    //     for(int i = 0; i < 100; i++)
    //     {
    //         _context.Author.Add(new Author
    //         {
    //             Name = $"Name{i}"
    //         });
    //     }
    //     await _context.SaveChangesAsync();
    //     return Ok();
    // }
    
}