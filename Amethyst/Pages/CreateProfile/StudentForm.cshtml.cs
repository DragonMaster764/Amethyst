using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Amethyst.Pages.CreateProfile
{
    [Authorize]
    public class StudentFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentFormModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Profile InputProfile { get; set; } = new Profile();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                // Optional: log validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("MODEL ERROR: " + error.ErrorMessage);
                }

                return Page();
            }

            // Get logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Check if profile already exists
            var existingProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.ProfileId == user.Id);

            if (existingProfile != null)
            {
                TempData["ProfileExists"] = "A profile already exists for your account.";
                return RedirectToPage("/CreateProfile/EditProfile", new { id = user.Id });
            }

            // Assign identity user ID as profile ID
            InputProfile.ProfileId = user.Id;
            InputProfile.UserCreationDate = DateTime.UtcNow;
            InputProfile.LastLoginTime = null;

            // Ensure timezone is never null
            if (string.IsNullOrWhiteSpace(InputProfile.Timezone))
                InputProfile.Timezone = "UTC";

            // Insert profile
            _context.Profiles.Add(InputProfile);
            await _context.SaveChangesAsync();

            // ⭐ Assign Student role
            await _userManager.AddToRoleAsync(user, "Student");

            // Redirect to success page
            return RedirectToPage("/CreateProfile/ProfileMade");
        }

    }
}
