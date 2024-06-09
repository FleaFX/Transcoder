namespace Transcoder.Model;

/// <summary>
/// Represents a playlist with channels.
/// </summary>
/// <param name="channels">The list of channels that makes up the playlist.</param>
public struct Playlist(Channel[] channels) {
    public static Playlist Parse(string m3u) {
        var channels = new List<Channel>();
        var lines = m3u.Split(Environment.NewLine);
        for (var i = 0; i < lines.Length; i++) {
            if (!lines[i].StartsWith("#EXTINF")) continue;
            var name = lines[i].Split(',')[1].Trim();
            var uri = new Uri(lines[i + 1]);
            channels.Add(new Channel(name, uri));
        }

        return new Playlist([..channels]);
    }
}