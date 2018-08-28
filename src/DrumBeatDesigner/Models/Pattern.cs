using System;
using MoonAndSun.Commons.Mvvm;

namespace DrumBeatDesigner.Models
{
    public class Pattern : NotificationObject
    {
        string _name;
        int _numberOfBeatsPerMeasure = 4;

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

        public int NumberOfBeatsPerMeasure
        {
            get => _numberOfBeatsPerMeasure;
            set
            {
                if (value == _numberOfBeatsPerMeasure) return;

                int val = _numberOfBeatsPerMeasure;


                if (value > _numberOfBeatsPerMeasure)
                {
                    _numberOfBeatsPerMeasure = value;

                    while (val < _numberOfBeatsPerMeasure)
                    {
                        AddBeatPerMeasure();
                        ++val;
                    }
                }
                else
                {
                    _numberOfBeatsPerMeasure = value;

                    while (val > _numberOfBeatsPerMeasure)
                    {
                        RemoveBeatPerMeasure();
                        --val;
                    }
                }

                RaisePropertyChanged(() => NumberOfBeatsPerMeasure);
            }
        }

        public void AddInstrument(Uri path)
        {
            var instrument = new Instrument { Path = path };
            
            for (int i = 0; i < NumberOfBeatsPerMeasure; ++i)
            {
                instrument.Beats.Add(new Beat { IsEnabled = false });
            }

            Instruments.Add(instrument);
        }

        void AddBeatPerMeasure()
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

        void RemoveBeatPerMeasure()
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
