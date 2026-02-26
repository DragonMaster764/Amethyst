using Amethyst.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Amethyst.Pages.Admin
{
    [Authorize(Roles="Admin")]
    public class UserListModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserListModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Users { get; set; }
        public SelectList Roles { get; set; }

        public List<UserRoleViewModel> UserRolesList { get; set; }
        public string SearchTerm { get; set; }
        public string RoleFilter { get; set; }

        public class InputModel
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public string RoleName { get; set; }
        }

        public class UserRoleViewModel
        {
            public string ID { get; set; }
            public string Email { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }

        public async Task OnGetAsync(string searchTerm = null, string roleFilter = null)
        {
            SearchTerm = searchTerm;
            RoleFilter = roleFilter;
            PopulateSelectLists();
            await LoadUserRolesAsync(searchTerm, roleFilter);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PopulateSelectLists();
                await LoadUserRolesAsync();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(Input.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                PopulateSelectLists();
                await LoadUserRolesAsync();
                return Page();
            }

            var isInRole = await _userManager.IsInRoleAsync(user, Input.RoleName);
            if (isInRole)
            {
                ModelState.AddModelError(string.Empty, "User is already in this role");
                PopulateSelectLists();
                await LoadUserRolesAsync();
                return Page();
            }

            var result = await _userManager.AddToRoleAsync(user, Input.RoleName);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role '{Input.RoleName}' assigned to {user.Email} successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            PopulateSelectLists();
            await LoadUserRolesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                TempData["ErrorMessage"] = "User ID and Role are required.";
                return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
            }

            // Get current roles and remove them
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Add the new role
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role for {user.Email} updated to {roleName}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error updating role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Set a default password
            string newPassword = "Password123!";

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Password for {user.Email} has been reset to: {newPassword}";
            }
            else
            {
                TempData["ErrorMessage"] = "Error resetting password: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} has been deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage(new { searchTerm = SearchTerm, roleFilter = RoleFilter });
        }

        private void PopulateSelectLists()
        {
            // Create lists to avoid multiple database calls
            var usersList = _userManager.Users.OrderBy(u => u.Email).ToList();
            var rolesList = _roleManager.Roles.OrderBy(r => r.Name).ToList();

            Users = new SelectList(usersList, "Id", "Email");
            Roles = new SelectList(rolesList, "Name", "Name");
        }

        private async Task LoadUserRolesAsync(string searchTerm = null, string roleFilter = null)
        {
            UserRolesList = new List<UserRoleViewModel>();

            // Get all users first
            var allUsers = await _userManager.Users.ToListAsync();

            // Get all enails
            var userEmails = allUsers.Select(u => u.Email).ToList();

            // Process each user
            foreach (var user in allUsers)
            {
                // Get roles for this user
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.Any() ? string.Join(", ", roles) : "No Role";

                // Apply role filter if provided
                if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter))
                {
                    continue; // Skip this user if they don't have the filtered role
                }

                string userName = user.Email; // Default to email

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    bool nameMatch = userName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                    bool emailMatch = user.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

                    if (!nameMatch && !emailMatch)
                    {
                        continue; // Skip this user if they don't match the search term
                    }
                }

                UserRolesList.Add(new UserRoleViewModel
                {
                    ID = user.Id,
                    Email = user.Email,
                    Name = userName,
                    Role = currentRole
                });
            }

            // Sort the list by name
            UserRolesList = UserRolesList.OrderBy(u => u.Name).ToList();
        }
    }
}
