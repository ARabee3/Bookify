using Bookify.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookify.Interfaces
{
    public interface IRatingService
    {
        Task<RatingDto?> AddRatingAsync(string userId, int bookId, AddRatingDto addRatingDto);
        Task<RatingDto?> UpdateRatingAsync(string userId, int ratingId, UpdateRatingDto updateRatingDto);
        Task<bool> DeleteRatingAsync(string userId, int ratingId); // يرجع true لو اتحذف
        Task<IEnumerable<RatingDto>> GetRatingsForBookAsync(int bookId, int pageNumber, int pageSize);
    }
}