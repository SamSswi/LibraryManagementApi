using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTO;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Filter;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;
namespace LibraryManagement.Api.Controllers;

[ApiController] // user can view their own loans
[Route("api/[controller]")]

public class LoanController(AppDbContext context) : BaseController (context)
{

    private async Task CheckBookAvailability(int bookId)
    {
        var book = await _context.Book.FirstOrDefaultAsync(b => b.Id == bookId) ?? throw new KeyNotFoundException("Book not found");
        if(book.AvailableCopies == 0) throw new InvalidOperationException($"The book {book.Title} is not available for loan");

    }
    
    [Authorize (Roles = "Admin")]
    [HttpGet ("all_loans")]
    public async Task<ActionResult<IEnumerable<ReadLoanDTO>>> GetAllLoans([FromQuery] LoanFilter filter)
    {
        var query = _context.Loan.AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(u => u.UserId == filter.UserId.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            query = query.Where(u => u.User != null && u.User.Name.ToLower().Contains(filter.UserName.ToLower()));
        }
        if (filter.BorrowedAt.HasValue)
        {
            query = query.Where(b => b.BorrowedAt == filter.BorrowedAt.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.BookTitle))
        {
            query = query.Where(bT => bT.LoanItems.Any(item => item.Book != null && 
                        item.Book.Title.ToLower().Contains(filter.BookTitle.ToLower())));
        }


        var loan = query.Select(l => new ReadLoanDTO
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.User.Name,
            BorrowedAt = l.BorrowedAt,
            LoanItems = l.LoanItems.Select(li => new ReadLoanItemDTO
            {
                Id = li.Id,
                DueDate = li.DueDate,
                ReturnedAt = li.ReturnedAt,
                BookId = li.BookId,
                BookTitle = li.Book.Title
            }).ToList()
        });

        var totalLoans = await loan.CountAsync();

        Dictionary<string, int> normData = NormalizePageData(filter.Page, filter.PageSize, totalLoans);

        var loansPerPage = await loan
            .OrderBy(u => u.Id)
            .Skip((normData["page"] - 1) * normData["pageSize"])
            .Take(normData["pageSize"])
            .ToListAsync();
            
