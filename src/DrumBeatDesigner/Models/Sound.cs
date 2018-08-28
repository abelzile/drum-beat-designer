using System;
using System.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;


namespace DrumBeatDesigner.Models
{
    public class Sound : IDisposable
    {
        public Sound(AudioBuffer audioBuffer, SourceVoice sourceVoice, uint[] decodedPacketsInfo, float volume)
        {
            Setup(audioBuffer, sourceVoice, decodedPacketsInfo, volume);
        }

        public Sound(Instrument instrument, XAudio2 xAudio2)
        {
            using (var stream = new SoundStream(File.OpenRead(instrument.Path.LocalPath)))
            {
                WaveFormat waveFormat = stream.Format;
                var buffer = new AudioBuffer
                {
                    Stream = stream.ToDataStream(),
                    AudioBytes = (int)stream.Length,
                    Flags = BufferFlags.EndOfStream
                };

                var sourceVoice = new SourceVoice(xAudio2, waveFormat);
                sourceVoice.SubmitSourceBuffer(buffer, stream.DecodedPacketsInfo);

                Setup(buffer, sourceVoice, stream.DecodedPacketsInfo, instrument.Volume);
            }
        }

        public AudioBuffer AudioBuffer { get; private set; }

        public uint[] DecodedPacketsInfo { get; private set; }

        public SourceVoice SourceVoice { get; private set; }

        public float Volume { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Setup(AudioBuffer audioBuffer, SourceVoice sourceVoice, uint[] decodedPacketsInfo, float volume)
        {
            if (!(0f <= volume && volume <= 1f))
            {
                throw new ArgumentOutOfRangeException(nameof(volume), volume, "Volume must be between 0 and 1.0.");
            }

            AudioBuffer = audioBuffer ?? throw new ArgumentNullException(nameof(audioBuffer));
            SourceVoice = sourceVoice ?? throw new ArgumentNullException(nameof(sourceVoice));
            DecodedPacketsInfo = decodedPacketsInfo;
            Volume = volume;
        }

        public void Play()
        {
            Stop();

            AudioBuffer.Stream.Position = 0;

            SourceVoice.SubmitSourceBuffer(AudioBuffer, DecodedPacketsInfo);
            SourceVoice.SetVolume(Volume);
            SourceVoice.Start();
        }

        public void Stop()
        {
            SourceVoice.Stop();
            SourceVoice.FlushSourceBuffers();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                if (SourceVoice != null)
                {
                    SourceVoice.DestroyVoice();
                    SourceVoice.Dispose();
                }

                if (AudioBuffer != null)
                {
                    AudioBuffer.Stream.Dispose();
                }
            }
        }
    }
}