using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Amethyst.Data;
using Amethyst.Models;
using Amethyst.Constants;
using System.Security.Claims;

namespace Amethyst.Pages.Courses
{
    [Authorize(Roles="Student")]
    public class CreateModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public CreateModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //Fill out profile id from the logged in user
            Course.ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // The binder ran before we set ProfileId and may have recorded a validation error.
            // Clear the Course validation state and revalidate so the server-set ProfileId is considered.
            ModelState.ClearValidationState("Course");
            TryValidateModel(Course, "Course");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Courses.Add(Course);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
