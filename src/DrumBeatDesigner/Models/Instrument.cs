using System;
using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner.Models
{
    public class Instrument : NotificationObject
    {
        bool _isMuted;
        string _name;
        Uri _path;
        float _volume = 1f;

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (value.Equals(_isMuted))
                {
                    return;
                }
                _isMuted = value;
                RaisePropertyChanged(() => IsMuted);
            }
        }

        public ObservableCollection<Beat> Beats { get; } = new ObservableCollection<Beat>();

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

        public Uri Path
        {
            get => _path;
            set
            {
                if (Equals(value, _path))
                {
                    return;
                }
                _path = value;
                RaisePropertyChanged(() => Path);

                if (string.IsNullOrEmpty(Name))
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(_path.ToString());
                }
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                if (value.Equals(_volume))
                {
                    return;
                }
                _volume = value;
                RaisePropertyChanged(() => Volume);
            }
        }

        public Instrument Clone()
        {
            var instrument = new Instrument {Name = Name, Path = Path, IsMuted = IsMuted, Volume = Volume};

            foreach (var beat in Beats)
            {
                instrument.Beats.Add(beat.Clone());
            }
            
            return instrument;
        }
    }
}