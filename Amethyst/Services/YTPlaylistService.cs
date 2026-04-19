using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;

namespace Amethyst.Services
{
    public class YouTubePlaylistService
    {
        private readonly YouTubeService _youtubeService;

        public YouTubePlaylistService(string accessToken)
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = GoogleCredential
                    .FromAccessToken(accessToken)
                    .CreateScoped(YouTubeService.Scope.Youtube)
            });
        }

        public async Task<string> CreatePlaylistFromRecommendations(
            string playlistName,
            List<string> songTitles)
        {
            // 1. Create the playlist
            var playlist = new Playlist
            {
                Snippet = new PlaylistSnippet { Title = playlistName },
                Status = new PlaylistStatus { PrivacyStatus = "private" }
            };
            var createRequest = _youtubeService.Playlists.Insert(playlist, "snippet,status");
            var createdPlaylist = await createRequest.ExecuteAsync();

            // 2. Search for each song and add to playlist
            foreach (var song in songTitles)
            {
                try
                {
                    var searchRequest = _youtubeService.Search.List("snippet");
                    searchRequest.Q = song + " official audio";
                    searchRequest.Type = "video";
                    searchRequest.MaxResults = 1;
                    var searchResult = await searchRequest.ExecuteAsync();
                    var video = searchResult.Items.FirstOrDefault();

                    if (video == null)
                    {
                        Console.WriteLine($"No results found for: {song}");
                        continue;
                    }

                    var playlistItem = new PlaylistItem
                    {
                        Snippet = new PlaylistItemSnippet
                        {
                            PlaylistId = createdPlaylist.Id,
                            ResourceId = new ResourceId
                            {
                                Kind = "youtube#video",
                                VideoId = video.Id.VideoId
                            }
                        }
                    };

                    await ExecuteWithRetryAsync(() =>
                        _youtubeService.PlaylistItems.Insert(playlistItem, "snippet").ExecuteAsync());

                    Console.WriteLine($"Added: {song}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add '{song}': {ex.Message} — skipping");
                    continue; // skip this song and keep going
                }
            }

            return $"https://www.youtube.com/playlist?list={createdPlaylist.Id}";
        }

        //Retries the action if there are issues, but will eventually skip if it doesn't work
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (Google.GoogleApiException ex)
                    when (ex.HttpStatusCode == System.Net.HttpStatusCode.Conflict ||
                          ex.HttpStatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // 1s, 2s, 4s
                }
            }
            throw new Exception("Max retries exceeded");
        }
    }
}