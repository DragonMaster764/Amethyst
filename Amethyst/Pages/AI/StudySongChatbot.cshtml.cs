using Amethyst.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace Amethyst.Pages
{
    public class StudySongChatbotModel : PageModel
    {
        private readonly AIService _aiService;

        public StudySongChatbotModel(AIService aiService)
        {
            _aiService = aiService;
        }

        [BindProperty]
        public string? UserMoodInput { get; set; }

        // Bound when user submits the "Save to YouTube" form
        [BindProperty]
        public string? PlaylistName { get; set; }

        // Hidden input that receives the JSON array from the page script
        [BindProperty]
        public string? Songs { get; set; }

        public string? PlaylistURL { get; set; }

        public string? PlaylistResult { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasResult => !string.IsNullOrEmpty(PlaylistResult);

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserMoodInput))
            {
                ErrorMessage = "Please describe your mood or what kind of music you're looking for.";
                return Page();
            }

            try
            {
                PlaylistResult = await _aiService.GetStudyPlaylistAsync(UserMoodInput);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Something went wrong: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveToYouTubeAsync()
        {
            var accessToken = HttpContext.Session.GetString("GoogleAccessToken");
            var refreshToken = HttpContext.Session.GetString("GoogleRefreshToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                ErrorMessage = "You must sign in with Google before saving a playlist.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(PlaylistName))
            {
                ErrorMessage = "Please provide a name for your playlist.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Songs))
            {
                ErrorMessage = "No songs were provided to save.";
                return Page();
            }

            try
            {
                // Expecting Songs to be a JSON array of objects like: [{ "title": "...", "artist": "..." }, ...]
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<List<SongDto>>(Songs, options);

                if (parsed == null || parsed.Count == 0)
                {
                    ErrorMessage = "Could not parse any songs from the provided data.";
                    return Page();
                }

                // Build search-friendly strings for each song (title + artist)
                var songTitles = parsed
                    .Where(s => !string.IsNullOrWhiteSpace(s.title))
                    .Select(s => string.IsNullOrWhiteSpace(s.artist) ? s.title!.Trim() : $"{s.title!.Trim()} - {s.artist!.Trim()}")
                    .ToList();

                if (!songTitles.Any())
                {
                    ErrorMessage = "Parsed songs did not contain titles.";
                    return Page();
                }

                try
                {
                    // Use YouTube playlist service to create a playlist and add videos
                    var ytService = new YouTubePlaylistService(accessToken);
                    var playlistUrl = await ytService.CreatePlaylistFromRecommendations(PlaylistName.Trim(), songTitles);
                    PlaylistURL = $"Playlist saved successfully. <a href=\"{playlistUrl}\" target=\"_blank\" rel=\"noopener\">Open on YouTube</a>";
                }
                catch(Google.GoogleApiException ex)
                {
                    Console.WriteLine($"HTTP Status: {ex.HttpStatusCode}");
                    Console.WriteLine($"Error message: {ex.Message}");

                    if (ex.Error?.Errors != null)
                    {
                        foreach (var err in ex.Error.Errors)
                        {
                            Console.WriteLine($"  Reason: {err.Reason}");
                            Console.WriteLine($"  Domain: {err.Domain}");
                            Console.WriteLine($"  Message: {err.Message}");
                            Console.WriteLine($"  Location: {err.Location}");
                            Console.WriteLine($"  LocationType: {err.LocationType}");
                        }
                    }
                }
                
            }
            catch (JsonException jex)
            {
                ErrorMessage = $"Failed to parse songs JSON: {jex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save playlist to YouTube: {ex.Message}";
            }

            return Page();
        }

        // DTO used to deserialize the JSON produced by the client/AI
        private class SongDto
        {
            public string? title { get; set; }
            public string? artist { get; set; }
        }
    }
}