using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.EditProfile
{
    public class EditProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Profile InputProfile { get; set; }

        public bool IsStudentProfile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get logged-in user ID
            var userId = User?.Identity?.Name;
            if (userId == null)
                return RedirectToPage("/Account/Login");

            // Load existing profile
            var profile = await _context.Profile
                .FirstOrDefaultAsync(p => p.ProfileId == userId);

            if (profile == null)
            {
                // If no profile exists, redirect to create one
                return RedirectToPage("/CreateProfile/UserType");
            }

            // Bind to form
            InputProfile = profile;

            // Determine if this user is a student
            IsStudentProfile = !string.IsNullOrEmpty(profile.AcademicYear);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User?.Identity?.Name;
            if (userId == null)
                return RedirectToPage("/Account/Login");

            var profile = await _context.Profile
                .FirstOrDefaultAsync(p => p.ProfileId == userId);

            if (profile == null)
                return RedirectToPage("/CreateProfile/StudentForm");

            // ⭐ Add this right here
            if (!ModelState.IsValid)
                return Page();

            // Update fields
            profile.DisplayName = InputProfile.DisplayName;
            profile.Name = InputProfile.Name;
            profile.NotificationPreferences = InputProfile.NotificationPreferences;
            profile.QuietHoursStart = InputProfile.QuietHoursStart;
            profile.QuietHoursEnd = InputProfile.QuietHoursEnd;
            profile.Timezone = InputProfile.Timezone;

            // Only update AcademicYear if this user is a student
            if (!string.IsNullOrEmpty(profile.AcademicYear))
            {
                profile.AcademicYear = InputProfile.AcademicYear;
            }

            profile.LastLoginTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToPage("/ProfileUpdated");
        }
    }
}