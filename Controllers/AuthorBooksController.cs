using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthorBookController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private AuthorBook FindAuthorBookById(int authorId, int bookId)
    {
        var authBookRel = _context.AuthorBooks.FirstOrDefault(ab => ab.AuthorId == authorId && ab.BookId == bookId) 
            ?? throw new KeyNotFoundException("Author Not Found");
        return authBookRel;
    }

    
    private List<Book> FindBooksByAnAuthor(int authorId)
    {
        return [.. _context.AuthorBooks.Where(a => a.AuthorId == authorId).Select(ab => ab.Book)];
    }

    private List<Author> FindAuthorsOfABook(int bookId)
    {
        return [.. _context.AuthorBooks.Where(a => a.BookId == bookId).Select(ab => ab.Author)];
    }


    [HttpGet("authors/{id}/books")]
    public List<Book> BooksByAuthor (int authorId)
    {
        return FindBooksByAnAuthor(authorId);
    }

    [HttpGet ("books/{id}/authors")]
    public List<Author> AuthorsOfBook(int bookId)
    {
        return FindAuthorsOfABook(bookId);
    }


    [HttpPost ("add_authorbooks")]
    public IActionResult AddAuthor([FromForm] AuthorBook authorBook)
    {
        _context.Add(authorBook);
        _context.SaveChanges();
        return Ok(authorBook);
    }

    [HttpDelete ("authorbooks/{authorId}/{bookId}")]
    public IActionResult DeleteAuthor(int authorId, int bookId)
    {
        var currAuthBook = FindAuthorBookById(authorId,bookId);
        _context.Remove(currAuthBook);
        _context.SaveChanges();
        return Ok();
    }
    
}