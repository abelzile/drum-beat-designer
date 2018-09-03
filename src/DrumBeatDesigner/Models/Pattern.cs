using System;
using MoonAndSun.Commons.Mvvm;
using Newtonsoft.Json;

namespace DrumBeatDesigner.Models
{
    public class Pattern : NotificationObject
    {
        string _name;
        int _numberOfBeats = 4;

        public Pattern(string name, int patternItemCount)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            if (patternItemCount < 1)
            {
                throw new ArgumentException("patternItemCount must be greater than 0.", nameof(patternItemCount));
            }

            for (int i = 0; i < patternItemCount; ++i)
            {
                PatternItems.Add(new PatternItem());
            }
        }

        [JsonConstructor]
        public Pattern(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public ObservableCollection<Instrument> Instruments { get; } = new ObservableCollection<Instrument>();

        public ObservableCollection<PatternItem> PatternItems { get; } = new ObservableCollection<PatternItem>();

        public int NumberOfBeats
        {
            get => _numberOfBeats;
            set
            {
                if (value == _numberOfBeats)
                {
                    return;
                }

                int val = _numberOfBeats;
                
                if (value > _numberOfBeats)
                {
                    _numberOfBeats = value;

                    while (val < _numberOfBeats)
                    {
                        AddBeat();
                        ++val;
                    }
                }
                else
                {
                    _numberOfBeats = value;

                    while (val > _numberOfBeats)
                    {
                        RemoveBeat();
                        --val;
                    }
                }

                RaisePropertyChanged(() => NumberOfBeats);
            }
        }

        public void AddInstrument(Uri path)
        {
            var instrument = new Instrument { Path = path };
            
            for (int i = 0; i < NumberOfBeats; ++i)
            {
                instrument.Beats.Add(new Beat { IsEnabled = false });
            }

            Instruments.Add(instrument);
        }

        void AddBeat()
        {
            if (Instruments.Count == 0)
            {
                return;
            }

            foreach (var instrument in Instruments)
            {
                instrument.Beats.Add(new Beat { IsEnabled = false });
            }
        }

        void RemoveBeat()
        {
            if (Instruments.Count == 0)
            {
                return;
            }

            foreach (var instrument in Instruments)
            {
                instrument.Beats.RemoveAt(instrument.Beats.Count - 1);
            }
        }
    }
}
