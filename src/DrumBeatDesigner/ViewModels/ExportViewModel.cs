using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DrumBeatDesigner.Models;
using MoonAndSun.Commons.Mvvm;


namespace DrumBeatDesigner.ViewModels
{
    public class ExportViewModel : DialogViewModelBase, IDataErrorInfo
    {
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();
        private Project _project;
        private string _savePath;
        private string _savePathExt;
        private int _selectedBitsPerSample;
        private int _selectedChannels;
        private int _selectedSampleRate;

        public ExportViewModel()
        {
            SampleRates = new ObservableCollection<int> { 22050, 44100, 48000, 96000 };
            BitsPerSample = new ObservableCollection<int> { 8, 16, 24, 32 };
            Channels = new ObservableCollection<int> { 1, 2 };

            SelectedSampleRate = 44100;
            SelectedBitsPerSample = 16;
            SelectedChannels = 1;
        }

        public ObservableCollection<int> BitsPerSample { get; }

        public ObservableCollection<int> Channels { get; }

        public string Error { get; private set; }

        public Project Project
        {
            get => _project;
            set
            {
                if (Equals(value, _project))
                {
                    return;
                }
                _project = value;
                RaisePropertyChanged(() => Project);
            }
        }

        public ObservableCollection<int> SampleRates { get; }

        public string SavePath
        {
            get => _savePath;
            set
            {
                if (value == _savePath)
                {
                    return;
                }
                _savePath = value;
                RaisePropertyChanged(() => SavePath);
            }
        }

        public string SavePathExt
        {
            get => _savePathExt;
            set
            {
                if (value == _savePathExt)
                {
                    return;
                }
                _savePathExt = value;
                RaisePropertyChanged(() => SavePathExt);
            }
        }

        public int SelectedBitsPerSample
        {
            get => _selectedBitsPerSample;
            set
            {
                if (value == _selectedBitsPerSample)
                {
                    return;
                }
                _selectedBitsPerSample = value;
                RaisePropertyChanged(() => SelectedBitsPerSample);
            }
        }

        public int SelectedChannels
        {
            get => _selectedChannels;
            set
            {
                if (value == _selectedChannels)
                {
                    return;
                }
                _selectedChannels = value;
                RaisePropertyChanged(() => SelectedChannels);
            }
        }

        public int SelectedSampleRate
        {
            get => _selectedSampleRate;
            set
            {
                if (value == _selectedSampleRate)
                {
                    return;
                }
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