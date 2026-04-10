using Amethyst.Data;
using Amethyst.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Amethyst.Pages;

public class AmyChatbotModel : PageModel
{    
    private readonly AIService _aiService;
    public AmyChatbotModel(AIService aiService)
    {
        _aiService = aiService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Please tell Amy what you're working on.")]
    public string UserInput { get; set; } = string.Empty;

    public string? FeedbackResult { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasResult => !string.IsNullOrEmpty(FeedbackResult);

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            FeedbackResult = await _aiService.PersonalFeedbackAsync(UserInput, userID);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Amy ran into an issue. Please try again.";
        }

        return Page();
    }
}