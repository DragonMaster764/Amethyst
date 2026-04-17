using Amethyst.Models;
using Microsoft.EntityFrameworkCore; // update to your actual namespace
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SQLitePCL;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amethyst.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly IMongoCollection<Study_Songs> _studySongs;

        private readonly Amethyst.Data.ApplicationDbContext _context;

        public AIService(HttpClient httpClient, ILogger<AIService> logger, IMongoClient mongoClient, Amethyst.Data.ApplicationDbContext context)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // Gemini settings from appsettings.json under AISettings
            //var aiSettings = config.GetSection("AISettings");
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? throw new InvalidOperationException("AISettings:ApiKey is not configured.");
            _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.0-flash";
            var baseAddress = Environment.GetEnvironmentVariable("GEMINI_BASE_ADDRESS") ?? "https://generativelanguage.googleapis.com/v1beta/";
            _httpClient.BaseAddress = new Uri(baseAddress);

            // MongoDB — collection name from appsettings.json under MongoDBSettings
            //var mongoSettings = config.GetSection("MongoDBSettings");
            var databaseName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? throw new InvalidOperationException("MongoDBSettings:DatabaseName is not configured.");
            var collectionName = Environment.GetEnvironmentVariable("MONGO_DB_COLLECTION_NAME") ?? throw new InvalidOperationException("MongoDBSettings:CollectionName is not configured.");
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

                Select exactly 10 songs from the list.

                OUTPUT REQUIREMENTS:

                1. Return an HTML ordered list (<ol>)
                   - Each song in a <li>
                   - Format: <strong>Song Title - Artist</strong>
                   - Include one short sentence explaining why it fits

                2. Add a short encouraging message in a <p> tag

                3. Then include a script tag:
                   <script type="application/json" id="playlist-data"></script>

                4. Inside that script tag, return a valid JSON array of objects.
                   Each object must include:
                   - title (string)
                   - artist (string)

                STRICT RULES:
                - Only use songs from the provided list
                - No text outside the HTML and script tag
                - JSON must be valid
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

        // Let the user get feedback about their productivity habits
        public async Task<string> PersonalFeedbackAsync(string userInput, string userID)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentNullException(nameof(userInput));

            //Current upcoming uncompleted assignments
            var assignments = await _context.Assignments
                .Where(a => a.Course.ProfileId == userID && a.DueDate > DateTime.Now && a.Status != "Completed")
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Priority)
                .Take(5)
                .ToListAsync();

            //Terms used for ordering courses by current term
            var termOrder = new Dictionary<string, int>
            {
                { "Winter", 0 },
                { "Spring", 1 },
                { "Summer", 2 },
                { "Fall", 3 }
            };

            // Determine the most recent (AcademicYear, Term) pair reliably, then load all courses for that pair.
            var latestTermPair = await _context.Courses
                .Where(c => c.ProfileId == userID)
                .Select(c => new { c.Term, c.AcademicYear })
                .Distinct()
                .OrderByDescending(x => x.AcademicYear)
                .ThenByDescending(x => x.Term == "Fall" ? 3
                                       : x.Term == "Summer" ? 2
                                       : x.Term == "Spring" ? 1
                                       : x.Term == "Winter" ? 0
                                       : -1)
                .FirstOrDefaultAsync();

            var courses = latestTermPair is null
                ? new List<Course>()
                : await _context.Courses
                    .Where(c => c.ProfileId == userID
                                && c.AcademicYear == latestTermPair.AcademicYear
                                && c.Term == latestTermPair.Term)
                    .ToListAsync();

            var tasks = await _context.TaskItems
                .Where(t => t.Profile.ProfileId == userID && t.DueAt > DateTime.Now && t.Status != "Completed")
                .OrderBy(t => t.DueAt)
                .ThenBy(t => t.Priority)
                .Take(5)
                .ToListAsync();

            // Build assignment context
            var assignmentContext = assignments.Any()
                ? string.Join("\n", assignments.Select(a =>
                    $"- {a.Title} (Due: {a.DueDate:MM/dd/yyyy}, Description: {a.Description}, Priority: {a.Priority}, Status: {a.Status}, Course: {a.Course.Title ?? "Unknown"}, Total Points: {a.TotalPoints}, Estimated Minutes: {a.EstimatedMinutes})"))
                : "No upcoming assignments.";

            // Build courses context
            var courseContext = courses.Any()
                ? string.Join("\n", courses.Select(c =>
                    $"- {c.Title} ({c.Term} {c.AcademicYear}), Meeting Time: {c.MeetingTime}"))
                : "No current courses found.";

            // Build tasks context
            var taskContext = tasks.Any()
                ? string.Join("\n", tasks.Select(t =>
                    $"- {t.Title} (Due: {t.DueAt:MM/dd/yyyy}, Priority: {t.Priority}, Status: {t.Status}, Estimated Minutes: {t.EstimatedMinutes})"))
                : "No upcoming tasks.";

            // Build final prompt
            var systemPrompt = $"""
                You are Amy, a friendly and focused productivity assistant for students.
                Your job is to help the student decide what to work on, how to start, and how to manage their time.
                Keep responses concise, encouraging, and actionable. Do not overwhelm them with too much at once.
                Never mention that you were given data — just respond naturally as if you know their situation.
                Format your response as clean HTML using only <p>, <ul>, <li>, and <strong> tags. Do not include <html>, <head>, <body>, or any CSS.

                Current date: {DateTime.Now:MM/dd/yyyy}

                Student's current courses ({latestTermPair?.Term} {latestTermPair?.AcademicYear}):
                {courseContext}

                Upcoming assignments (next 7 days, sorted by due date):
                {assignmentContext}

                Upcoming tasks (sorted by due date):
                {taskContext}

                Student message: {userInput}
                """;

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = systemPrompt } } }
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