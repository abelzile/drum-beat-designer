using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner.Models
{
    public class Beat : NotificationObject
    {
        bool _isEnabled;
        bool _isPlaying;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value.Equals(_isEnabled)) return;
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (value.Equals(_isPlaying)) return;
                _isPlaying = value;
                RaisePropertyChanged(() => IsPlaying);
            }
        }
    }
}