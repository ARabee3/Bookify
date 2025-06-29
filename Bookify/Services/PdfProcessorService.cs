using Bookify.DTOs.PdfProcessor;
using Bookify.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Bookify.Services
{
    public class PdfProcessorService : IPdfProcessorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PdfProcessorService> _logger;

        public PdfProcessorService(HttpClient httpClient, ILogger<PdfProcessorService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PdfGetChaptersResponseDto?> GetChaptersAsync(IFormFile pdfFile)
        {
            var response = await PostPdfAsync<PdfGetChaptersResponseDto>("get-chapters", pdfFile);
            return response?.Data;
        }

        public async Task<PdfSummaryResponseDto?> SummarizeChapterAsync(IFormFile pdfFile, int chapterNumber)
        {
            var additionalData = new Dictionary<string, string>
            {
                { "chapter_number", chapterNumber.ToString() }
            };
            var response = await PostPdfAsync<PdfSummaryResponseDto>("summarize-chapter", pdfFile, additionalData);
            return response?.Data;
        }

        public async Task<PdfQuizResponseDto?> GenerateQuizForChapterAsync(IFormFile pdfFile, int chapterNumber)
        {
            var additionalData = new Dictionary<string, string>
            {
                { "chapter_number", chapterNumber.ToString() }
            };
            var response = await PostPdfAsync<PdfQuizResponseDto>("generate-quiz-chapter", pdfFile, additionalData);
            return response?.Data;
        }

        private async Task<PdfApiResponseDto<T>?> PostPdfAsync<T>(string relativeUrl, IFormFile file, Dictionary<string, string>? additionalData = null)
        {
            if (file == null || file.Length == 0) return null;

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            // The new API documentation specifies the parameter name is "file"
            content.Add(streamContent, "file", file.FileName);

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
                    var apiResponse = await response.Content.ReadFromJsonAsync<PdfApiResponseDto<T>>();
                    if (apiResponse?.Status == "success")
                    {
                        return apiResponse;
                    }
                    else
                    {
                        _logger.LogError("AI API returned non-success status: {Status} - {Message}", apiResponse?.Status, apiResponse?.Message);
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error from AI API ({Url}): {StatusCode} - {ErrorContent}", relativeUrl, response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling AI API ({Url})", relativeUrl);
                return null;
            }
        }
    }
}