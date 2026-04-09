using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Amethyst.Models; // update to your actual namespace

namespace Amethyst.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly IMongoCollection<Study_Songs> _studySongs;

        public AIService(HttpClient httpClient, IConfiguration config, ILogger<AIService> logger, IMongoClient mongoClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Gemini settings from appsettings.json under AISettings
            var aiSettings = config.GetSection("AISettings");
            _apiKey = aiSettings["ApiKey"] ?? throw new InvalidOperationException("AISettings:ApiKey is not configured.");
            _model = aiSettings["Model"] ?? "gemini-2.0-flash";
            var baseAddress = aiSettings["BaseAddress"] ?? "https://generativelanguage.googleapis.com/v1beta/";
            _httpClient.BaseAddress = new Uri(baseAddress);

            // MongoDB — collection name from appsettings.json under MongoDBSettings
            var mongoSettings = config.GetSection("MongoDBSettings");
            var databaseName = mongoSettings["DatabaseName"] ?? throw new InvalidOperationException("MongoDBSettings:DatabaseName is not configured.");
            var collectionName = mongoSettings["CollectionName"] ?? throw new InvalidOperationException("MongoDBSettings:CollectionName is not configured.");
            var database = mongoClient.GetDatabase(databaseName);
            _studySongs = database.GetCollection<Study_Songs>(collectionName);
        }

        private static string ExtractGeminiText(string respText, ILogger logger)
        {
            try
            {
                using var doc = JsonDocument.Parse(respText);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var first = candidates[0];
                    if (first.TryGetProperty("content", out var contentProp) &&
                        contentProp.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString() ?? string.Empty;
                    }
                }
                logger.LogError("Gemini API returned unexpected JSON shape: {Response}", respText);
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse Gemini response JSON: {Response}", respText);
                throw;
            }
        }

        // Ask Gemini a free-form question with no MongoDB context
        public async Task<string> AskGeminiAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentNullException(nameof(prompt));

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            return await PostToGeminiAsync(payload);
        }

        //Let the user get a curated playlist based on their mood + MongoDB songs
        public async Task<string> GetStudyPlaylistAsync(string userMoodInput)
        {
            if (string.IsNullOrWhiteSpace(userMoodInput))
                throw new ArgumentNullException(nameof(userMoodInput));

            // Pull all songs from MongoDB
            var songs = await _studySongs.Find(_ => true).ToListAsync();
            var songsJson = JsonSerializer.Serialize(songs);

            var prompt = $"""
                You are a music curator assistant. A student is looking for study songs that match how they feel.

                Below is a JSON array of available songs from our database. You MUST only recommend songs from this list — do not invent or suggest songs that are not in it.

                Available songs:
                {songsJson}

                The student says: "{userMoodInput}"

                Based on their mood and preferences, recommend a curated playlist by selecting songs from the list above.
                For each song you pick, briefly explain (1 sentence) why it fits their mood.
                Format your response as an HTML ordered list (`<ol>`). Each song should be in `<li>` with the song title in `<strong>` tags. End with a short encouraging note in a `<p>`.
                """;

            var payload = new
            {
                contents = new[]
                {
            new { parts = new[] { new { text = prompt } } }
        }
            };

            return await PostToGeminiAsync(payload);
        }

        // Send a specific StudySong document + a question to Gemini
        public async Task<string> AskAboutSongAsync(Study_Songs song, string userQuestion)
        {
            if (song == null) throw new ArgumentNullException(nameof(song));
            if (string.IsNullOrWhiteSpace(userQuestion)) throw new ArgumentNullException(nameof(userQuestion));

            var songJson = JsonSerializer.Serialize(song);

            var prompt = $"""
                You are a helpful music assistant. Here is a study song in JSON format:
                {songJson}

                Question: {userQuestion}
                """;

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            return await PostToGeminiAsync(payload);
        }

        // Shared helper to POST to Gemini and return the text response
        private async Task<string> PostToGeminiAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var endpoint = $"models/{_model}:generateContent?key={_apiKey}";
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var resp = await _httpClient.PostAsync(endpoint, content);
            var respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {Status} - {Response}", resp.StatusCode, respText);
                throw new InvalidOperationException($"Gemini API error: {resp.StatusCode} - {respText}");
            }

            return ExtractGeminiText(respText, _logger);
        }
    }
}