using Bookify.Contexts; // For SaveChangesAsync
using Bookify.DTOs;
using Bookify.Entities;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Identity; // For UserManager
using Microsoft.EntityFrameworkCore; // For AnyAsync
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IBookRepository _bookRepository; // To check if book exists
        private readonly UserManager<ApplicationUser> _userManager; // To get user details for DTO
        private readonly AppDbContext _context; // For SaveChangesAsync (Unit of Work principle)

        public RatingService(
            IRatingRepository ratingRepository,
            IBookRepository bookRepository,
            UserManager<ApplicationUser> userManager,
            AppDbContext context)
        {
            _ratingRepository = ratingRepository;
            _bookRepository = bookRepository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<RatingDto?> AddRatingAsync(string userId, int bookId, AddRatingDto addRatingDto)
        {
            var book = await _bookRepository.GetByIdWithDetailsAsync(bookId);
            if (book == null)
            {
                // Or throw custom exception e.g., NotFoundException
                return null; // Book not found
            }

            var existingRating = await _ratingRepository.GetUserRatingForBookAsync(userId, bookId);
            if (existingRating != null)
            {
                // User has already rated this book, maybe allow update instead?
                // For now, let's prevent adding a new one.
                // Or throw custom exception e.g., ConflictException
                return null; // Or a DTO indicating conflict
            }

            var user = await _userManager.FindByIdAsync(userId); // Get user for username
            if (user == null)
            {
                return null; // Should not happen if userId is from token
            }

            var newRating = new UserBookRating
            {
                UserID = userId,
                BookID = bookId,
                Rating = addRatingDto.RatingValue,
                Review = addRatingDto.ReviewText,
                RatedAt = DateTime.UtcNow
            };

            await _ratingRepository.AddAsync(newRating);
            await _context.SaveChangesAsync(); // Save changes here

            // Map to DTO
            return new RatingDto
            {
                RatingID = newRating.RatingID,
                Username = user.UserName ?? "Unknown User", // Use UserName
                BookID = newRating.BookID,
                BookTitle = book.Title ?? "Unknown Book", // Use Book Title
                RatingValue = newRating.Rating,
                ReviewText = newRating.Review,
                RatedAt = newRating.RatedAt
            };
        }

        public async Task<RatingDto?> UpdateRatingAsync(string userId, int ratingId, UpdateRatingDto updateRatingDto)
        {
            var ratingToUpdate = await _ratingRepository.GetByIdAsync(ratingId);

            if (ratingToUpdate == null)
            {
                return null; // Rating not found
            }

            if (ratingToUpdate.UserID != userId)
            {
                // User is not the owner of the rating - Forbidden
                // Or throw custom exception e.g., ForbiddenException
                return null; // Or a DTO indicating forbidden
            }

            ratingToUpdate.Rating = updateRatingDto.RatingValue;
            ratingToUpdate.Review = updateRatingDto.ReviewText;
            ratingToUpdate.RatedAt = DateTime.UtcNow; // Update the rated at timestamp

            // _ratingRepository.UpdateAsync(ratingToUpdate); // EF Core tracks it, so this might not be needed
            await _context.SaveChangesAsync();

            // Fetch user and book details again for the DTO, or ensure they are loaded with GetByIdAsync
            var user = await _userManager.FindByIdAsync(ratingToUpdate.UserID);
            // var book = await _bookRepository.GetByIdAsync(ratingToUpdate.BookID); // Could be simplified if GetByIdAsync in repo includes Book

            return new RatingDto
            {
                RatingID = ratingToUpdate.RatingID,
                Username = user?.UserName ?? ratingToUpdate.User?.UserName ?? "Unknown User",
                BookID = ratingToUpdate.BookID,
                BookTitle = ratingToUpdate.Book?.Title ?? "Unknown Book",
                RatingValue = ratingToUpdate.Rating,
                ReviewText = ratingToUpdate.Review,
                RatedAt = ratingToUpdate.RatedAt
            };
        }

        public async Task<bool> DeleteRatingAsync(string userId, int ratingId)
        {
            var ratingToDelete = await _ratingRepository.GetByIdAsync(ratingId);

            if (ratingToDelete == null)
            {
                return false; // Rating not found
            }

            if (ratingToDelete.UserID != userId)
            {
                // User is not the owner -
                return false; // Or throw ForbiddenException
            }

            await _ratingRepository.DeleteAsync(ratingToDelete);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RatingDto>> GetRatingsForBookAsync(int bookId, int pageNumber, int pageSize)
        {
            var book = await _bookRepository.GetByIdWithDetailsAsync(bookId);
            if (book == null)
            {
                return new List<RatingDto>(); // Book not found, return empty list
            }

            var ratings = await _ratingRepository.GetRatingsForBookAsync(bookId, pageNumber, pageSize);

            // Map to DTO
            return ratings.Select(r => new RatingDto
            {
                RatingID = r.RatingID,
                Username = r.User?.UserName ?? "Unknown User", // User should be included by repository
                BookID = r.BookID,
                BookTitle = book.Title ?? "Unknown Book", // We have the book title
                RatingValue = r.Rating,
                ReviewText = r.Review,
                RatedAt = r.RatedAt
            });
        }
    }
}