using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.Text.Json;

namespace Amethyst.Controllers
{
    [Route("auth/google")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config) => _config = config;

        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            HttpContext.Session.SetString("AuthReturnUrl", returnUrl);

            var clientId = _config["Google:ClientId"];
            var redirectUri = _config["Google:RedirectUri"];
            var scope = "https://www.googleapis.com/auth/youtube";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&response_type=code" +
                      $"&scope={Uri.EscapeDataString(scope)}" +
                      $"&access_type=offline" +
                      $"&prompt=consent" +
                      $"&include_granted_scopes=false";

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

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OAuth token exchange failed: {body}");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(body);

            var accessToken = json.GetProperty("access_token").GetString();

            Console.WriteLine($"Token received: {accessToken?.Substring(0, 20)}..."); // debug

            var refreshToken = json.TryGetProperty("refresh_token", out var rt)
                ? rt.GetString()
                : null;

            HttpContext.Session.SetString("GoogleAccessToken", accessToken);

            if (!string.IsNullOrEmpty(refreshToken))
                HttpContext.Session.SetString("GoogleRefreshToken", refreshToken);

            var saved = HttpContext.Session.GetString("GoogleAccessToken");
            Console.WriteLine($"Token saved to session: {saved != null}");

            var returnUrl = HttpContext.Session.GetString("AuthReturnUrl") ?? "/";
            return LocalRedirect(returnUrl);
        }
    }
}
