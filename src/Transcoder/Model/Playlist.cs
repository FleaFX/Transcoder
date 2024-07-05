using System.Collections;

namespace Transcoder.Model;

/// <summary>
/// Represents a playlist with channels.
/// </summary>
/// <param name="channels">The list of channels that makes up the playlist.</param>
public class Playlist(Channel[] channels) : IEnumerable<Channel> {
    /// <summary>
    /// Parses a <see cref="string"/> as an M3U playlist file.
    /// </summary>
    /// <param name="m3u">The <see cref="string"/> to parse.</param>
    /// <returns>A <see cref="Playlist"/>.</returns>
    public static Playlist Parse(string m3u) {
        var channels = new List<Channel>();
        var lines = m3u.Split('\n');
        for (var i = 0; i < lines.Length; i++) {
            if (!lines[i].StartsWith("#EXTINF")) continue;
            var name = lines[i].Split(',')[1].Trim();
            var uri = new Uri(lines[i + 1]);
            channels.Add(new Channel(name, uri));
        }

        return new Playlist([..channels]);
    }

    /// <summary>
    /// Formats the playlist as a M3U file.
    /// </summary>
    /// <returns>A <see cref="string"/>.</returns>
    public string GetM3U(Func<string, string> createChannelUrl) =>
        $"""
         #EXTM3U
         {channels.Reverse().Aggregate("", (acc, channel) => $"{channel.AsM3UChannel(createChannelUrl)}{Environment.NewLine}{acc}")}
         """;

    /// <summary>
    /// Gets the <see cref="Channel"/> with the given name.
    /// </summary>
    /// <param name="channelName">The name of the channel to return.</param>
    /// <returns>A <see cref="Channel"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when the playlist doesn't contain a channel with the requested name.</exception>
    public Channel this[string channelName] =>
        channels.FirstOrDefault(channel => channel.Equals(new Channel(channelName))) ?? throw new IndexOutOfRangeException();

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<Channel> GetEnumerator() => ((IEnumerable<Channel>)channels).GetEnumerator();

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}