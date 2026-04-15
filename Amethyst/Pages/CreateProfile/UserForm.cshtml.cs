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
    public class UserFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserFormModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Profile InputProfile { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value!.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine("MODEL ERROR: " + error);
                }

                throw new Exception("ModelState invalid: " + string.Join(" | ", errors));
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

            // Regular users do not have academic year
            InputProfile.AcademicYear = null;

            // Ensure timezone is never null
            if (string.IsNullOrWhiteSpace(InputProfile.Timezone))
                InputProfile.Timezone = "UTC";

            // Insert profile
            try
            {
                _context.Profiles.Add(InputProfile);
                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                throw new Exception("Profile save failed: " + ex.Message, ex);
            }

            // ⭐ Assign User role
            await _userManager.AddToRoleAsync(user, "User");

            // Redirect to success page
            return RedirectToPage("/CreateProfile/ProfileMade");
        }


    }
}