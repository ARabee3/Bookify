using Bookify.DTOs.Ai;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class AiContentService : IAiContentService
    {
        private readonly HttpClient _httpClient; // المحقون من Program.cs

        public AiContentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AiGetChaptersResponseDto?> DiscoverChaptersAsync(IFormFile pdfFile)
        {
            // نفترض أن endpoint اكتشاف الشابترات تأخذ نفس اسم الحقل للملف
            return await PostPdfAsync<AiGetChaptersResponseDto>("get-chapters", pdfFile);
        }

        public async Task<AiQuizResponseDto?> GenerateQuizAsync(IFormFile pdfFile, int chapterNumber)
        {
            var additionalData = new Dictionary<string, string>
            {
                { "chapter_number", chapterNumber.ToString() } // <<< هذا الاسم صحيح بناءً على Postman
            };
            return await PostPdfAsync<AiQuizResponseDto>("generate-quiz-chapter", pdfFile, additionalData);
        }

        public async Task<AiSummaryResponseDto?> GenerateSummaryAsync(IFormFile pdfFile, int chapterNumber)
        {
            var additionalData = new Dictionary<string, string>
            {
                { "chapter_number", chapterNumber.ToString() } // <<< هذا الاسم صحيح بناءً على Postman
            };
            return await PostPdfAsync<AiSummaryResponseDto>("summarize-chapter", pdfFile, additionalData);
        }

        // الميثود المساعدة العامة
        private async Task<T?> PostPdfAsync<T>(string relativeUrl, IFormFile file, Dictionary<string, string>? additionalData = null) where T : class
        {
            if (file == null || file.Length == 0) return null;

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            // --- تم التعديل هنا ليطابق Postman ---
            content.Add(streamContent, "file", file.FileName); // <<< تم تغيير اسم الـ field إلى "file"
            // ------------------------------------

            if (additionalData != null)
            {
                foreach (var item in additionalData)
                {
                    content.Add(new StringContent(item.Value), item.Key);
                }
            }

            try
            {
                var response = await _httpClient.PostAsync(relativeUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error from AI API ({relativeUrl}): {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while calling AI API ({relativeUrl}): {ex.Message}");
                return null;
            }
        }
    }
}