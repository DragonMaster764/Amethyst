using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Amethyst.Controllers
{
    [Route("auth/google")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config) => _config = config;

        [HttpGet("login")]
        public IActionResult Login()
        {
            var clientId = _config["Google:ClientId"];
            var redirectUri = _config["Google:RedirectUri"];
            var scope = "https://www.googleapis.com/auth/youtube";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&response_type=code" +
                      $"&scope={Uri.EscapeDataString(scope)}" +
                      $"&access_type=offline";

            return Redirect(url);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            // Exchange code for access token
            using var http = new HttpClient();
            var response = await http.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = _config["Google:ClientId"],
                    ["client_secret"] = _config["Google:ClientSecret"],
                    ["redirect_uri"] = _config["Google:RedirectUri"],
                    ["grant_type"] = "authorization_code"
                }));

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = json.GetProperty("access_token").GetString();

            HttpContext.Session.SetString("GoogleAccessToken", token);
            return RedirectToAction("Index", "Home");
        }
    }
}
