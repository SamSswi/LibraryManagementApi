using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace LibraryManagement.Api.Models;

public class Book
{
    public int Id {get; private set;}
    public required string Title {get; set;}
    public required string ISBN {get; set;}
    
    [Range(1454, 2050)]
    public required int PublicationYear {get; set;}
    public string Genre {get; set;} = "";
    public required string Author {get; set;}
    public required int TotalCopies {get; set;}
    public required int AvailableCopies{get; set;}
    // might have to separate book and book copy model where will be available copies
}