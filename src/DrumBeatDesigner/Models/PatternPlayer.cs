using System;
using System.Collections.Generic;
using System.Linq;
using HighPrecisionTimer;
using Microsoft.Practices.ObjectBuilder2;
using MoonAndSun.Commons.Mvvm;
using SharpDX.XAudio2;

namespace DrumBeatDesigner.Models
{
    public class PatternPlayer : NotificationObject, IDisposable
    {
        public static readonly PatternPlayer EmptyPlayer = new PatternPlayer();

        private readonly IDictionary<Instrument, InstrumentState> _instrumentStates = new Dictionary<Instrument, InstrumentState>();
        private readonly MultimediaTimer _timer;
        private readonly MasteringVoice _masteringVoice;
        private readonly XAudio2 _xAudio2 = new XAudio2();
        private readonly int _bpm;
        private readonly bool _loop;
        private bool _isPlaying;

        public event EventHandler Stopped;
        public event EventHandler AllInstrumentsDone;

        public PatternPlayer(IEnumerable<Instrument> instruments, int bpm, MultimediaTimer timer, bool loop)
        {
            _masteringVoice = new MasteringVoice(_xAudio2);

            _bpm = bpm;
            _loop = loop;
            foreach (Instrument instrument in instruments)
            {
                _instrumentStates.Add(instrument, new InstrumentState(new Sound(instrument, _xAudio2), instrument.Beats.Count));
            }
            
            double interval = (60d / _bpm) * 1000d;

            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _timer.Interval = (int)interval;
            _timer.Elapsed += OnElapsed;
        }

        public PatternPlayer(IList<Instrument> instruments, int bpm, bool loop = false) : this(instruments, bpm, CreateNewTimer(), loop)
        {
        }

        private PatternPlayer()
        {
        }

        public static MultimediaTimer CreateNewTimer()
        {
            return new MultimediaTimer { Resolution = 0 };
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (value.Equals(_isPlaying))
                {
                    return;
                }
                _isPlaying = value;
                RaisePropertyChanged(() => IsPlaying);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Stop();

            foreach (var instrumentState in _instrumentStates)
            {
                instrumentState.Value.Sound.Dispose();
            }

            _instrumentStates.Clear();

            _masteringVoice?.Dispose();
            _xAudio2?.Dispose();
        }

        public void Play()
        {
            ResetIndexes();

            IsPlaying = true;

            if (!_timer.IsRunning)
            {
                _timer.Start();
            }

            PlayInstruments();
        }

        public void Stop()
        {
            ResetIndexes();

            IsPlaying = false;

            if (_timer.IsRunning)
            {
                _timer.Stop();
            }

            StopInstruments();

            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void OnElapsed(object sender, EventArgs e)
        {
            PlayInstruments();
        }

        private void PlayInstruments()
        {
            if (AreAllInstrumentsDone())
            {
                AllInstrumentsDone?.Invoke(this, EventArgs.Empty);

                if (!_loop)
                {
                    Stop();

                    return;
                }

                ResetIndexes();
            }

            foreach (var instrumentState in _instrumentStates)
            {
                Instrument instrument = instrumentState.Key;
                InstrumentState state = instrumentState.Value;

                int prevBeatIndex = state.BeatIndex - 1;
                if (prevBeatIndex < 0)
                {
                    prevBeatIndex = state.BeatCount - 1;
                }

                instrument.Beats[prevBeatIndex].IsPlaying = false;

                if (state.BeatIndex < state.BeatCount)
                {
                    Beat beat = instrument.Beats[state.BeatIndex];
                    beat.IsPlaying = true;

                    if (!instrument.IsMuted && beat.IsEnabled)
                    {
                        state.Sound.Play();
                    }

                    state.BeatIndex++;
                }
            }
        }

        private void StopInstruments()
        {
            foreach (var instrumentState in _instrumentStates)
            {
                instrumentState.Value.Sound.Stop();
            }
        }

        private bool AreAllInstrumentsDone()
        {
            return _instrumentStates.All(state => state.Value.BeatIndex >= state.Value.BeatCount);
        }

        private void ResetIndexes()
        {
            foreach (var instrumentState in _instrumentStates)
            {
                instrumentState.Key.Beats.ForEach(b => b.IsPlaying = false);
                instrumentState.Value.BeatIndex = 0;
            }
        }

        private class InstrumentState
        {
            public InstrumentState(Sound sound, int beatCount)
            {
                Sound = sound ?? throw new ArgumentNullException(nameof(sound));
                BeatCount = beatCount;
            }

            public Sound Sound { get; }
            public int BeatCount { get; }
            public int BeatIndex;
        }
    }
}