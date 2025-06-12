using Bookify.DTOs;         
using Bookify.Entities;       
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;      
using Microsoft.AspNetCore.Identity;  
using Microsoft.AspNetCore.Mvc;       
using System;                    
using System.Linq;               
using System.Security.Claims;    
using System.Threading.Tasks;    

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار الأساسي: /api/profile
    [ApiController]
    [Authorize] // <<< تطبيق الـ Authorization على مستوى الكنترولر كله (كل الـ Actions جواه هتتطلب Login)
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager; // لحقن الـ UserManager

        // Constructor لحقن الـ UserManager
        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                // نجيب الـ ID بتاع المستخدم الحالي من الـ Token اللي اتبعت مع الـ Request
                // الـ [Authorize] فوق بتتأكد إن الـ Claim ده موجود والـ Token صالح
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // احتياطي: لو الـ ID مش موجود (المفروض متحصلش بسبب [Authorize])
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User ID claim not found in token." });
                }

                // نجيب بيانات المستخدم الكاملة من قاعدة البيانات باستخدام الـ ID
                var user = await _userManager.FindByIdAsync(userId);

                // لو المستخدم مش موجود في الداتا بيز (حالة نادرة جداً لو الـ Token لسه صالح والمستخدم اتحذف)
                if (user == null)
                {
                    return NotFound(new { Message = "User associated with this token not found." });
                }

                // نحول بيانات المستخدم من Entity (ApplicationUser) إلى DTO (ProfileDto)
                // عشان نرجع بس البيانات اللي محتاجين نعرضها في البروفايل
                var profileDto = new ProfileDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Age = user.Age,
                    Specialization = user.Specialization,
                    Level = user.Level,
                    Interest = user.Interest
                    // نضيف أي حقول تانية موجودة في ProfileDto هنا
                };

                // نرجع الـ DTO بنجاح (200 OK)
                return Ok(profileDto);
            }
            catch (Exception ex)
            {
                // معالجة أي خطأ غير متوقع ممكن يحصل
                // الأفضل استخدام نظام Logging حقيقي بدل Console.WriteLine
                Console.WriteLine($"Error getting profile for user {User.FindFirstValue(ClaimTypes.NameIdentifier)}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while retrieving the profile." });
            }
        }

        // --- Endpoint لتحديث بيانات بروفايل المستخدم الحالي ---
        // PUT /api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto updateDto)
        {
            // التحقق من صحة الـ DTO اللي جاي من الـ Request Body باستخدام الـ DataAnnotations
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // نجيب الـ ID بتاع المستخدم الحالي
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized(); // احتياطي

                // نجيب المستخدم من قاعدة البيانات
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound(new { Message = "User not found." });

                // نحدّث البيانات في كائن الـ user بالقيم الجديدة اللي جاية من الـ DTO
                // (فقط لو المستخدم بعت قيمة جديدة - عشان كده بنستخدم ?. أو .HasValue)
                user.Age = updateDto.Age ?? user.Age; // لو updateDto.Age مش null، استخدم قيمته، وإلا سيب القيمة القديمة
                user.Specialization = updateDto.Specialization ?? user.Specialization;
                user.Level = updateDto.Level ?? user.Level;
                user.Interest = updateDto.Interest ?? user.Interest;
                // لا نغير الإيميل أو الـ Username هنا لأنها عمليات أكثر تعقيداً وتحتاج تأكيد

                // نحاول نحفظ التغييرات في قاعدة البيانات
                var result = await _userManager.UpdateAsync(user);

                // لو عملية الحفظ فشلت (مثلاً بسبب مشكلة في الداتا بيز)
                if (!result.Succeeded)
                {
                    // نرجع الأخطاء اللي جاية من Identity
                    return BadRequest(new { Message = "Failed to update profile.", Errors = result.Errors.Select(e => e.Description) });
                }

                // لو نجحت، نرجع رسالة نجاح
                // ممكن نرجع البروفايل المحدث (بعد تحويله لـ ProfileDto) لو الـ Frontend محتاجه
                // var updatedProfileDto = new ProfileDto { ... }; // Convert user to ProfileDto
                // return Ok(updatedProfileDto);
                return Ok(new { Message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile for user {User.FindFirstValue(ClaimTypes.NameIdentifier)}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred while updating the profile." });
            }
        }
    }
}