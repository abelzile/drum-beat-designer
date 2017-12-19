using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Utils;
using NAudio.Wave;


namespace DrumBeatDesigner.Models
{
    public class LoopExporter
    {
        public void Export(Project project, string outputPath, int sampleRate, int bitsPerSample, int channels)
        {
            var streamTracker = new StreamTracker();
            var finalMixer = new WaveMixerStream32();

            try
            {
                foreach (var channel in project.Channels)
                {
                    int audioLen;
                    WaveFormat format;
                    var audio = GetIeeeFloatWaveBytes(channel, out audioLen, out format);

                    int avgBytesPerMs = format.AverageBytesPerSecond / 1000;
                    int msPerBeat = (int)(project.MinutesPerBeat * 60d * 1000d);
                    int beatArrayLen = avgBytesPerMs * msPerBeat;
                
                    var channelMixer = new WaveMixerStream32();
                    streamTracker.AddStream(channelMixer);

                    int i = 0;

                    byte[] silence = new byte[beatArrayLen];

                    foreach (var beat in channel.Measures.SelectMany(measure => measure.Beats))
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
                        
                            channelMixer.AddInputStream(rdr);
                        }

                        ++i;
                    }

                    byte[] channelBytes = new byte[channelMixer.Length];

                    channelMixer.Read(channelBytes, 0, channelBytes.Length);
                
                    var channelMem = new IgnoreDisposeStream(new MemoryStream());
                    streamTracker.AddStream(channelMem);

                    using (var channelWriter = new WaveFileWriter(channelMem, channelMixer.WaveFormat))
                    {
                        channelWriter.Write(channelBytes, 0, channelBytes.Length);
                        channelWriter.Flush();
                    }

                    channelMem.Position = 0;

                    var chnlRdr = new WaveFileReader(channelMem);

                    finalMixer.AddInputStream(chnlRdr);
                }
            
                EnsureLength(project, finalMixer, streamTracker);

                WaveStream finalStream = ConvertTo(finalMixer, sampleRate, bitsPerSample, channels);
                
                byte[] finalBytes = new byte[finalStream.Length];

                finalStream.Read(finalBytes, 0, finalBytes.Length);

                using (var finalWriter = new WaveFileWriter(outputPath, finalStream.WaveFormat))
                {
                    finalWriter.Write(finalBytes, 0, finalBytes.Length);
                    finalWriter.Flush();
                }
            }
            finally
            {
                finalMixer.Dispose();
                streamTracker.Dispose();
            }
        }

        static void EnsureLength(Project project, WaveMixerStream32 finalMixer, StreamTracker streamTracker)
        {
            int avgBytesPerMs = finalMixer.WaveFormat.AverageBytesPerSecond / 1000;
            int msPerBeat = (int)(project.MinutesPerBeat * 60d * 1000d);
            int beatArrayLen = avgBytesPerMs * msPerBeat;

            byte[] silence = new byte[beatArrayLen];

            var mem = new IgnoreDisposeStream(new MemoryStream());
            streamTracker.AddStream(mem);

            using (var writer = new WaveFileWriter(mem, finalMixer.WaveFormat))
            {
                for (int i = 0; i < (project.NumberOfMeasures * project.NumberOfBeatsPerMeasure); ++i)
                {
                    writer.Write(silence, 0, silence.Length);
                    writer.Flush();
                }
            }

            mem.Position = 0;

            var rdr = new WaveFileReader(mem);

            finalMixer.AddInputStream(rdr);
        }

        static byte[] GetIeeeFloatWaveBytes(Channel channel, out int audioLen, out WaveFormat format)
        {
            byte[] audio;

            using (var channelFileReader = new WaveFileReader(channel.Path.LocalPath))
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
            //Works (but may not work on WinXP)
            //return new ResamplerDmoStream(reader, WaveFormat.CreateIeeeFloatWaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels));
            return new ResamplerDmoStream(reader, 
                                          WaveFormat.CreateIeeeFloatWaveFormat(44100, 1), 
                                          (int)Math.Ceiling(reader.TotalTime.TotalMilliseconds));

            //Doesn't work (AcmNotPossible calling acmStreamOpen)
            //return new WaveFormatConversionStream(WaveFormat.CreateIeeeFloatWaveFormat(waveFormat.SampleRate, waveFormat.Channels), reader);
        }

        static WaveStream ConvertTo(WaveStream reader, int sampleRate, int bitsPerSample, int channels)
        {
            if (reader.WaveFormat.SampleRate == sampleRate 
                && reader.WaveFormat.BitsPerSample == bitsPerSample
                && reader.WaveFormat.Channels == channels)
            {
                return reader;
            }

            var format = new WaveFormat(sampleRate, bitsPerSample, channels);

            return new ResamplerDmoStream(reader, format, (int)Math.Ceiling(reader.TotalTime.TotalMilliseconds));
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
                        if (stream is IgnoreDisposeStream)
                        {
                            ((IgnoreDisposeStream)stream).IgnoreDispose = false;
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