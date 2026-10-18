using LibraryManagement.Api.Data;
using LibraryManagement.Api.GlobalExceptionHandler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Tests;

public class ReturnServiceTests
{
    private readonly AppDbContext _context;
    public ReturnServiceTests() {
        _context = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task ReturnLoanItem_UserHasLoan_LoanItemReturned()
    {
        var user = TestDataFactory.CreateTestUser();
        _context.Add(user);
        var book = TestDataFactory.CreateTestBook();
        _context.Add(book);
        await _context.SaveChangesAsync();
        var loan = TestDataFactory.CreateTestLoan(user.Id, book.Id);
        _context.Loan.Add(loan);
        book.AvailableCopies --;
        await _context.SaveChangesAsync();


        var loanItemsId = await _context.LoanItem.Select(li => li.Id).ToListAsync();

        var controller = TestDataFactory.CreateAuthenticatedReturnController(_context, user.Id);

        foreach(var li in loanItemsId)
        {
            var result = await controller.ReturnBook(li);
            Assert.IsType<OkResult>(result);
        }

        foreach(var li in _context.LoanItem)
        {
            bool status = li.ReturnedAt != null;
            Assert.True(status);
        }
    }

    [Fact]
    public async Task ReturnLoanItem_UserNeverBorrowedTheBook_Reject()
    {
        var user = TestDataFactory.CreateTestUser();
        _context.Add(user);
        await _context.SaveChangesAsync();

        var controller = TestDataFactory.CreateAuthenticatedReturnController(_context, user.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => controller.ReturnBook(1));
    }

    [Fact]
    public async Task ReturnLoanItem_BookBorrowedByDifferentUser_Reject()
    {
        var user1 = TestDataFactory.CreateTestUser();
        _context.Add(user1);
        var user2 = TestDataFactory.CreateTestUser();
        _context.Add(user2);
        var book = TestDataFactory.CreateTestBook();
        _context.Add(book);
        await _context.SaveChangesAsync();

        var loan = TestDataFactory.CreateTestLoan(user1.Id, book.Id);
        _context.Add(loan);
        await _context.SaveChangesAsync();

        var controller = TestDataFactory.CreateAuthenticatedReturnController(_context, user2.Id);

        var loanItemId = _context.Loan.SelectMany(l => l.LoanItems).FirstOrDefault()?.Id;

        if(loanItemId == null) Assert.Fail("Test Failed: no loan item found");
        
        await Assert.ThrowsAsync<ConflictException>(() => controller.ReturnBook(loanItemId.Value));
    }
}