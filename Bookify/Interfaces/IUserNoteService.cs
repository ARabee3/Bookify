using Bookify.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Bookify.Interfaces
{
    public interface IUserNoteService
    {
        Task<NoteDto?> CreateNoteAsync(string userId, CreateNoteDto createNoteDto);
        Task<NoteDto?> GetNoteByIdAsync(string userId, int noteId); // نتأكد إن المستخدم هو صاحب النوت
        Task<IEnumerable<NoteDto>> GetMyNotesForBookAsync(string userId, int bookId);
        Task<IEnumerable<NoteDto>> GetMyNotesForChapterAsync(string userId, int chapterId);
        Task<IEnumerable<NoteDto>> GetAllMyNotesAsync(string userId); // كل ملاحظات المستخدم
        Task<bool> UpdateNoteAsync(string userId, int noteId, UpdateNoteDto updateNoteDto);
        Task<bool> DeleteNoteAsync(string userId, int noteId);
    }
}