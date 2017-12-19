using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner.Models
{
    public class Measure : NotificationObject
    {
        readonly ObservableCollection<Beat> _beats = new ObservableCollection<Beat>();

        public ObservableCollection<Beat> Beats
        {
            get { return _beats; }
        }
    }
}