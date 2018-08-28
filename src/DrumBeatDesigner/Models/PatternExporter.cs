using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Utils;
using NAudio.Wave;


namespace DrumBeatDesigner.Models
{
    public class PatternExporter
    {
        public void Export(Pattern pattern, int bpm, string outputPath, int sampleRate, int bitsPerSample, int channels)
        {
            var streamTracker = new StreamTracker();
            var finalMixer = new WaveMixerStream32();
            streamTracker.AddStream(finalMixer);

            try
            {
                double minutesPerBeat = 1 / (double)bpm;
                int msPerBeat = (int)(minutesPerBeat * 60d * 1000d);
                    
                foreach (var instrument in pattern.Instruments)
                {
                    var audio = GetIeeeFloatWaveBytes(instrument, out var audioLen, out var format);

                    int avgBytesPerMs = format.AverageBytesPerSecond / 1000;
                    int beatArrayLen = avgBytesPerMs * msPerBeat;

                    var instrumentMixer = new WaveMixerStream32();
                    streamTracker.AddStream(instrumentMixer);

                    int i = 0;

                    var silence = new byte[beatArrayLen];

                    foreach (var beat in instrument.Beats)
                    {
                        if (beat.IsEnabled)
                        {
                            var mem = new IgnoreDisposeStream(new MemoryStream());
                            streamTracker.AddStream(mem);

                            using (var writer = new WaveFileWriter(mem, format))
                            {
                                for (int j = 0; j < i; ++j)
                                {
                                    writer.Write(silence, 0, silence.Length);
                                }

                                writer.Write(audio, 0, audioLen);
                                writer.Flush();
                            }

                            mem.Position = 0;

                            var rdr = new WaveFileReader(mem);

                            instrumentMixer.AddInputStream(rdr);
                        }

                        ++i;
                    }

                    var instrumentBytes = new byte[instrumentMixer.Length];

                    instrumentMixer.Read(instrumentBytes, 0, instrumentBytes.Length);

                    var instrumentStream = new IgnoreDisposeStream(new MemoryStream());
                    streamTracker.AddStream(instrumentStream);

                    using (var instrumentWriter = new WaveFileWriter(instrumentStream, instrumentMixer.WaveFormat))
                    {
                        instrumentWriter.Write(instrumentBytes, 0, instrumentBytes.Length);
                        instrumentWriter.Flush();
                    }

                    instrumentStream.Position = 0;

                    var instrumentReader = new WaveFileReader(instrumentStream);

                    finalMixer.AddInputStream(instrumentReader);
                }

                EnsureLength(pattern, msPerBeat, finalMixer, streamTracker);

                WaveStream finalStream = ConvertTo(finalMixer, sampleRate, bitsPerSample, channels);
                streamTracker.AddStream(finalStream);

                var finalBytes = new byte[finalStream.Length];

                finalStream.Read(finalBytes, 0, finalBytes.Length);

                using (var finalWriter = new WaveFileWriter(outputPath, finalStream.WaveFormat))
                {
                    finalWriter.Write(finalBytes, 0, finalBytes.Length);
                    finalWriter.Flush();
                }
            }
            finally
            {
                streamTracker.Dispose();
            }
        }

        static void EnsureLength(Pattern pattern, int msPerBeat, WaveMixerStream32 finalMixer, StreamTracker streamTracker)
        {
            int avgBytesPerMs = finalMixer.WaveFormat.AverageBytesPerSecond / 1000;
            int beatArrayLen = avgBytesPerMs * msPerBeat;
            var silence = new byte[beatArrayLen];
            var mem = new IgnoreDisposeStream(new MemoryStream());

            streamTracker.AddStream(mem);

            using (var writer = new WaveFileWriter(mem, finalMixer.WaveFormat))
            {
                for (int i = 0; i < pattern.NumberOfBeatsPerMeasure; ++i)
                {
                    writer.Write(silence, 0, silence.Length);
                    writer.Flush();
                }
            }

            mem.Position = 0;

            var rdr = new WaveFileReader(mem);

            finalMixer.AddInputStream(rdr);
        }

        static byte[] GetIeeeFloatWaveBytes(Instrument instrument, out int audioLen, out WaveFormat format)
        {
            byte[] audio;

            using (var channelFileReader = new WaveFileReader(instrument.Path.LocalPath))
            using (var conversionReader = ConvertToIeeeFloatWave(channelFileReader))
            {
                audio = new byte[conversionReader.Length];
                audioLen = conversionReader.Read(audio, 0, audio.Length);
                format = conversionReader.WaveFormat;
            }

            return audio;
        }

        static WaveStream ConvertToIeeeFloatWave(WaveStream reader)
        {
            var ieeeOutFormat = WaveFormat.CreateIeeeFloatWaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);

            return ConvertTo(reader, ieeeOutFormat);
        }

        static WaveStream ConvertTo(WaveStream reader, int sampleRate, int bitsPerSample, int channels)
        {
            var outFormat = new WaveFormat(sampleRate, bitsPerSample, channels);

            return ConvertTo(reader, outFormat);
        }

        static WaveStream ConvertTo(WaveStream reader, WaveFormat format)
        {
            if (reader.WaveFormat.Equals(format))
            {
                return reader;
            }

            var resampler = new MediaFoundationResampler(reader, format);
            var memStream = new MemoryStream();

            WaveFileWriter.WriteWavFileToStream(memStream, resampler);
            memStream.Position = 0;

            return new WaveFileReader(memStream);
        }

        class StreamTracker : IDisposable
        {
            IList<Stream> _streams = new List<Stream>();
        
            public void AddStream(Stream stream)
            {
                _streams.Add(stream);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    for (int i = _streams.Count; i-- > 0;)
                    {
                        var stream = _streams[i];
                        if (stream is IgnoreDisposeStream disposeStream)
                        {
                            disposeStream.IgnoreDispose = false;
                        }
                        stream.Dispose();
                    
                        _streams.RemoveAt(i);
                    }
                    _streams = null;
                }
            }
        }
    }
}