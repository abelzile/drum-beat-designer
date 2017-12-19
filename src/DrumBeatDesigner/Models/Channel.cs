using System;
using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner.Models
{
    public class Channel : NotificationObject
    {
        readonly ObservableCollection<Measure> _measures = new ObservableCollection<Measure>();
        bool _isMuted;
        string _name;
        Uri _path;
        float _volume = 1f;

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (value.Equals(_isMuted)) return;
                _isMuted = value;
                RaisePropertyChanged(() => IsMuted);
            }
        }

        public ObservableCollection<Measure> Measures
        {
            get { return _measures; }
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

        public Uri Path
        {
            get { return _path; }
            set
            {
                if (Equals(value, _path)) return;
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
            get { return _volume; }
            set
            {
                if (value.Equals(_volume)) return;
                _volume = value;
                RaisePropertyChanged(() => Volume);
            }
        }
    }
}