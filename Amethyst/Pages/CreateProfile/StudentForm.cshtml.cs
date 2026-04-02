using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Amethyst.Pages.CreateProfile
{
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
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            
            //if (user == null) return Page();
            
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // --- PROFILE EXISTS GUARD ---
            var existingProfile = await _context.Profile
                .FirstOrDefaultAsync(p => p.ProfileId == user.Id);

            if (existingProfile != null)
            {
                TempData["ProfileExists"] = "A profile already exists for your account.";
                return RedirectToPage("/CreateProfile/EditProfile", new { id = user.Id });
            }
            // ----------------------------

            InputProfile.ProfileId = user.Id;
            InputProfile.UserCreationDate = DateTime.UtcNow;
            InputProfile.LastLoginTime = null;

            _context.Profile.Add(InputProfile);
            await _context.SaveChangesAsync();

            return RedirectToPage("/CreateProfile/ProfileMade");
        }
    }
}

