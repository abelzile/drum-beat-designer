using NAudio.Utils;
using NAudio.Wave;
using System.IO;


namespace DrumBeatDesigner.Models
{
    public class PatternExporter
    {
        public void Export(Pattern pattern, int bpm, string outputPath, int sampleRate, int bitsPerSample, int channels)
        {
            using (var streamTracker = new StreamTracker())
            {
                var finalMixer = new WaveMixerStream32();
                streamTracker.AddStream(finalMixer);

                AddInstrumentStreamsToMixer(pattern, bpm, finalMixer, streamTracker);

                WaveStream finalStream = ConvertTo(finalMixer, sampleRate, bitsPerSample, channels);
                streamTracker.AddStream(finalStream);

                WaveFileWriter.CreateWaveFile(outputPath, finalStream);
            }
        }

        private static void AddInstrumentStreamsToMixer(Pattern pattern, int bpm, WaveMixerStream32 finalMixer, StreamTracker streamTracker)
        {
            double minutesPerBeat = 1d / (double) bpm;
            int msPerBeat = (int) (minutesPerBeat * 60d * 1000d);

            foreach (var instrument in pattern.Instruments)
            {
                var audio = GetIeeeFloatWaveBytes(instrument, out int audioLen, out WaveFormat format);

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
                            // for each beat of the instrument after the first, pad the stream with silence before the beat position, eg:
                            // beat
                            // silence | beat
                            // silence | silence | beat
                            // etc., then mix all the streams together
                            for (int j = 0; j < i; ++j)
                            {
                                writer.Write(silence, 0, silence.Length);
                            }

                            writer.Write(audio, 0, audioLen);
                            writer.Flush();
                        }

                        mem.Position = 0;

                        instrumentMixer.AddInputStream(new WaveFileReader(mem));
                    }

                    ++i;
                }

                var instrumentStream = new IgnoreDisposeStream(new MemoryStream());
                streamTracker.AddStream(instrumentStream);

                WaveFileWriter.WriteWavFileToStream(instrumentStream, instrumentMixer);

                instrumentStream.Position = 0;

                var instrumentReader = new WaveFileReader(instrumentStream);

                finalMixer.AddInputStream(instrumentReader);
            }

            EnsureLength(pattern, msPerBeat, finalMixer, streamTracker);
        }

        private static void EnsureLength(Pattern pattern, int msPerBeat, WaveMixerStream32 finalMixer, StreamTracker streamTracker)
        {
            int avgBytesPerMs = finalMixer.WaveFormat.AverageBytesPerSecond / 1000;
            int beatArrayLen = avgBytesPerMs * msPerBeat;
            var silence = new byte[beatArrayLen];
            var mem = new IgnoreDisposeStream(new MemoryStream());

            streamTracker.AddStream(mem);

            using (var writer = new WaveFileWriter(mem, finalMixer.WaveFormat))
            {
                for (int i = 0; i < pattern.NumberOfBeats; ++i)
                {
                    writer.Write(silence, 0, silence.Length);
                    writer.Flush();
                }
            }

            mem.Position = 0;

            var rdr = new WaveFileReader(mem);

            finalMixer.AddInputStream(rdr);
        }

        private static byte[] GetIeeeFloatWaveBytes(Instrument instrument, out int audioLen, out WaveFormat format)
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

        private static WaveStream ConvertToIeeeFloatWave(WaveStream reader)
        {
            var ieeeOutFormat = WaveFormat.CreateIeeeFloatWaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);

            return ConvertTo(reader, ieeeOutFormat);
        }

        private static WaveStream ConvertTo(WaveStream reader, int sampleRate, int bitsPerSample, int channels)
        {
            var outFormat = new WaveFormat(sampleRate, bitsPerSample, channels);

            return ConvertTo(reader, outFormat);
        }

        private static WaveStream ConvertTo(WaveStream reader, WaveFormat format)
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
    }
}