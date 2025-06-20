using Bookify.DTOs;          // للوصول لـ RegisterDto, LoginDto, ChangePasswordDto, ForgotPasswordDto, ResetPasswordDto
using Bookify.Entities;       // للوصول لـ ApplicationUser
using Bookify.Interfaces;     // <<< مهمة جداً عشان IEmailSender
// using Bookify.Services;    // الـ EmailSender موجود في الـ DI، مش محتاجين using هنا
using Microsoft.AspNetCore.Authorization; // للوصول لـ [Authorize]
using Microsoft.AspNetCore.Http;      // للوصول لـ StatusCodes
using Microsoft.AspNetCore.Identity;  // للوصول لـ UserManager
using Microsoft.AspNetCore.Mvc;       // للوصول لـ [ApiController], ControllerBase, etc.
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration; // للوصول لـ IConfiguration (لقراءة appsettings)
using Microsoft.IdentityModel.Tokens; // للوصول لـ SymmetricSecurityKey وغيرها
using System;                    // للوصول لـ Guid, DateTime, Convert, Exception
using System.Collections.Generic; // للوصول لـ List<>
using System.IdentityModel.Tokens.Jwt; // للوصول لـ JwtRegisteredClaimNames, JwtSecurityToken, JwtSecurityTokenHandler
using System.Linq;               // للوصول لـ .Select() للأخطاء
using System.Security.Claims;    // للوصول لـ Claim, ClaimTypes
using System.Text;               // للوصول لـ Encoding
using System.Threading.Tasks;    // للوصول لـ Task, async/await

namespace Bookify.Controllers
{
    [Route("api/[controller]")] // المسار الأساسي: /api/auth
    [ApiController]
    public class AuthController : ControllerBase // تم إزالة الـ constructor parameters من هنا واستخدام الـ primary constructor
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender; // <<< تم إضافته
        // private readonly RoleManager<IdentityRole> _roleManager; // معلق مؤقتاً

