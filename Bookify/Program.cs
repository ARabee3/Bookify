using Bookify.Contexts;
using Bookify.Entities;
using Bookify.Hubs;
using Bookify.Interfaces;      // <<< مهمة جداً
using Bookify.Repositories;    // <<< مهمة جداً
using Bookify.Services;        // <<< مهمة جداً
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System; // <<< مهمة عشان Uri
using System.Collections.Generic; // عشان List<>
using System.Text;

namespace Bookify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- بداية تسجيل الخدمات ---

            // 1. الخدمات الأساسية للـ API
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // 2. إعداد Swagger لدعم JWT
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Bookify API", Version = "v1" });
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = JwtBearerDefaults.AuthenticationScheme
                    });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                            Scheme = "oauth2",
                            Name = JwtBearerDefaults.AuthenticationScheme,
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            // 3. تسجيل الـ DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 4. تسجيل خدمات Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedAccount = true; // تأكد من هذا الإعداد
                options.User.AllowedUserNameCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz@_-0123456789 ";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // 5. تسجيل JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            });

            // 6. تسجيل الـ Repositories والـ Services
            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<ISummaryRepository, SummaryRepository>();
            builder.Services.AddScoped<ISummaryService, SummaryService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>(); // خدمة الإيميل
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<AgoraService>();

            builder.Services.AddSignalR();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });
            // ...
            builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
            builder.Services.AddScoped<IProgressService, ProgressService>();
            builder.Services.AddScoped<IStreakService, StreakService>();

            builder.Services.AddScoped<IRatingRepository, RatingRepository>();
            builder.Services.AddScoped<IRatingService, RatingService>();








            builder.Services.AddHttpClient<IAiRecommendationService, AiRecommendationService>(client =>
            {
                string? aiApiBaseUrl = builder.Configuration.GetSection("AiApiSettings")["BaseUrl"];
                if (string.IsNullOrEmpty(aiApiBaseUrl))
                {
                    throw new InvalidOperationException("AI API Base URL is not configured in appsettings.json (AiApiSettings:BaseUrl)");
                }
                client.BaseAddress = new Uri(aiApiBaseUrl);
            });













            // (لو عملت IChapterRepository، سجله)
            // ...


            // --- 7. تسجيل خدمة التكامل مع الـ AI Recommendation API ---
            // استخدام AddHttpClient لتسجيل IAiRecommendationService كـ Typed HttpClient
            // مع تحديد الـ BaseAddress من الإعدادات.

            // ----------------------------------------------------

            // --- نهاية تسجيل الخدمات ---


            var app = builder.Build();



            
            // --- بداية إعداد الـ Middleware Pipeline ---

            app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookify API V1");
                });
            
            app.MapHub<SpaceHub>("/spacehub");
            app.UseCors("AllowSpecificOrigin");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // --- نهاية إعداد الـ Middleware Pipeline ---

            app.Run();
        }
    }
}