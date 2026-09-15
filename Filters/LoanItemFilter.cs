using System.Text.Json.Serialization;

namespace LibraryManagement.Api.Filter;

public class LoanItemFilter : BaseFilter
{
    public string? BookTitle {get; set;}
    
    public LoanItemCategory? Category {get; set;}
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoanItemCategory
{
    PastLoans,
    Overdue,
    Current,
    All
}