using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthorController(AppDbContext context) : BaseController
{
    private readonly AppDbContext _context = context;

    private Author FindAuthorById(int id)
    {
        var currAuth = _context.Author.FirstOrDefault(m => m.Id == id) ?? throw new KeyNotFoundException("Author Not Found");
        return currAuth;
    }

    [HttpGet ("all_authors")]
    public async Task<ActionResult<IEnumerable<ReadAuthorDTO>>> GetAllAuthors(int page = 1, int pageSize = 10)
    {
        var query = _context.Author.Select(a => new ReadAuthorDTO
            {
                Id = a.Id,
                Name = a.Name
            });
        
        var totalAuthors = await query.CountAsync();
        
        Dictionary<string, int> normData = NormalizePageData(page, pageSize, totalAuthors);

        var authorsPerPage = await query
            .OrderBy(a => a.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();

        return Ok(authorsPerPage);
    }

    [HttpGet("get_author_by_id")]
    public ActionResult<ReadAuthorDTO> GetAuthorById(int id)
    {
        var currAuthor = FindAuthorById(id);
        var author =  new ReadAuthorDTO
        {
            Id = currAuthor.Id,
            Name = currAuthor.Name
        };
        return author;
    }

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

    [HttpPut ("edit_author")]
    public async Task<IActionResult> EditAuthor(int id, CreateEditAuthorDTO dto)
    {
        var currAuth = FindAuthorById(id);
        currAuth.Name = dto.Name;

        await _context.SaveChangesAsync();
        return Ok(currAuth);
    }

    [HttpDelete ("delete_author")]
    public IActionResult DeleteAuthor(int id)
    {
        var currAuth = FindAuthorById(id);
        _context.Remove(currAuth);
        _context.SaveChanges();
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