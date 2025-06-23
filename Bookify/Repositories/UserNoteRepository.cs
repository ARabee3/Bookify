using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories
{
    public class UserNoteRepository : IUserNoteRepository
    {
        private readonly AppDbContext _context;

        public UserNoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserNote note)
        {
            await _context.UserNotes.AddAsync(note);
        }

        public async Task<UserNote?> GetByIdAsync(int noteId)
        {
            // نرجع النوت مع معلومات المستخدم والكتاب والشابتر عشان الـ Mapping للـ DTO
            return await _context.UserNotes
                                 .Include(n => n.User)
                                 .Include(n => n.Book)
                                 .Include(n => n.Chapter)
                                 .FirstOrDefaultAsync(n => n.NoteID == noteId);
        }

        public async Task<IEnumerable<UserNote>> GetNotesForUserAsync(string userId)
        {
            return await _context.UserNotes
                                 .Where(n => n.UserID == userId)
                                 .Include(n => n.Book)
                                 .Include(n => n.Chapter)
                                 .OrderByDescending(n => n.LastModifiedAt)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<UserNote>> GetNotesForBookAsync(string userId, int bookId)
        {
            return await _context.UserNotes
                                 .Where(n => n.UserID == userId && n.BookID == bookId)
                                 .Include(n => n.Chapter) // لو النوت مرتبطة بشابتر جوه الكتاب ده
                                 .OrderByDescending(n => n.LastModifiedAt)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<UserNote>> GetNotesForChapterAsync(string userId, int chapterId)
        {
            return await _context.UserNotes
                                 .Where(n => n.UserID == userId && n.ChapterID == chapterId)
                                 .Include(n => n.Book) // عشان نعرض اسم الكتاب لو النوت على شابتر
                                 .OrderByDescending(n => n.LastModifiedAt)
                                 .ToListAsync();
        }

        public void Update(UserNote note)
        {
            // EF Core يتتبع التغييرات، SaveChangesAsync في الـ Service هتحفظ
            _context.UserNotes.Update(note);
        }

        public void Delete(UserNote note)
        {
            _context.UserNotes.Remove(note);
        }
    }
}