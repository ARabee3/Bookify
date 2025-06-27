using Bookify.DTOs;
using Bookify.Entities; // لإضافة ApplicationUser (عشان UserManager)
using Bookify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;    // لإضافة IFormFile و StatusCodes
using Microsoft.AspNetCore.Identity; // لإضافة UserManager
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;   // لإضافة List
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار الأساسي: /api/profile
    [ApiController]
    [Authorize] // كل الـ Endpoints هنا مؤمنة
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IProgressService _progressService;
        private readonly UserManager<ApplicationUser> _userManager; // مازال موجوداً للـ Check بتاع Username

        public ProfileController(
            IProfileService profileService,
            IProgressService progressService,
            UserManager<ApplicationUser> userManager)
        {
            _profileService = profileService;
            _progressService = progressService;
            _userManager = userManager;
        }

        // GET /api/profile/me (لجلب بيانات صفحة البروفايل المجمعة)
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfilePage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(new { Message = "User ID could not be determined from token." });

            try
            {
                var userProfile = await _profileService.GetUserProfileAsync(userId);
                var readingStats = await _profileService.GetUserReadingStatsAsync(userId);
                var currentlyReading = await _progressService.GetCurrentlyReadingBooksAsync(userId);

                if (userProfile == null) // التحقق من البروفايل يكفي هنا، الباقي ممكن يرجع بيانات افتراضية
                {
                    return NotFound(new { Message = "User profile could not be retrieved." });
                }

                var profilePageData = new ProfilePageDto
                {
                    UserProfile = userProfile,
                    ReadingStats = readingStats ?? new UserReadingStatsDto(),
                    CurrentlyReadingBooks = currentlyReading ?? new List<BookProgressDto>()
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
                if (!string.IsNullOrWhiteSpace(updateProfileDto.Username))
                {
                    var user = await _userManager.FindByIdAsync(userId); // نحصل على المستخدم الحالي للتأكد
                    if (user != null && !user.UserName.Equals(updateProfileDto.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        var existingUserWithNewUsername = await _userManager.FindByNameAsync(updateProfileDto.Username);
                        if (existingUserWithNewUsername != null && existingUserWithNewUsername.Id != userId)
                        {
                            return Conflict(new { Message = "Username is already taken." });
                        }
                    }
                }

                var success = await _profileService.UpdateUserProfileAsync(userId, updateProfileDto);
                if (!success)
                {
                    // الـ Service قد ترجع false لأسباب أخرى (مثل فشل UserManager.UpdateAsync)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to update profile. Please check your input or try again." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while updating your profile." });
            }
        }

        // --- Endpoint جديدة لرفع/تغيير صورة البروفايل ---
        // POST /api/profile/me/picture
        [HttpPost("me/picture")]
        public async Task<IActionResult> UploadMyProfilePicture(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized(new { Message = "User ID not found." });

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file uploaded or file is empty." });
            }
            if (file.Length > 5 * 1024 * 1024) // مثال: حد أقصى 5MB
            {
                return BadRequest(new { Message = "File size exceeds the limit of 5MB." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { Message = "Invalid file type. Only JPG, JPEG, PNG are allowed." });
            }


            try
            {
                // الـ IProfileService هو المسؤول عن الـ Logic الفعلي للرفع والتحديث
                var newProfilePictureUrl = await _profileService.UploadProfilePictureAsync(userId, file);

                if (string.IsNullOrEmpty(newProfilePictureUrl))
                {
                    // الـ Service رجعت null (يعني فيه خطأ حصل أثناء الرفع أو الحفظ)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to upload profile picture." });
                }
                return Ok(new { ProfilePictureUrl = newProfilePictureUrl });
            }
            catch (KeyNotFoundException knfEx) // لو المستخدم مش موجود (الـ Service ممكن ترميها)
            {
                return NotFound(new { Message = knfEx.Message });
            }
            catch (ArgumentException argEx) // لو فيه مشكلة في الملف (الـ Service ممكن ترميها)
            {
                return BadRequest(new { Message = argEx.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading profile picture for user {userId}: {ex.ToString()}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while uploading your profile picture." });
            }
        }
        // --- نهاية Endpoint رفع الصورة ---
    }
}