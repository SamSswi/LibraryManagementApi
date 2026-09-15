using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController] // admin only
[Route("api/[controller]")]

public class ReturnEndpoint(AppDbContext context) : BaseController(context)
{

    [Authorize (Roles = "Admin")]
    [HttpPut ("return/loan-item-{id}")]
    public async Task<ActionResult> ReturnItem(int id)
    {
        var loanItem = await _context.LoanItem.FirstOrDefaultAsync(li => li.Id == id)
            ?? throw new KeyNotFoundException("LoanItem not found");

        if(loanItem.ReturnedAt != null) return Conflict($"loan already returned on {loanItem.ReturnedAt}");
        loanItem.ReturnedAt = DateOnly.FromDateTime(DateTime.Today);
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == loanItem.BookId) ?? throw new KeyNotFoundException("Book not Found");
        await AvailableCopiesCalc(book.Id, Operands.Plus);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpPut ("return")]
    public async Task<ActionResult> ReturnBook(int id)
    {
        int userId = ExtractId();
        var loanItem = await _context.LoanItem.FirstOrDefaultAsync(li => li.Id == id)
            ?? throw new KeyNotFoundException("LoanItem not found");
        var loan = await FindLoanById(loanItem.LoanId);
        if(userId != loan.UserId) return Conflict($"The book is not loaned to this user");
        if(loanItem.ReturnedAt != null) return Conflict($"loan already returned on {loanItem.ReturnedAt}");
        loanItem.ReturnedAt = DateOnly.FromDateTime(DateTime.Today);
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == loanItem.BookId) ?? throw new KeyNotFoundException("Book not Found");
        await AvailableCopiesCalc(book.Id, Operands.Plus);
        await _context.SaveChangesAsync();
        return Ok();
    }

}