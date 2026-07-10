using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Diagnostics;

namespace VisualMetronome;

public class SpotifyService
{
    private SpotifyClient? _spotify;
    private readonly string _clientId;
    private const int ServerPort = 5000;
    private static readonly string CallbackUrl = $"http://127.0.0.1:{ServerPort}/callback";

    public bool IsAuthenticated => _spotify != null;
    
    public SpotifyService(string clientId)
    {
        _clientId = clientId;
    }

    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            Console.WriteLine("=== SPOTIFY AUTHENTICATION (PKCE) ===");
            Console.WriteLine("This allows access to your playlists");
            Console.WriteLine("");
            
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            var server = new EmbedIOAuthServer(new Uri(CallbackUrl), ServerPort);
            await server.Start();

            var loginRequest = new LoginRequest(
                new Uri(CallbackUrl),
                _clientId,
                LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { 
                    Scopes.UserReadPrivate,
                    Scopes.PlaylistReadPrivate, 
                    Scopes.PlaylistReadCollaborative
                }
            };

            var tcs = new TaskCompletionSource<bool>();

            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await server.Stop();
                
                var tokenResponse = await new OAuthClient().RequestToken(
                    new PKCETokenRequest(_clientId, response.Code, server.BaseUri, verifier)
                );

                Console.WriteLine("✓ Authentication successful");
                Console.WriteLine("");

                _spotify = new SpotifyClient(tokenResponse.AccessToken);
                
                // Verify authentication
                try
                {
                    var user = await _spotify.UserProfile.Current();
                    Console.WriteLine($"✓ Logged in as: {user.DisplayName}");
                    Console.WriteLine($"  User ID: {user.Id}");
                    Console.WriteLine($"  Account: {user.Product}");
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not fetch profile: {ex.Message}");
                }
                
                tcs.SetResult(true);
            };

            server.ErrorReceived += async (sender, error, errorDescription) =>
            {
                await server.Stop();
                Console.WriteLine($"✗ Authentication failed: {error} - {errorDescription}");
                tcs.SetResult(false);
            };

            var uri = loginRequest.ToUri();
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri.ToString(),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to open browser: {ex.Message}");
                await server.Stop();
                return false;
            }

            return await tcs.Task;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<PlaylistInfo>> GetUserPlaylistsAsync()
    {
        if (_spotify == null)
            throw new InvalidOperationException("Not authenticated");

        var playlists = new List<PlaylistInfo>();
        var page = await _spotify.Playlists.CurrentUsers();
        
        await foreach (var playlist in _spotify.Paginate(page))
        {
            playlists.Add(new PlaylistInfo
            {
                Id = playlist.Id ?? string.Empty,
                Name = playlist.Name ?? "Unknown",
                TrackCount = playlist.Tracks?.Total ?? 0
            });
        }

        return playlists;
    }

    public async Task<List<PlaylistTrack<IPlayableItem>>> GetPlaylistTracksAsync(string playlistId)
    {
        if (_spotify == null)
            throw new InvalidOperationException("Not authenticated");

        var tracks = new List<PlaylistTrack<IPlayableItem>>();
        var page = await _spotify.Playlists.GetItems(playlistId);
        
        await foreach (var track in _spotify.Paginate(page))
        {
            tracks.Add(track);
        }

        return tracks;
    }

    public async Task<int?> GetTrackBpmAsync(string trackId)
    {
        if (_spotify == null)
            throw new InvalidOperationException("Not authenticated");

        try
        {
            var features = await _spotify.Tracks.GetAudioFeatures(trackId);
            if (features?.Tempo > 0)
            {
                int bpm = (int)Math.Round(features.Tempo);
                Console.WriteLine($"✓ Spotify audio features returned BPM: {bpm}");
                return bpm;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Spotify audio features unavailable: {ex.Message}");
        }

        return null;
    }
}

public class PlaylistInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TrackCount { get; set; }
    
    public override string ToString() => $"{Name} ({TrackCount} tracks)";
}

public class TrackInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Artists { get; set; } = string.Empty;
    public int? Bpm { get; set; }
    
    public override string ToString() => $"{Name} - {Artists}";
}
