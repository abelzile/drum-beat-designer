using System;
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

        string _name;
        int _beatsPerMinute = 120;
        readonly ObservableCollection<Channel> _channels = new ObservableCollection<Channel>();
        int _numberOfMeasures = 1;
        int _numberOfBeatsPerMeasure = 4;

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

        public int NumberOfMeasures
        {
            get { return _numberOfMeasures; }
            set
            {
                if (value == _numberOfMeasures) return;

                int val = _numberOfMeasures;
                    
                if (value > _numberOfMeasures)
                {
                    _numberOfMeasures = value;

                    while (val < _numberOfMeasures)
                    {
                        AddMeasure();
                        ++val;
                    }
                }
                else
                {
                    _numberOfMeasures = value;

                    while (val > _numberOfMeasures)
                    {
                        RemoveMeasure();
                        --val;
                    }
                }

                RaisePropertyChanged(() => NumberOfMeasures);
            }
        }

        public int NumberOfBeatsPerMeasure
        {
            get { return _numberOfBeatsPerMeasure; }
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

        public void AddChannel(Uri path)
        {
            var channel = new Channel { Path = path };

            for (int i = 0; i < NumberOfMeasures; ++i)
            {
                var measure = new Measure();
                for (int j = 0; j < NumberOfBeatsPerMeasure; ++j)
                {
                    var beat = new Beat();
                    beat.IsEnabled = false;

                    measure.Beats.Add(beat);
                }

                channel.Measures.Add(measure);
            }

            Channels.Add(channel);
        }

        void AddMeasure()
        {
            if (Channels.Count == 0)
                return;

            foreach (var channel in Channels)
            {
                var measure = new Measure();
                for (int i = 0; i < NumberOfBeatsPerMeasure; ++i)
                {
                    var beat = new Beat { IsEnabled = false };
                    measure.Beats.Add(beat);
                }

                channel.Measures.Add(measure);
            }
        }

        void RemoveMeasure()
        {
            if (Channels.Count == 0) 
                return;

            if (NumberOfMeasures == 0) 
                return;

            foreach (var channel in Channels)
            {
                channel.Measures.RemoveAt(channel.Measures.Count - 1);
            }
        }

        void AddBeatPerMeasure()
        {
            if (Channels.Count == 0) 
                return;

            if (NumberOfMeasures == 0) 
                return;

            foreach (var channel in Channels)
            {
                foreach (var measure in channel.Measures)
                {
                    var beat = new Beat { IsEnabled = false };
                    measure.Beats.Add(beat);
                }
            }
        }

        void RemoveBeatPerMeasure()
        {
            if (Channels.Count == 0) 
                return;

            if (NumberOfMeasures == 0) 
                return;

            foreach (var channel in Channels)
            {
                foreach (var measure in channel.Measures)
                {
                    measure.Beats.RemoveAt(measure.Beats.Count - 1);
                }
            }
        }

        public ObservableCollection<Channel> Channels
        {
            get { return _channels; }
        }
    }
}