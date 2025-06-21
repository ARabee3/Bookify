using Bookify.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IRatingRepository
    {
        Task AddAsync(UserBookRating rating);
        Task UpdateAsync(UserBookRating rating); // EF Core يتتبع التغييرات
        Task DeleteAsync(UserBookRating rating);
        Task<UserBookRating?> GetByIdAsync(int ratingId);
        Task<UserBookRating?> GetUserRatingForBookAsync(string userId, int bookId);
        Task<IEnumerable<UserBookRating>> GetRatingsForBookAsync(int bookId, int pageNumber, int pageSize); // مع Pagination
        Task<int> GetTotalRatingsCountForBookAsync(int bookId); // لعدد التقييمات
    }
}