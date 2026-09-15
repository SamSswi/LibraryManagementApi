using LibraryManagement.Api.Data;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Api.Controllers;

public abstract class BaseController(AppDbContext context) : ControllerBase
{
    protected readonly AppDbContext _context = context;
    protected Dictionary<string, int>  NormalizePageData(int page, int pageSize, int totalInstances)
    {
        if(pageSize < 1) pageSize = 1;
        var totalPages = (int) Math.Ceiling((decimal) totalInstances / pageSize);
        if(pageSize > totalInstances) pageSize = totalInstances;
        if(page < 1) page = 1;
        else if(page > totalPages) page = totalPages;
        
        var pageData = new Dictionary<string, int> 
        {
            {"page", page},
            {"pageSize", pageSize}
        };
        return pageData;
    }

    protected enum Operands
    {
        Plus,
        Minus
    }

    protected async Task AvailableCopiesCalc (int bookId, Operands operand)
    {
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == bookId) ?? throw new KeyNotFoundException("Book not found");
        if(operand == Operands.Plus && book.AvailableCopies < book.TotalCopies) book.AvailableCopies ++;
        else if(operand == Operands.Plus && book.AvailableCopies >= book.TotalCopies) throw new InvalidOperationException("Available copies cannot exceed total copies");
        else if(operand == Operands.Minus && book.AvailableCopies > 0) book.AvailableCopies --;
        else if(operand == Operands.Minus && book.AvailableCopies <= 0) throw new InvalidOperationException("Available copies cannot be fewer than zero");
        else throw new InvalidOperationException("Attempted invalid operation with available copies");
    }

    protected int ExtractId()
    {
        var uId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(!int.TryParse(uId, out var userId))
            throw new UnauthorizedAccessException("Token contains an invalid user Id");
        return userId;
    }

    protected async Task<Loan> FindLoanById(int id)
    {
        var loan = await _context.Loan
            .Include(l => l.User)
            .Include(l => l.LoanItems)
                .ThenInclude(li => li.Book)
            .FirstOrDefaultAsync(m => m.Id == id) 
            ?? throw new KeyNotFoundException("Loan Not Found");
        return loan;
    }
}
