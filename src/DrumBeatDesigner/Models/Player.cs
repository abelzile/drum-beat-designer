using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using MoonAndSun.Commons.Mvvm;
using SharpDX.Multimedia;
using SharpDX.XAudio2;


namespace DrumBeatDesigner.Models
{
    public class Player : NotificationObject, IDisposable
    {
        public static readonly Player EmptyPlayer = new Player();
        readonly IList<Channel> _channels;
        int _bpm;
        bool _isPlaying;
        MasteringVoice _masteringVoice;
        int _prevBeatIndex;
        int _prevMeasureIndex;
        IDictionary<Channel, Sound> _soundItems = new Dictionary<Channel, Sound>();
        Timer _timer;
        XAudio2 _xaudio2;
        
        public Player(IList<Channel> channels, int bpm)
        {
            _channels = channels;
            _xaudio2 = new XAudio2();
            _masteringVoice = new MasteringVoice(_xaudio2);

            foreach (Channel channel in channels)
            {
                AddSound(channel);
            }

            Bpm = bpm;
        }

        Player()
        {
            _channels = new List<Channel>();
        }

        public int BeatIndex { get; private set; }

        public int Bpm
        {
            get { return _bpm; }
            private set // think about allowing public.
            {
                if (value == _bpm) return;
                _bpm = value;
                RaisePropertyChanged(() => Bpm);
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set
            {
                if (value.Equals(_isPlaying)) return;
                _isPlaying = value;
                RaisePropertyChanged(() => IsPlaying);
            }
        }

        public int MeasureIndex { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Play()
        {
            ResetIndexes();

            IsPlaying = true;

            double interval = (60d / _bpm) * 1000d;

            _timer = new Timer(interval) { AutoReset = true };
            _timer.Elapsed += OnElapsed;
            _timer.Start();
        }

        public void Stop()
        {
            IsPlaying = false;

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            ResetIndexes();

            foreach (Channel channel in _channels)
            {
                _soundItems[channel].Stop();
            }
        }

        Sound AddSound(Channel channel)
        {
            using (var stream = new SoundStream(File.OpenRead(channel.Path.LocalPath)))
            {
                WaveFormat waveFormat = stream.Format;
                var buffer = new AudioBuffer
                    {
                        Stream = stream.ToDataStream(),
                        AudioBytes = (int)stream.Length,
                        Flags = BufferFlags.EndOfStream
                    };

                var sourceVoice = new SourceVoice(_xaudio2, waveFormat);
                sourceVoice.SubmitSourceBuffer(buffer, stream.DecodedPacketsInfo);

                var sound = new Sound(buffer, sourceVoice, stream.DecodedPacketsInfo, channel.Volume);
                _soundItems.Add(channel, sound);

                return sound;
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                foreach (var soundItem in _soundItems)
                {
                    soundItem.Value.Dispose();
                }

                _soundItems.Clear();
                _soundItems = null;

                if (_masteringVoice != null)
                {
                    _masteringVoice.Dispose();
                    _masteringVoice = null;
                }

                if (_xaudio2 != null)
                {
                    _xaudio2.Dispose();
                    _xaudio2 = null;
                }
            }
        }

        void OnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (Channel channel in _channels)
            {
                channel.Measures[_prevMeasureIndex].Beats[_prevBeatIndex].IsPlaying = false;

                if (!channel.IsMuted)
                {
                    Beat beat = channel.Measures[MeasureIndex].Beats[BeatIndex];
                    beat.IsPlaying = true;

                    if (beat.IsEnabled)
                    {
                        Sound sound;

                        if (!_soundItems.TryGetValue(channel, out sound))
                        {
                            sound = AddSound(channel);
                        }

                        sound.Play();
                    }
                }
            }

            _prevBeatIndex = BeatIndex;
            _prevMeasureIndex = MeasureIndex;

            if (_channels.Count > 0)
            {
                ++BeatIndex;

                if (BeatIndex >= _channels[0].Measures[0].Beats.Count)
                {
                    BeatIndex = 0;

                    ++MeasureIndex;

                    if (MeasureIndex >= _channels[0].Measures.Count)
                    {
                        MeasureIndex = 0;
                    }
                }
            }
        }

        void ResetIndexes()
        {
            foreach (var channel in _channels)
            {
                channel.Measures[_prevMeasureIndex].Beats[_prevBeatIndex].IsPlaying = false;
                channel.Measures[MeasureIndex].Beats[BeatIndex].IsPlaying = false;
            }

            MeasureIndex = 0;
            BeatIndex = 0;
            _prevMeasureIndex = 0;
            _prevBeatIndex = 0;
        }

        class Sound : IDisposable
        {
            readonly AudioBuffer _audioBuffer;
            readonly uint[] _decodedPacketsInfo;
            readonly SourceVoice _sourceVoice;
            readonly float _volume;

            public Sound(AudioBuffer audioBuffer, SourceVoice sourceVoice, uint[] decodedPacketsInfo, float volume)
            {
                if (audioBuffer == null) throw new ArgumentNullException("audioBuffer");
                if (sourceVoice == null) throw new ArgumentNullException("sourceVoice");
                if (!(0f <= volume && volume <= 1f)) throw new ArgumentOutOfRangeException("volume", volume, "Volume must be between 0 and 1.0.");

                _audioBuffer = audioBuffer;
                _sourceVoice = sourceVoice;
                _decodedPacketsInfo = decodedPacketsInfo;
                _volume = volume;
            }

            public AudioBuffer AudioBuffer
            {
                get { return _audioBuffer; }
            }

            public uint[] DecodedPacketsInfo
            {
                get { return _decodedPacketsInfo; }
            }

            public SourceVoice SourceVoice
            {
                get { return _sourceVoice; }
            }

            public float Volume
            {
                get { return _volume; }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
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

            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Stop();

                    if (_sourceVoice != null)
                    {
                        _sourceVoice.DestroyVoice();
                        _sourceVoice.Dispose();
                    }

                    if (_audioBuffer != null)
                    {
                        _audioBuffer.Stream.Dispose();
                    }
                }
            }
        }
    }
}