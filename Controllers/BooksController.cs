using System.Collections.Immutable;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class BookController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private Book FindBookById(int id)
    {
        var currBook = _context.Book.FirstOrDefault(m => m.Id == id) ?? throw new KeyNotFoundException("Book Not Found");
        return currBook;
    }

    // add book not found functionality
    
    [HttpGet("all_books")]
    public List<Book> GetBooks()
    {
        var books = _context.Book.ToList();
        return books;
    }

    [HttpGet("get_book_by_id")]
    public Book GetBookById(int id)
    {
        var currBook = FindBookById(id);
        return currBook;

    }

    [HttpPost ("add_book")]
    public IActionResult AddBook([FromForm] Book book)
    {
        _context.Add(book);
        _context.SaveChanges();
        return Ok(book);
    }

    [HttpPut ("edit_book")]
    public IActionResult EditBook(int id, Book book)
    {
        var currBook = FindBookById(id);
        currBook.Title = book.Title;
        currBook.ISBN = book.ISBN;
        currBook.PublicationYear = book.PublicationYear;
        currBook.Genre = book.Genre;
        currBook.Author = book.Author;
        currBook.TotalCopies = book.TotalCopies;
        currBook.AvailableCopies = book.AvailableCopies;

        _context.SaveChanges();
        return Ok(currBook);
    }

    [HttpDelete ("delete_book")]
    public IActionResult DeleteBook(int id)
    {
        var currBook = FindBookById(id);
        _context.Remove(currBook);
        _context.SaveChanges();
        return Ok();
    }
}