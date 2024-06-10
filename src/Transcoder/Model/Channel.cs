using System.Diagnostics;

namespace Transcoder.Model;

/// <summary>
/// Represents a single channel.
/// </summary>
/// <param name="name">The name of the channel.</param>
/// <param name="uri">The URI where the channel stream can be read.</param>
public sealed class Channel(string name, Uri? uri = null) {
    string Name => name;

    public Stream Transcode() => new TranscodeStream(uri!);

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
            _process = new Process() {
                StartInfo = new ProcessStartInfo {
                    FileName = "ffmpeg",
                    Arguments = $"-i {uri} -f mpegts -codec:v mpeg1video -codec:a mp2 -", // TODO: find correct settings
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _process.Start();
        }

        protected override void Dispose(bool disposing) {
            if (!disposing) return;
            if (!_process.HasExited)
                _process.Kill();
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