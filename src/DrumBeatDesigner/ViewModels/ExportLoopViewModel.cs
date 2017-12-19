using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DrumBeatDesigner.Models;
using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner.ViewModels
{
    public class ExportLoopViewModel : DialogViewModelBase, IDataErrorInfo
    {
        readonly ObservableCollection<int> _bitsPerSample;
        readonly ObservableCollection<int> _channels;
        readonly Dictionary<string, string> _errors = new Dictionary<string, string>();
        readonly ObservableCollection<int> _sampleRates;
        Project _project;
        string _savePath;
        string _savePathExt;
        int _selectedBitsPerSample;
        int _selectedChannels;
        int _selectedSampleRate;

        public ExportLoopViewModel()
        {
            _sampleRates = new ObservableCollection<int> { 22050, 44100, 48000, 96000 };
            _bitsPerSample = new ObservableCollection<int> { 8, 16, 24, 32 };
            _channels = new ObservableCollection<int> { 1, 2 };

            SelectedSampleRate = 44100;
            SelectedBitsPerSample = 16;
            SelectedChannels = 1;
        }

        public ObservableCollection<int> BitsPerSample
        {
            get { return _bitsPerSample; }
        }

        public ObservableCollection<int> Channels
        {
            get { return _channels; }
        }
        public string Error { get; private set; }

        public Project Project
        {
            get { return _project; }
            set
            {
                if (Equals(value, _project)) return;
                _project = value;
                RaisePropertyChanged(() => Project);
            }
        }

        public ObservableCollection<int> SampleRates
        {
            get { return _sampleRates; }
        }

        public string SavePath
        {
            get { return _savePath; }
            set
            {
                if (value == _savePath) return;
                _savePath = value;
                RaisePropertyChanged(() => SavePath);
            }
        }

        public string SavePathExt
        {
            get { return _savePathExt; }
            set
            {
                if (value == _savePathExt) return;
                _savePathExt = value;
                RaisePropertyChanged(() => SavePathExt);
            }
        }

        public int SelectedBitsPerSample
        {
            get { return _selectedBitsPerSample; }
            set
            {
                if (value == _selectedBitsPerSample) return;
                _selectedBitsPerSample = value;
                RaisePropertyChanged(() => SelectedBitsPerSample);
            }
        }

        public int SelectedChannels
        {
            get { return _selectedChannels; }
            set
            {
                if (value == _selectedChannels) return;
                _selectedChannels = value;
                RaisePropertyChanged(() => SelectedChannels);
            }
        }

        public int SelectedSampleRate
        {
            get { return _selectedSampleRate; }
            set
            {
                if (value == _selectedSampleRate) return;
                _selectedSampleRate = value;
                RaisePropertyChanged(() => SelectedSampleRate);
            }
        }

        public string this[string columnName]
        {
            get
            {
                string msg = "";

                _errors.Remove(columnName);

                switch (columnName)
                {
                    case "SavePath":
                        {
                            msg = (string.IsNullOrWhiteSpace(SavePath)) ? "Save Path required." : "";
                            break;
                        }
                }

                if (!string.IsNullOrWhiteSpace(msg))
                {
                    _errors.Add(columnName, msg);
                }

                OkCommand.RaiseCanExecuteChanged();

                return msg;
            }
        }

        protected override bool CanDoOk()
        {
            return !_errors.Any();
        }
    }
}