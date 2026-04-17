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
                var searchRequest = _youtubeService.Search.List("snippet");
                searchRequest.Q = song + " official audio";
                searchRequest.Type = "video";
                searchRequest.MaxResults = 1;
                var searchResult = await searchRequest.ExecuteAsync();
                var video = searchResult.Items.FirstOrDefault();

                if (video != null)
                {
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
                    await _youtubeService.PlaylistItems
                        .Insert(playlistItem, "snippet")
                        .ExecuteAsync();
                }
            }

            return $"https://www.youtube.com/playlist?list={createdPlaylist.Id}";
        }
    }
}