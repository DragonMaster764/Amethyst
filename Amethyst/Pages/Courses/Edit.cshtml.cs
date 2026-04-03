using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Amethyst.Pages.Courses
{
    [Authorize(Roles = "Student")]
    public class EditModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public EditModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course =  await _context.Courses.FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            Course = course;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
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

            _context.Attach(Course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(Course.CourseId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
