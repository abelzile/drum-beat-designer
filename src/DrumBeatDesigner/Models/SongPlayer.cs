using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using MoonAndSun.Commons.Mvvm;

namespace DrumBeatDesigner.Models
{
    public class SongPlayer : NotificationObject, IDisposable
    {
        public static readonly SongPlayer EmptyPlayer = new SongPlayer();

        private readonly int _bpm;
        private readonly IList<PatternPlayer> _players = new List<PatternPlayer>();
        private bool _isPlaying;
        private int _currentIndex;

        public event EventHandler Stopped;

        public SongPlayer(IList<Pattern> patterns, int bpm)
        {
            _bpm = bpm;

            int maxPatternItemIndex = GetMaxPatternItemIndex(patterns);

            Instrument nullInstrument = CreateNullInstrument(patterns);

            for (int i = 0; i <= maxPatternItemIndex; ++i)
            {
                var instruments = new List<Instrument>();

                foreach (var pattern in patterns)
                {
                    if (pattern.PatternItems[i].IsEnabled)
                    {
                        instruments.AddRange(pattern.Instruments);
                    }
                }

                if (instruments.Count == 0)
                {
                    instruments.Add(nullInstrument);
                }

                _players.Add(new PatternPlayer(instruments, _bpm));
            }
        }

        private SongPlayer()
        {
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

        public void Play()
        {
            Stop();

            _currentIndex = 0;

            IsPlaying = true;

            _players[_currentIndex].AllInstrumentsDone += OnAllInstrumentsDone;
            _players[_currentIndex].Play();
        }

        public void Stop()
        {
            foreach (var player in _players)
            {
                player.Stop();
            }

            IsPlaying = false;

            Stopped?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _players.ForEach(p => p.Dispose());
        }

        private void OnAllInstrumentsDone(object sender, EventArgs e)
        {
            _players[_currentIndex].AllInstrumentsDone -= OnAllInstrumentsDone;

            ++_currentIndex;

            if (_currentIndex >= _players.Count)
            {
                Stop();

                return;
            }

            _players[_currentIndex].AllInstrumentsDone += OnAllInstrumentsDone;
            _players[_currentIndex].Play();
        }

        private static int GetMaxPatternItemIndex(IList<Pattern> patterns)
        {
            int startPatternItemIndex = patterns[0].PatternItems.Count - 1;
            int maxPatternItemIndex = 0;

            foreach (var pattern in patterns)
            {
                for (int i = startPatternItemIndex; i-- > 0;)
                {
                    if (pattern.PatternItems[i].IsEnabled)
                    {
                        if (i > maxPatternItemIndex)
                        {
                            maxPatternItemIndex = i;
                        }

                        break;
                    }
                }
            }

            return maxPatternItemIndex;
        }

        private static Instrument CreateNullInstrument(IEnumerable<Pattern> patterns)
        {
            int maxBeats = 0;
            Uri tempPath = null;

            foreach (var pattern in patterns)
            {
                foreach (var instrument in pattern.Instruments)
                {
                    if (instrument.Beats.Count > maxBeats)
                    {
                        maxBeats = instrument.Beats.Count;
                        tempPath = instrument.Path;
                    }
                }
            }

            var nullInstrument = new Instrument
            {
                Path = tempPath,
                Name = "Null Instrument"
            };

            for (int i = 0; i < maxBeats; ++i)
            {
                nullInstrument.Beats.Add(new Beat { IsEnabled = false });
            }

            return nullInstrument;
        }
    }
}
