using System.ComponentModel;
using System.Diagnostics;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthorBookController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet ("all-books-and-authors")]
    public async Task<ActionResult<List<AuthorBookReadDTO>>> AllAuthorBooks()
    {
        var authorBooks = await _context.AuthorBooks.Select(ab => new AuthorBookReadDTO
        {
            AuthorId = ab.AuthorId,
            AuthorName = ab.Author.Name,
            BookId = ab.BookId,
            BookTitle = ab.Book.Title,
        }).ToListAsync();
        return Ok(authorBooks);
    }

    [HttpGet ("author-{authorId}/books")]
    // for some reason there is a second irrelevant id in swagger i think it's from calling on readbooksdto
    public async Task<ActionResult<List<ReadBooksDTO>>> FindBooksByAnAuthor(int authorId)
    {
        var authorExists = await _context.Author.AnyAsync(a => a.Id == authorId);
        if(!authorExists) return NotFound("Author not found");
        var books = await _context.AuthorBooks
            .Where(ab => ab.AuthorId == authorId)
            .Select(ab => new ReadBooksDTO
            {
                Id = ab.Book.Id,
                Title = ab.Book.Title
            })
            .ToListAsync();
        if(books.Count == 0) return NotFound("No books by this author");
        return Ok(books);
    }


    [HttpGet ("book-{bookId}/authors")]
    // for some reason there is a second irrelevant id in swagger i think it's from calling on readbooksdto
    public async Task<ActionResult<List<ReadAuthorDTO>>> FindAuthorsOfABook(int bookId)
    {
        var bookExists = await _context.Book.AnyAsync(b => b.Id == bookId);
        if(!bookExists) return NotFound("Book not found");
        var books = await _context.AuthorBooks
            .Where(ab => ab.BookId == bookId)
            .Select(ab => new ReadAuthorDTO
            {
                Id = ab.Author.Id,
                Name = ab.Author.Name
            })
            .ToListAsync();
        if(books.Count == 0) return NotFound("No authors for this book");
        return Ok(books);
    }


    [HttpPost ("add_authorbooks")]
    public async Task<IActionResult> AddAuthorBook([FromForm] CreateAuthorBooksDTO dto)
    {
        var ab = new AuthorBook
        {
            AuthorId = dto.AuthorId,
            BookId = dto.BookId
        };
        _context.Add(ab);
        await _context.SaveChangesAsync();
        return Ok(ab);
    }

    [HttpDelete ("authorbooks/{authorId}/{bookId}")]
    public async Task<IActionResult> DeleteAuthorBookRel(int authorId, int bookId)
    {
        var authorExists = await _context.Author.AnyAsync(a => a.Id == authorId);
        if(!authorExists) return NotFound("Author not found");
        var bookExists = await _context.Book.AnyAsync(b => b.Id == bookId);
        if(!bookExists) return NotFound("Book not found");
        var currAuthBook = await _context.AuthorBooks.FirstOrDefaultAsync(ab => ab.AuthorId == authorId && ab.BookId == bookId);
        if(currAuthBook == null) return NotFound("Author-book relation not found");
        _context.AuthorBooks.Remove(currAuthBook);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
}