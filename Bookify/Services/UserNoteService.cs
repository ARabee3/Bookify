using Bookify.Contexts;
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Identity; // للوصول لـ UserManager
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore; // لم تعد ضرورية هنا إذا كان Repository يتولى كل استعلامات EF

namespace Bookify.Services
{
    public class UserNoteService : IUserNoteService
    {
        private readonly IUserNoteRepository _noteRepository;
        private readonly IBookRepository _bookRepository;         // للتحقق من وجود الكتاب
        // private readonly IChapterRepository _chapterRepository; // سنحتاجها للتحقق من الشابتر وإحضار عنوانه (لو لم يتم تضمينه)
        private readonly AppDbContext _context;                  // لـ SaveChangesAsync
        private readonly UserManager<ApplicationUser> _userManager; // لجلب اسم المستخدم

        public UserNoteService(
            IUserNoteRepository noteRepository,
            IBookRepository bookRepository,
            // IChapterRepository chapterRepository, // يجب حقنها إذا تم إنشاؤها واستخدامها
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _noteRepository = noteRepository;
            _bookRepository = bookRepository;
            // _chapterRepository = chapterRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<NoteDto?> CreateNoteAsync(string userId, CreateNoteDto createNoteDto)
        {
            if (!createNoteDto.BookID.HasValue && !createNoteDto.ChapterID.HasValue)
            {
                // يمكن إلقاء ArgumentException هنا لتوضيح الخطأ للـ Controller
                // throw new ArgumentException("Note must be associated with either a BookID or a ChapterID.");
                return null; // أو التعامل معها في الـ Controller
            }

            if (createNoteDto.BookID.HasValue)
            {
                var bookExists = await _bookRepository.GetByIdWithDetailsAsync(createNoteDto.BookID.Value);
                if (bookExists == null)
                {
                    // throw new ArgumentException($"Book with ID {createNoteDto.BookID.Value} not found.");
                    return null;
                }
            }

            // TODO: إضافة تحقق مشابه لـ ChapterID إذا كان موجوداً
            // if (createNoteDto.ChapterID.HasValue)
            // {
            //     var chapterExists = await _chapterRepository.GetByIdAsync(createNoteDto.ChapterID.Value);
            //     if (chapterExists == null)
            //     {
            //         // throw new ArgumentException($"Chapter with ID {createNoteDto.ChapterID.Value} not found.");
            //         return null;
            //     }
            //     // (اختياري) التأكد أن الشابتر يتبع الكتاب لو تم توفير BookID و ChapterID معاً
            //     if (createNoteDto.BookID.HasValue && chapterExists.BookID != createNoteDto.BookID.Value)
            //     {
            //          // throw new ArgumentException("Chapter does not belong to the specified Book.");
            //          return null;
            //     }
            // }


            var user = await _userManager.FindByIdAsync(userId); // لجلب بيانات المستخدم للـ DTO
            if (user == null)
            {
                // هذا لا يجب أن يحدث إذا كان userId صالحاً من Token
                // throw new InvalidOperationException("User not found for the provided userId.");
                return null;
            }

            var note = new UserNote
            {
                UserID = userId,
                BookID = createNoteDto.BookID,
                ChapterID = createNoteDto.ChapterID,
                Content = createNoteDto.Content,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _noteRepository.AddAsync(note);
            await _context.SaveChangesAsync(); // حفظ النوت الجديدة للحصول على NoteID

            // --- تم تعديل هذا الجزء لجلب النوت كاملة بعد الحفظ ---
            var createdNoteWithDetails = await _noteRepository.GetByIdAsync(note.NoteID);
            if (createdNoteWithDetails == null)
            {
                // حالة نادرة: النوت لم يتم حفظها أو جلبها بشكل صحيح
                // Log an error here
                Console.WriteLine($"Critical error: Note with ID {note.NoteID} was not found after saving.");
                return null;
            }

            return new NoteDto
            {
                NoteID = createdNoteWithDetails.NoteID,
                Username = createdNoteWithDetails.User?.UserName ?? "N/A",
                BookID = createdNoteWithDetails.BookID,
                BookTitle = createdNoteWithDetails.Book?.Title,
                ChapterID = createdNoteWithDetails.ChapterID,
                ChapterTitle = createdNoteWithDetails.Chapter?.Title,
                Content = createdNoteWithDetails.Content,
                CreatedAt = createdNoteWithDetails.CreatedAt,
                LastModifiedAt = createdNoteWithDetails.LastModifiedAt
            };
            // --- نهاية التعديل ---
        }

        public async Task<NoteDto?> GetNoteByIdAsync(string userIdOfCurrentUser, int noteId)
        {
            var noteEntity = await _noteRepository.GetByIdAsync(noteId);

            if (noteEntity == null || noteEntity.UserID != userIdOfCurrentUser)
            {
                return null;
            }

            return new NoteDto
            {
                NoteID = noteEntity.NoteID,
                Username = noteEntity.User?.UserName ?? "N/A",
                BookID = noteEntity.BookID,
                BookTitle = noteEntity.Book?.Title,
                ChapterID = noteEntity.ChapterID,
                ChapterTitle = noteEntity.Chapter?.Title,
                Content = noteEntity.Content,
                CreatedAt = noteEntity.CreatedAt,
                LastModifiedAt = noteEntity.LastModifiedAt
            };
        }

        public async Task<IEnumerable<NoteDto>> GetMyNotesForBookAsync(string userId, int bookId)
        {
            var notes = await _noteRepository.GetNotesForBookAsync(userId, bookId);
            // الـ Repository المفروض يعمل Include للـ User, Book, Chapter
            return notes.Select(note => new NoteDto
            {
                NoteID = note.NoteID,
                Username = note.User?.UserName ?? "N/A", // يفترض أن User متضمن
                BookID = note.BookID,
                BookTitle = note.Book?.Title, // يفترض أن Book متضمن
                ChapterID = note.ChapterID,
                ChapterTitle = note.Chapter?.Title, // يفترض أن Chapter متضمن
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                LastModifiedAt = note.LastModifiedAt
            }).ToList();
        }

        public async Task<IEnumerable<NoteDto>> GetMyNotesForChapterAsync(string userId, int chapterId)
        {
            var notes = await _noteRepository.GetNotesForChapterAsync(userId, chapterId);
            return notes.Select(note => new NoteDto
            {
                NoteID = note.NoteID,
                Username = note.User?.UserName ?? "N/A",
                BookID = note.BookID,
                BookTitle = note.Book?.Title,
                ChapterID = note.ChapterID,
                ChapterTitle = note.Chapter?.Title, // الـ Repository المفروض يكون عمل Include لـ Chapter
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                LastModifiedAt = note.LastModifiedAt
            }).ToList();
        }

        public async Task<IEnumerable<NoteDto>> GetAllMyNotesAsync(string userId)
        {
            var notes = await _noteRepository.GetNotesForUserAsync(userId);
            return notes.Select(note => new NoteDto
            {
                NoteID = note.NoteID,
                Username = note.User?.UserName ?? "N/A",
                BookID = note.BookID,
                BookTitle = note.Book?.Title,
                ChapterID = note.ChapterID,
                ChapterTitle = note.Chapter?.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                LastModifiedAt = note.LastModifiedAt
            }).ToList();
        }

        public async Task<bool> UpdateNoteAsync(string userId, int noteId, UpdateNoteDto updateNoteDto)
        {
            var noteToUpdate = await _noteRepository.GetByIdAsync(noteId);
            if (noteToUpdate == null || noteToUpdate.UserID != userId)
            {
                return false;
            }

            noteToUpdate.Content = updateNoteDto.Content;
            noteToUpdate.LastModifiedAt = DateTime.UtcNow;

            // _noteRepository.Update(noteToUpdate); // EF Core يتتبع التغييرات، لا حاجة لاستدعاء Update صريح إذا كان الـ Entity متتبع
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteNoteAsync(string userId, int noteId)
        {
            var noteToDelete = await _noteRepository.GetByIdAsync(noteId);
            if (noteToDelete == null || noteToDelete.UserID != userId)
            {
                return false;
            }

            _noteRepository.Delete(noteToDelete); // يستدعي _context.UserNotes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}