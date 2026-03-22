using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Security.Claims;

namespace Amethyst.Pages.CreateProfile
{
    public class UserFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserFormModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Profile InputProfile { get; set; } = new Profile();

        public IActionResult OnGet()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if profile already exists
            var existingProfile = _context.Profiles.FirstOrDefault(p => p.ProfileId == userId);

            if (existingProfile != null)
            {
                return RedirectToPage("/EditProfile", new { id = userId });
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToPage("/Account/Login");

            // Check again on POST
            var existingProfile = _context.Profiles.FirstOrDefault(p => p.ProfileId == userId);

            if (existingProfile != null)
            {
                return RedirectToPage("/EditProfile", new { id = userId });
            }

            // Ensure timezone has a value
            if (string.IsNullOrWhiteSpace(InputProfile.Timezone))
                InputProfile.Timezone = "UTC";

            var nowUtc = DateTime.UtcNow;

            InputProfile.ProfileId = userId;
            InputProfile.UserCreationDate = nowUtc;
            InputProfile.LastLoginTime = nowUtc;

            // AcademicYear stays null for non-students (correct)
            _context.Profiles.Add(InputProfile);
            _context.SaveChanges();

            return RedirectToPage("/Success");
        }
    }
}

