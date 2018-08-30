using MoonAndSun.Commons.Mvvm;

namespace DrumBeatDesigner.Models
{
    public class PatternItem : NotificationObject
    {
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value.Equals(_isEnabled))
                {
                    return;
                }
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }
    }
}
