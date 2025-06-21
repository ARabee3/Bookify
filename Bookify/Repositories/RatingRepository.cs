using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _context;

        public RatingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserBookRating rating)
        {
            await _context.UserBookRatings.AddAsync(rating);
            // SaveChangesAsync will be called in the Service layer or Unit of Work
        }

        public async Task UpdateAsync(UserBookRating rating)
        {
            // EF Core tracks changes to entities fetched from the context.
            // If 'rating' was fetched and then modified, SaveChangesAsync in the service will persist it.
            // If it's a detached entity you want to update, you'd use:
            _context.UserBookRatings.Update(rating); // Marks the entity as modified
            await Task.CompletedTask; // Update itself doesn't need await if just marking state
        }

        public async Task DeleteAsync(UserBookRating rating)
        {
            _context.UserBookRatings.Remove(rating);
            await Task.CompletedTask; // Remove itself doesn't need await
        }

        public async Task<UserBookRating?> GetByIdAsync(int ratingId)
        {
            return await _context.UserBookRatings
                                 .Include(r => r.User) // To get Username for DTO
                                 .Include(r => r.Book) // To get BookTitle for DTO
                                 .FirstOrDefaultAsync(r => r.RatingID == ratingId);
        }

        public async Task<UserBookRating?> GetUserRatingForBookAsync(string userId, int bookId)
        {
            return await _context.UserBookRatings
                                 .FirstOrDefaultAsync(r => r.UserID == userId && r.BookID == bookId);
        }

        public async Task<IEnumerable<UserBookRating>> GetRatingsForBookAsync(int bookId, int pageNumber, int pageSize)
        {
            return await _context.UserBookRatings
                                 .Where(r => r.BookID == bookId)
                                 .Include(r => r.User) // To get Username
                                 .OrderByDescending(r => r.RatedAt) // الأحدث أولاً
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();
        }

        public async Task<int> GetTotalRatingsCountForBookAsync(int bookId)
        {
            return await _context.UserBookRatings.CountAsync(r => r.BookID == bookId);
        }
    }
}