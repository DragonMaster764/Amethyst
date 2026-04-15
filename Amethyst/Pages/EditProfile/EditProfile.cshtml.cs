using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Identity;

namespace Amethyst.Pages.EditProfile
{
    public class EditProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditProfileModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Profile InputProfile { get; set; }

        public bool IsStudentProfile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the REAL Identity user ID (GUID)
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToPage("/Account/Login");

            // Load existing profile
            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.ProfileId == userId);

            if (profile == null)
            {
                return RedirectToPage("/CreateProfile/UserType");
            }

            InputProfile = profile;
            IsStudentProfile = !string.IsNullOrEmpty(profile.AcademicYear);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToPage("/Account/Login");

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.ProfileId == userId);

            if (profile == null)
                return RedirectToPage("/CreateProfile/UserType");

            if (!ModelState.IsValid)
                return Page();

            // Update fields
            profile.DisplayName = InputProfile.DisplayName;
            profile.Name = InputProfile.Name;
            profile.NotificationPreferences = InputProfile.NotificationPreferences;
            profile.QuietHoursStart = InputProfile.QuietHoursStart;
            profile.QuietHoursEnd = InputProfile.QuietHoursEnd;
            profile.Timezone = InputProfile.Timezone;

            if (!string.IsNullOrEmpty(profile.AcademicYear))
            {
                profile.AcademicYear = InputProfile.AcademicYear;
            }

            profile.LastLoginTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToPage("/EditProfile/ProfileUpdated");
        }
    }
}