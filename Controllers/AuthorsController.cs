using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthorController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private Author FindAuthorById(int id)
    {
        var currAuth = _context.Author.FirstOrDefault(m => m.Id == id) ?? throw new KeyNotFoundException("Author Not Found");
        return currAuth;
    }

    [HttpGet ("all_authors")]
    public List<Author> GetAllAuthors()
    {
        return [.. _context.Author];
    }

    [HttpGet("get_author_by_id")]
    public Author GetAuthorById(int id)
    {
        var currAuthor = FindAuthorById(id);
        return currAuthor;
    }

    [HttpPost ("add_author")]
    public IActionResult AddAuthor([FromForm] Author author)
    {
        _context.Add(author);
        _context.SaveChanges();
        return Ok(author);
    }

    [HttpPut ("edit_author")]
    public IActionResult EditAuthor(int id, Author author)
    {
        var currAuth = FindAuthorById(id);
        currAuth.Name = author.Name;

        _context.SaveChanges();
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
    
}