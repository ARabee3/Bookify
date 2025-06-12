namespace Bookify.DTOs;

public class UpdateProfileDto
{
    // المستخدم ممكن يغير دول مثلاً
    public int? Age { get; set; } // خليه Nullable عشان لو مش عايز يغيره
    public string? Specialization { get; set; }
    public string? Level { get; set; }
    public string? Interest { get; set; }
    // مش هنسمح بتغيير الإيميل أو الـ Username من هنا (دي عمليات أعقد)
}
