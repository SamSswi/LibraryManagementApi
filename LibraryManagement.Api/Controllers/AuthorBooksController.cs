using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.GlobalExceptionHandler;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")] //only view for user

public class AuthorBookController(AppDbContext context) : BaseController(context)
{
    [Authorize(Roles = "Admin")]
    [HttpGet ("author-books")]
    public async Task<ActionResult<IEnumerable<AuthorBookReadDTO>>> AllAuthorBooks(int page = 1, int pageSize = 10)
    {
        var query = _context.AuthorBooks.Select(ab => new AuthorBookReadDTO
        {
            AuthorId = ab.AuthorId,
            AuthorName = ab.Author.Name,
            BookId = ab.BookId,
            BookTitle = ab.Book.Title,
        });
        var totalAuthorBooks = await query.CountAsync();

        
        Dictionary<string, int> normData = NormalizePageData(page, pageSize, totalAuthorBooks);

        var abPerPage = await query
            .OrderBy(ab => ab.AuthorId)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();

        return Ok(abPerPage);
    }
    
    [Authorize (Roles = "Admin")]
    [HttpGet ("authors/{authorId}/books")]
    // for some reason there is a second irrelevant id in swagger i think it's from calling on readbooksdto
    public async Task<ActionResult<List<ReadBooksDTO>>> FindBooksByAnAuthor(int authorId)
    {
        await FindAuthorById(authorId);
        var books = await _context.AuthorBooks
            .Where(ab => ab.AuthorId == authorId)
            .Select(ab => new ReadBooksDTO
            {
                Id = ab.Book.Id,
                Title = ab.Book.Title,
                Author = new List<string> {ab.Author.Name},
                PublicationYear = ab.Book.PublicationYear,
                Genre = ab.Book.Genre,
                TotalCopies = ab.Book.TotalCopies,
                AvailableCopies = ab.Book.AvailableCopies                
            }) 
            .ToListAsync();
        return Ok(books);
    }

    [Authorize (Roles = "Admin")]
    [HttpGet ("books/{bookId}/authors")]
    // for some reason there is a second irrelevant id in swagger i think it's from calling on readbooksdto
    public async Task<ActionResult<List<ReadAuthorDTO>>> FindAuthorsOfABook(int bookId)
    {
        await FindBookById(bookId);
        var books = await _context.AuthorBooks
            .Where(ab => ab.BookId == bookId)
            .Select(ab => new ReadAuthorDTO
            {
                Id = ab.Author.Id,
                Name = ab.Author.Name,
                Books = new List<string> {ab.Book.Title}
            })
            .ToListAsync();
        return Ok(books);
    }

    private async Task CheckRelationship(int authorId, int bookId)
    {
        var checkRel = await _context.AuthorBooks.FirstOrDefaultAsync(ab => ab.AuthorId == authorId && ab.BookId == bookId);
        if(checkRel != null) throw new ConflictException("The Author Book relationship exists or the book is duplicated in the memory");
    }

    [Authorize (Roles = "Admin")]
    [HttpPost ("author-books")]
    public async Task<IActionResult> AddAuthorBook([FromForm] CreateAuthorBooksDTO dto)
    {
        await FindAuthorById(dto.AuthorId);
        var book = await FindBookById(dto.BookId);
        await CheckRelationship(dto.AuthorId, dto.BookId);
        // workaround for not having ISBN code, since multiple books can have identical titles
        var booksSameTitle = await _context.Book.Where(b => b.Title.ToLower() == book.Title.ToLower()).ToListAsync();
        foreach(var bk in booksSameTitle)
        {
            await CheckRelationship(dto.AuthorId, bk.Id);
        }
        var ab = new AuthorBook
        {
            AuthorId = dto.AuthorId,
            BookId = dto.BookId
        };
        _context.Add(ab);
        await _context.SaveChangesAsync();
        return Ok(new AuthorBookReadResponseDTO
        {
            AuthorId = ab.AuthorId,
            BookId = ab.BookId
        });
    }

    [Authorize (Roles = "Admin")]
    [HttpDelete ("author/{authorId}/book/{bookId}")]
    public async Task<IActionResult> DeleteAuthorBookRel(int authorId, int bookId)
    {
        await FindAuthorById(authorId);
        await FindBookById(bookId);
        var currAuthBook = await _context.AuthorBooks.FirstOrDefaultAsync(ab => ab.AuthorId == authorId && ab.BookId == bookId) 
            ?? throw new NotFoundException("Author - Book Relation not found");
        _context.AuthorBooks.Remove(currAuthBook);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // [HttpPost ("seed-authorBook")]
    // public async Task<IActionResult> SeedAuthorBooks()
    // {
    //     int bookId = 12;
    //     int authorId = 13;
    //     for(int i = 0; i < 100; i++)
    //     {
    //         _context.AuthorBooks.Add(new AuthorBook
    //         {
    //             AuthorId = authorId,
    //             BookId = bookId
    //         });
    //         authorId++;
    //         bookId++;
    //     }
    //     await _context.SaveChangesAsync();
    //     return Ok();
    // }
    
}