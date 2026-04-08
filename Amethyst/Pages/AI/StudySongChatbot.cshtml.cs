using Amethyst.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amethyst.Services;

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
    }
}