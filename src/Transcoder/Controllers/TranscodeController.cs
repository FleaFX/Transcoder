using System.Buffers;
using System.Net;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using Transcoder.Model;

namespace Transcoder.Controllers;

[ApiController]
[Route("/transcode")]
public class TranscodeController(IConfiguration configuration) : ControllerBase {
    readonly Playlist _playlist =
        Playlist.Parse(System.IO.File.ReadAllText(configuration.GetValue<string>(nameof(Playlist))!));

    /// <summary>
    /// Get the list of available channels as an M3U playlist file.
    /// </summary>
    /// <returns>A <see cref="IActionResult"/>.</returns>
    [HttpGet("/")]
    public IActionResult Get() =>
        new ContentResult {
            Content = _playlist.GetM3U(name => $"{Request.Scheme}://{Request.Host}{Url.Action("Get", "Transcode", new { channelName = name })}"),
            ContentType = "application/x-mpegurl",
            StatusCode = (int)HttpStatusCode.OK
        };

    /// <summary>
    /// Tune into the channel with the requested name and transcode the video stream.
    /// </summary>
    /// <param name="channelName">The name of the channel to tune into.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [HttpGet("{channelName}")]
    public async Task<IActionResult> Get(string channelName, CancellationToken cancellationToken) {
        try {
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";
            Response.Headers.ContentType = "video/mp2t";

            await using var stream = _playlist[channelName].Transcode();
            using var owner = MemoryPool<byte>.Shared.Rent(1024);
            while (!cancellationToken.IsCancellationRequested) {
                if (await stream.ReadAsync(owner.Memory, cancellationToken) > 0) {
                    await Response.Body.WriteAsync(owner.Memory, cancellationToken);
                }
            }
            
            return Empty;
        } catch (IndexOutOfRangeException) {
            return NotFound($"""Channel "{channelName}" not found.""");
        }
    }
}