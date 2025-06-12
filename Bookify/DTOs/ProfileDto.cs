namespace Bookify.DTOs;

public class ProfileDto
{
    public string Id { get; set; } // هنرجع الـ ID بتاع Identity
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public string? Specialization { get; set; }
    public string? Level { get; set; }
    public string? Interest { get; set; }
    // ممكن تضيف أي بيانات تانية عايز تعرضها
}
