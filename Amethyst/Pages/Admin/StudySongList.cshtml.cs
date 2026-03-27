using Amethyst.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Amethyst.Pages.Admin
{
    public class StudySongListModel : PageModel
    {
        private readonly MongoDBServices _mongoDBService;

        public List<Study_Songs> Songs { get; set; } = new List<Study_Songs>();

        public StudySongListModel(MongoDBServices mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task OnGetAsync()
        {
            Songs = await _mongoDBService.GetAllAsync();
        }
    }
}
