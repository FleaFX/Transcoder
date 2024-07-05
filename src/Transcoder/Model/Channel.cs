using System.Diagnostics;

namespace Transcoder.Model;

/// <summary>
/// Represents a single channel.
/// </summary>
/// <param name="name">The name of the channel.</param>
/// <param name="uri">The URI where the channel stream can be read.</param>
public sealed class Channel(string name, Uri? uri = null) {
    string Name => name;

    /// <summary>
    /// Starts the transcoding of this channel and returns the <see cref="Stream"/> to read.
    /// </summary>
    /// <returns>A <see cref="Stream"/>.</returns>
    public Stream Transcode() => new TranscodeStream(uri!);

    /// <summary>
    /// Formats the channel as an entry in a M3U playlist.
    /// </summary>
    /// <returns>A <see cref="string"/>.</returns>
    public string AsM3UChannel(Func<string, string> createUrl) =>
        $"""
         #EXTINF:0, {name}
         {createUrl(name)}
         """;

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(Channel? other) {
        if (other is null) return false;
        return ReferenceEquals(this, other) || name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => name.GetHashCode();

    sealed class TranscodeStream : Stream {
        readonly Process _process;

        public TranscodeStream(Uri uri) {
            _process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "ffmpeg",
                    Arguments = $"-i {uri} -c:v copy -c:a copy -f mpegts pipe:1",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _process.Start();
            Task.Run(_process.StandardError.ReadToEnd);
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) return;
            if (!_process.HasExited) {
                _process.StandardInput.WriteLine("q");
                _process.WaitForExit();
            }
            _process.Dispose();
        }

        #region Stream overrides
        public override void Flush() =>
            _process.StandardOutput.BaseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            _process.StandardOutput.BaseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) =>
            _process.StandardOutput.BaseStream.Seek(offset, origin);

        public override void SetLength(long value) =>
            _process.StandardOutput.BaseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) =>
            _process.StandardOutput.BaseStream.Write(buffer, offset, count);

        public override bool CanRead => _process.StandardOutput.BaseStream.CanRead;
        public override bool CanSeek => _process.StandardOutput.BaseStream.CanSeek;
        public override bool CanWrite => _process.StandardOutput.BaseStream.CanWrite;
        public override long Length => _process.StandardOutput.BaseStream.Length;
        public override long Position {
            get => _process.StandardOutput.BaseStream.Position;
            set => _process.StandardOutput.BaseStream.Position = value;
        }
        #endregion
    }
}