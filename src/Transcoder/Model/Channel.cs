namespace Transcoder.Model;

/// <summary>
/// Represents a single channel.
/// </summary>
/// <param name="name">The name of the channel.</param>
/// <param name="uri">The URI where the channel stream can be read.</param>
public struct Channel(string name, Uri uri);