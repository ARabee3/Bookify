using Bookify.DTOs;
using Bookify.Entities; // <<< لإضافة ApplicationUser (عشان UserManager)
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // <<< لإضافة UserManager
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic; // لإضافة List
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // كل الـ Endpoints هنا مؤمنة
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IProgressService _progressService;
        private readonly UserManager<ApplicationUser> _userManager; // <<< تم إضافته

        public ProfileController(
            IProfileService profileService,
            IProgressService progressService,
            UserManager<ApplicationUser> userManager) // <<< تم إضافته
        {
            _profileService = profileService;
            _progressService = progressService;
            _userManager = userManager; // <<< تم إضافته
        }

        // GET /api/profile/me (لجلب بيانات صفحة البروفايل المجمعة)
        // داخل ProfileController.GetMyProfilePage
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfilePage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(new { Message = "User ID could not be determined from token." });

            try
            {
                // --- نفذهم بالتتابع (Sequentially) ---
                var userProfile = await _profileService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return NotFound(new { Message = "User profile could not be retrieved." });
                }

                var readingStats = await _profileService.GetUserReadingStatsAsync(userId);
                // if (readingStats == null) readingStats = new UserReadingStatsDto(); // قيمة افتراضية لو ممكن يرجع null

                var currentlyReadingBooks = await _progressService.GetCurrentlyReadingBooksAsync(userId);
                // --------------------------------------

                var profilePageData = new ProfilePageDto
                {
                    UserProfile = userProfile,
                    ReadingStats = readingStats ?? new UserReadingStatsDto(), // قيمة افتراضية
                    CurrentlyReadingBooks = currentlyReadingBooks ?? new List<BookProgressDto>()
                };

                return Ok(profilePageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting profile page for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while retrieving your profile." });
            }
        }

        // PUT /api/profile/me (لتحديث بيانات البروفايل)
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(new { Message = "User ID could not be determined from token." });

            try
            {
                // --- التحقق من الـ Username المأخوذ (إذا تم إرساله في الـ DTO) ---
                if (!string.IsNullOrWhiteSpace(updateProfileDto.Username))
                {
                    var existingUserWithNewUsername = await _userManager.FindByNameAsync(updateProfileDto.Username);
                    if (existingUserWithNewUsername != null && existingUserWithNewUsername.Id != userId)
                    {
                        return Conflict(new { Message = "Username is already taken." });
                    }
                }
                // -----------------------------------------------------------------

                var success = await _profileService.UpdateUserProfileAsync(userId, updateProfileDto);

                if (!success)
                {
                    // الـ Service هي المسؤولة عن منطق الفشل (مثلاً فشل الـ UpdateAsync في UserManager)
                    // إذا كان الفشل بسبب أن الـ Username مأخوذ، فالـ Check اللي فوق هيتعامل معاه
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to update profile. Please check your input or try again." });
                }
                return NoContent(); // أو Ok("Profile updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while updating your profile." });
            }
        }
    }
}