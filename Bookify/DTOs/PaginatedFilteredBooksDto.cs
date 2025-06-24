namespace Bookify.DTOs;

public class PaginatedFilteredBooksDto
{
    public int TotalBooks { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalBooks / PageSize) : 0;
    public List<BookListItemDto> Books { get; set; }
}