        return Ok(loansPerPage);        
    }


    [Authorize]
    [HttpGet ("own_loans")]
    public async Task<ActionResult<IEnumerable<ReadLoanDTO>>> GetOwnLoans([FromQuery] LoanItemFilter filter)
    {
        var query = _context.Loan.AsQueryable();
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (!string.IsNullOrWhiteSpace(filter.BookTitle))
        {
            query = query.Where(l => l.LoanItems.Any(b => b.Book != null && b.Book.Title.ToLower().Contains(filter.BookTitle.ToLower())));
        }
        if(filter.Category == LoanItemCategory.All)
        {
            query = query.Where(l => l.LoanItems.Any());
        }
        
        if(filter.Category == LoanItemCategory.PastLoans)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt != null));
        }
        if(filter.Category == LoanItemCategory.Current)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt == null && today <= li.DueDate));
        }
        if(filter.Category == LoanItemCategory.Overdue)
        {
            query = query.Where(l => l.LoanItems.Any(li => li.ReturnedAt == null && today > li.DueDate));
        }

        int uId = ExtractId();

        var loans = query.Where(l => l.UserId == uId).Select(l => new ReadLoanDTO
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.User.Name,
            BorrowedAt = l.BorrowedAt,
            LoanItems = l.LoanItems.Select(li => new ReadLoanItemDTO
            {
                Id = li.Id,
                Status =  li.ReturnedAt != null ? "Returned" 
                    : li.DueDate > today ? $"Current Loan. Return Deadline: {li.DueDate}" 
                    : "Overdue",
                DueDate = li.DueDate,
                BookId = li.BookId,
                BookTitle = li.Book.Title
            }).ToList()
        });
            
        return Ok(loans);        
    }



    [Authorize (Roles = "Admin")]  
    [HttpGet("get-loan-by-id")]
    public async Task<ActionResult<ReadLoanDTO>> GetLoanById(int id)
    {
        var currLoan = await FindLoanById(id);
        var loan = new ReadLoanDTO
        {
          Id = currLoan.Id,
          UserId = currLoan.UserId,
          UserName = currLoan.User.Name,
          BorrowedAt = currLoan.BorrowedAt,
          LoanItems = [.. currLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title
          })]
        };
        return loan;
    }

    [Authorize (Roles = "Admin")]
    [HttpPost ("add-loan")]
    public async Task<IActionResult> AddLoan([FromBody] CreateLoanDTO dto)
    {
        var loan = new Loan
        {
            UserId = dto.UserId,
            BorrowedAt = DateOnly.FromDateTime(DateTime.Today)
        };
        foreach(var item in dto.LoanItems)
        {
            loan.LoanItems.Add(new LoanItem
            {
                BookId = item.BookId,
                DueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14)
            });
            await CheckBookAvailability(item.BookId);
            await AvailableCopiesCalc(item.BookId, Operands.Minus);
        }
        _context.Loan.Add(loan);
        await _context.SaveChangesAsync();
        var createdLoan = await FindLoanById(loan.Id);
        return Ok(new ReadLoanResponseDTO
        {
            Id = createdLoan.Id,
            UserId = createdLoan.UserId,
            UserName = createdLoan.User.Name,
            BorrowedAt = createdLoan.BorrowedAt,
            LoanItems = [.. createdLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title
          })]
        });
    }

    [Authorize]
    [HttpPost ("borrow")]
    public async Task<IActionResult> Borrow([FromBody] BorrowDTO dto)
    {
        int id = ExtractId();
        var loan = new Loan
        {
            UserId = id,
            BorrowedAt = DateOnly.FromDateTime(DateTime.Today)
        };
        foreach(var item in dto.LoanItems)
        {
            loan.LoanItems.Add(new LoanItem
            {
                BookId = item.BookId,
                DueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14)
            });
            await CheckBookAvailability(item.BookId);
            await AvailableCopiesCalc(item.BookId, Operands.Minus);
        }
        _context.Loan.Add(loan);
        await _context.SaveChangesAsync();
        var createdLoan = await FindLoanById(loan.Id);
        return Ok(new ReadLoanResponseDTO
        {
            Id = createdLoan.Id,
            UserId = createdLoan.UserId,
            UserName = createdLoan.User.Name,
            BorrowedAt = createdLoan.BorrowedAt,
            LoanItems = [.. createdLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title
          })]
        });
    }


    
    [Authorize (Roles = "Admin")]
    [HttpPut ("edit-loan")]
    public async Task<IActionResult> EditLoan(int id, EditLoanDTO dto)
    {
        var loan = await FindLoanById(id);
        foreach(var item in loan.LoanItems)
        {
            await AvailableCopiesCalc(item.BookId, Operands.Plus);
        }
        loan.UserId = dto.UserId;
        foreach(var itemDTO in dto.LoanItems)
        {
            var item = loan.LoanItems.First(li => li.Id == itemDTO.Id);
            item.DueDate = itemDTO.DueDate;
            item.ReturnedAt = itemDTO.ReturnedAt;
            item.BookId = itemDTO.BookId;
        }
        foreach(var item in loan.LoanItems)
        {
            await AvailableCopiesCalc(item.BookId, Operands.Minus);
        }
        await _context.SaveChangesAsync();
        var updatedLoan = await FindLoanById(loan.Id);
        return Ok(new ReadLoanResponseDTO
        {
            Id = updatedLoan.Id,
            UserId = updatedLoan.UserId,
            UserName = updatedLoan.User.Name,
            BorrowedAt = updatedLoan.BorrowedAt,
            LoanItems = [.. updatedLoan.LoanItems.Select(li => new ReadLoanItemDTO{
              Id = li.Id,
              DueDate = li.DueDate,
              ReturnedAt = li.ReturnedAt,
              BookId = li.BookId,
              BookTitle = li.Book.Title
          })]
        });
    }
    
    [Authorize (Roles = "Admin")]
    [HttpDelete ("delete-loan")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var loan = await FindLoanById(id);
        foreach(var item in loan.LoanItems)
        {
            await AvailableCopiesCalc(item.BookId, Operands.Plus);
        }
        _context.Remove(loan);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // [HttpPost ("seed-loans")]
    // public async Task<IActionResult> SeedLoans()
    // {
    //     int userId = 2;
    //     int bookId = 12;
    //     for(int i = 0; i < 100; i++)
    //     {
    //         _context.Loan.Add(new Loan
    //         {
    //             UserId = userId,
    //             LoanItems = [new LoanItem {
    //                 BookId = bookId
    //             }]

    //         });
    //         userId ++;
    //         bookId ++;
    //     }
    //     await _context.SaveChangesAsync();
    //     return Ok();
    // }
    // [HttpDelete ("delete-loans-many")]
    // public IActionResult DeleteLoans()
    // {

    //     for(int i = 1; i < 101; i++)
    //     {
    //         _context.Remove(FindLoanById(i));
    //     }
    //     _context.SaveChanges();
    //     return Ok();
    // }
    

}