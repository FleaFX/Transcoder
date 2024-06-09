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
            // tune into the channel and create the transcoding stream
            var stream = _playlist[channelName].Transcode();

            // make sure that the stream is disposed when the client disconnects
            HttpContext.RequestAborted.Register(stream.Dispose);

            return new FileStreamResult(stream, "video/mp2t");
        } catch (IndexOutOfRangeException) {
            return NotFound($"""Channel "{channelName}" not found.""");
        }
    }
}