        // --- Constructor لحقن الخدمات المطلوبة ---
        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender // <<< تم إضافته هنا
            /*, RoleManager<IdentityRole> roleManager */)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender; // <<< تم إضافته هنا
            // _roleManager = roleManager;
        }

        // --- Endpoint للتسجيل (Register) ---
        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { Message = "User with this email already exists!" });
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(), // مهم للأمان
                UserName = registerDto.Username,
                Age = registerDto.Age ?? 0,
                Specialization = registerDto.Specialization,
                Level = registerDto.Level,
                Interest = registerDto.Interest
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "User creation failed! Please check user details and try again.", Errors = errors });
            }



            // --- بداية الكود الجديد لتأكيد الإيميل ---
            // 1. توليد الـ Email Confirmation Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // 1. Read the base URL from your appsettings.json
            // This will automatically use the value from appsettings.Development.json when running locally
            var baseUrl = "http://localhost:5173";

            if (string.IsNullOrEmpty(baseUrl))
            {
                // Handle the case where the configuration is missing, perhaps throw an exception
                throw new InvalidOperationException("Frontend BaseUrl is not configured in appsettings.json");
            }

            // 2. Define your frontend route and parameters
            var frontendRoute = "/confirm-email";
            var parameters = new Dictionary<string, string>
        {
            { "userId", user.Id.ToString() },
            { "token", token }
        };

            // 3. Create the fragment part of the URL
            var fragment = QueryHelpers.AddQueryString(frontendRoute, parameters);

            // 4. Combine the configured base URL with the fragment
            // We trim the trailing slash from the base URL to prevent issues like "http://localhost:3000//#/..."
            var confirmationLink = $"{baseUrl.TrimEnd('/')}#{fragment}";
            // 3. إرسال الإيميل (باستخدام الخدمة اللي عملناها)
            // تأكد من تعديل محتوى الإيميل ليكون أكثر احترافية
            await _emailSender.SendEmailAsync(user.Email, "Confirm your Bookify Account Email",
                $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
            // --- نهاية الكود الجديد ---

            // --- (اختياري) إضافة دور "User" افتراضي ---
            // if (!await _roleManager.RoleExistsAsync("User"))
            //     await _roleManager.CreateAsync(new IdentityRole("User"));
            // await _userManager.AddToRoleAsync(user, "User");
            // -----------------------------------------

            return Ok(new { Status = "Success", Message = "User created successfully! Please check your email to confirm your account." }); // تم تعديل الرسالة
        }

        // --- Endpoint جديدة لتأكيد الإيميل ---
        // GET /api/auth/confirm-email?userId=xxx&token=yyy
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { Message = "Invalid email confirmation link. User ID and Token are required." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // لا تكشف أن المستخدم غير موجود مباشرة
                return BadRequest(new { Message = "Invalid email confirmation link." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // ممكن ترجع صفحة HTML بسيطة أو توجيه للـ Frontend
                return Ok(new { Status = "Success", Message = "Email confirmed successfully! You can now login." });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description);
                // لا تكشف تفاصيل كثيرة هنا أيضاً
                return BadRequest(new { Message = "Email confirmation failed. Please try registering again or contact support.", Errors = errors });
            }
        }


        // --- Endpoint لتسجيل الدخول (Login) ---
        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid Email or Password." });
            }

            // --- التحقق إذا كان الإيميل مؤكد ---
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized(new { Message = "Email not confirmed. Please check your email for the confirmation link." });
            }
            // --- نهاية التحقق ---

            if (await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                var jwtTokenObject = await GenerateJwtToken(user);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtTokenObject);

                return Ok(new
                {
                    token = tokenString,
                    expiration = jwtTokenObject.ValidTo,
                    userId = user.Id, // <<< إضافة UserID هنا مفيدة للـ Frontend
                    username = user.UserName // <<< إضافة Username هنا مفيدة للـ Frontend
                });
            }

            return Unauthorized(new { Message = "Invalid Email or Password." });
        }

        // --- Endpoint لطلب إعادة تعيين كلمة المرور (Forgot Password) ---
        // POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // نرجع نفس الرسالة في كل الحالات لمنع كشف وجود المستخدمين
                return Ok(new { Status = "Success", Message = "If an account with this email exists and is confirmed, a password reset link has been sent." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var baseUrl = "http://localhost:5173";

            var frontendRoute = "/reset-password-page";
            var parameters = new Dictionary<string, string>
        {
            { "email", user.Email},
            { "token", token }
        };

            // 3. Create the fragment part of the URL
            var fragment = QueryHelpers.AddQueryString(frontendRoute, parameters);

            // 4. Combine the configured base URL with the fragment
            // We trim the trailing slash from the base URL to prevent issues like "http://localhost:3000//#/..."
            var resetLink = $"{baseUrl.TrimEnd('/')}#{fragment}";
            //var resetLink = Url.Action(nameof(ResetPasswordPage), "Auth", new { token = token, email = user.Email }, Request.Scheme); // نفترض صفحة للـ Reset
            // أو ممكن نرجع الـ token للـ Frontend وهو يبني اللينك لصفحة Reset Password عنده

            await _emailSender.SendEmailAsync(user.Email, "Bookify - Reset Your Password",
                $"To reset your password, please <a href='{resetLink}'>click here</a>. If you did not request a password reset, please ignore this email.");

            return Ok(new { Status = "Success", Message = "If an account with this email exists and is confirmed, a password reset link has been sent." });
        }

        // --- (اختياري) Endpoint لعرض صفحة إعادة تعيين كلمة المرور (لو الـ Backend هيعملها) ---
        // GET /api/auth/reset-password-page?token=xxx&email=yyy
        [HttpGet("reset-password-page")] // هذا Endpoint لعرض صفحة HTML أو توجيه
        public IActionResult ResetPasswordPage(string token, string email)
        {
            // هذا الـ Endpoint عادة ما يكون في الـ Frontend
            // لو الـ Backend سيرجع HTML:
            // return Content($"<html><body><form action='/api/auth/reset-password' method='post'>" +
            //                $"<input type='hidden' name='token' value='{token}' />" +
            //                $"<input type='hidden' name='email' value='{email}' />" +
            //                "New Password: <input type='password' name='newPassword' /><br/>" +
            //                "Confirm Password: <input type='password' name='confirmPassword' /><br/>" +
            //                "<input type='submit' value='Reset Password' /></form></body></html>", "text/html");

            // الأفضل للـ API أن ترجع فقط تأكيد أن الـ Token صالح أو توجيه للـ Frontend
            // أو أن الـ Frontend هو من يبني هذه الصفحة ويستدعي /api/auth/reset-password مباشرة
            return Ok(new { Message = "Please provide a new password.", Token = token, Email = email }); // مثال لإرجاع بيانات للـ Frontend
        }


        // --- Endpoint لتنفيذ إعادة تعيين كلمة المرور ---
        // POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                // لا تكشف أن المستخدم غير موجود
                return BadRequest(new { Message = "Password reset failed. Invalid request." });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Status = "Success", Message = "Password has been reset successfully! You can now login with your new password." });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Password reset failed.", Errors = errors });
            }
        }


        // --- Endpoint لتغيير كلمة المرور (للمستخدم المسجل دخوله) ---
        // POST /api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                var errors = changePasswordResult.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to change password.", Errors = errors });
            }

            return Ok(new { Status = "Success", Message = "Password changed successfully!" });
        }


        // --- Helper method لتوليد الـ JWT Token ---
        private async Task<JwtSecurityToken> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JWT");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                // الأفضل عمل Log للخطأ هنا بدلاً من رمي Exception يوقف التطبيق
                // Log.Error("JWT settings are not configured properly.");
                throw new InvalidOperationException("JWT settings (Secret, Issuer, Audience) are not configured properly in appsettings.json");
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // (اختياري) إضافة الـ Roles
            // var userRoles = await _userManager.GetRolesAsync(user);
            // foreach (var userRole in userRoles)
            // {
            //     authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            // }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var durationString = jwtSettings["DurationInMinutes"];
            var durationInMinutes = double.TryParse(durationString, out var duration) ? duration : 60.0; // الافتراضي 60 دقيقة
            var expirationTime = DateTime.UtcNow.AddMinutes(durationInMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: expirationTime,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}