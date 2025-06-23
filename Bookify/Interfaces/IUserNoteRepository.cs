using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Bookify.Interfaces
{
    public interface IUserNoteRepository
    {
        Task AddAsync(UserNote note);
        Task<UserNote?> GetByIdAsync(int noteId);
        Task<IEnumerable<UserNote>> GetNotesForUserAsync(string userId);
        Task<IEnumerable<UserNote>> GetNotesForBookAsync(string userId, int bookId);
        Task<IEnumerable<UserNote>> GetNotesForChapterAsync(string userId, int chapterId);
        void Update(UserNote note); // EF يتتبع التغييرات
        void Delete(UserNote note);
    }
}