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
        public const int MinBeats = 1;
        public const int MaxBeats = 16;
        public const int PatternItemsCount = 400;

        private string _name;
        private int _beatsPerMinute = 120;
        private Pattern _selectedPattern;
        
        public PatternCollection Patterns { get; } = new PatternCollection();

        public Pattern SelectedPattern
        {
            get => _selectedPattern;
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
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }
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
                {
                    return -1;
                }

                return 1 / (double)BeatsPerMinute;
            }
        }

        public int BeatsPerMinute
        {
            get => _beatsPerMinute;
            set
            {
                if (value == _beatsPerMinute)
                {
                    return;
                }
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