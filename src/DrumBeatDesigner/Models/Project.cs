using System;
using System.Linq;
using MoonAndSun.Commons.Mvvm;
using Newtonsoft.Json;


namespace DrumBeatDesigner.Models
{
    public class Project : NotificationObject
    {
        public const int DefaultBpm = 120;
        public const int MinBpm = 1;
        public const int MaxBpm = 400;
        public const int MinMeasures = 1;
        public const int MaxMeasures = 8;
        public const int MinBeatsPerMeasure = 1;
        public const int MaxBeatsPerMeasure = 16;

        public const int PatternItemsCount = 20;

        string _name;
        int _beatsPerMinute = 120;
        

        Pattern _selectedPattern;
        
        public ObservableCollection<Pattern> Patterns { get; } = new ObservableCollection<Pattern>();

        public Pattern SelectedPattern
        {
            get
            {
                return _selectedPattern;
            }
            set
            {
                if (value == _selectedPattern)
                {
                    return;
                }
                _selectedPattern = value;
                RaisePropertyChanged(() => SelectedPattern);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        [JsonIgnore]
        public double MinutesPerBeat
        {
            get
            {
                if (BeatsPerMinute == 0)
                    return -1;

                return 1 / (double)BeatsPerMinute;
            }
        }

        public int BeatsPerMinute
        {
            get { return _beatsPerMinute; }
            set
            {
                if (value == _beatsPerMinute) return;
                _beatsPerMinute = value;
                RaisePropertyChanged(() => BeatsPerMinute);
            }
        }

        public void AddPattern(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(name);
            }

            Patterns.Add(new Pattern(name, PatternItemsCount));

            if (SelectedPattern == null)
            {
                SelectedPattern = Patterns.First();
            }
        }
        
    }
}