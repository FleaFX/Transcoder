using Microsoft.AspNetCore.Mvc;
using Transcoder.Model;

namespace Transcoder.Controllers;

[ApiController]
public class TranscodeController(IConfiguration configuration) : ControllerBase {
    readonly Playlist _playlist =
        Playlist.Parse(System.IO.File.ReadAllText(configuration.GetValue<string>(nameof(Playlist))!));

    /// <summary>
    /// Tune into the channel with the requested name and transcode the video stream.
    /// </summary>
    /// <param name="channelName">The name of the channel to tune into.</param>
    /// <returns>A <see cref="IActionResult"/>.</returns>
    [HttpGet("{channelName}")]
    public IActionResult Get(string channelName) {
        try {
            return new FileStreamResult(_playlist[channelName].Transcode(), "video/mp2t");
        } catch (IndexOutOfRangeException) {
            return NotFound($"""Channel "{channelName}" not found.""");
        }
    }
}