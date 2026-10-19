using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Authorization;
using LibraryManagement.Api.GlobalExceptionHandler;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthorController(AppDbContext context) : BaseController(context)
{
    [HttpGet ("authors")]
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
                Name = a.Name,
                Books = a.AuthorBooks.Select(ab => ab.Book.Title).ToList()
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
    [HttpGet("authors/{id}")]
    public async Task<ActionResult<ReadAuthorDTO>> GetAuthorById(int id)
    {
        var currAuthor = await FindAuthorById(id);
        var author =  new ReadAuthorDTO
        {
            Id = currAuthor.Id,
            Name = currAuthor.Name,
            Books = currAuthor.AuthorBooks.Select(ab => ab.Book.Title).ToList()
        };
        return author;
    }

    [Authorize (Roles = "Admin")]
    [HttpPost ("authors")]
    public async Task<ActionResult<ReadAuthorDTO>> AddAuthor([FromForm] CreateEditAuthorDTO dto)
    {
        var author = new Author
        {
            Name = dto.Name
        };
        var duplicateAuth = await _context.Author.AnyAsync(a => a.Name.ToLower() == author.Name.ToLower());
        if(duplicateAuth) throw new ConflictException("The Author name has been already added");
        _context.Author.Add(author);
        await _context.SaveChangesAsync();
        var newAuthor = await FindAuthorById(author.Id);
        return Ok(new ReadAuthorDTO
        {
            Id = newAuthor.Id,
            Name = newAuthor.Name,
            Books = newAuthor.AuthorBooks.Select(ab => ab.Book.Title).ToList()
        });
    }

    [Authorize (Roles = "Admin")]
    [HttpPut ("authors/{id}")]
    public async Task<ActionResult<ReadAuthorDTO>> EditAuthor(int id, CreateEditAuthorDTO dto)
    {
        var currAuth = await FindAuthorById(id);
        currAuth.Name = dto.Name;

        await _context.SaveChangesAsync();
        return Ok(new ReadAuthorDTO
        {
            Id = currAuth.Id,
            Name = currAuth.Name,
            Books = currAuth.AuthorBooks.Select(ab => ab.Book.Title).ToList()
        });
    }

    [Authorize (Roles = "Admin")]
    [HttpDelete ("authors/{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var currAuth = await FindAuthorById(id);
        _context.Remove(currAuth);
        await _context.SaveChangesAsync();
        return Ok();
    }
}