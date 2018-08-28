using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using DrumBeatDesigner.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Win32;
using MoonAndSun.Commons.Mvvm;
using MoonAndSun.Commons.OSServices;
using MoonAndSun.Commons.Windows;


namespace DrumBeatDesigner.ViewModels
{
    public class MainViewModel : NotificationObject
    {
        private readonly CommandHelper _cmdHelper;
        private readonly TimeSpan _span = new TimeSpan(0, 0, 0, 1);
        private readonly DispatcherTimer _timer;
        private readonly ISoundFileValidator _soundFileValidator;
        private readonly IOpenFileDialogService _openFileDialogService;
        private TimeSpan _playTime;
        private PatternPlayer _patternPlayer;
        private SongPlayer _songPlayer;
        private Uri _savePath;
        private Project _selectedProject;

        public MainViewModel(ISoundFileValidator soundFileValidator, IOpenFileDialogService openFileDialogService)
        {
            _soundFileValidator = soundFileValidator;
            _openFileDialogService = openFileDialogService;
            _cmdHelper = new CommandHelper(this);

            _timer = new DispatcherTimer();
            _timer.Tick += (sender, args) => { PlayTime = PlayTime.Add(_span); };
            _timer.Interval = _span;

            NewProjectCommand = new DelegateCommand(NewProject, CanNewProject);
            OpenProjectCommand = new DelegateCommand(OpenProject, CanOpenProject);
            SaveProjectCommand = new DelegateCommand(SaveProject, CanSaveProject);
            AddInstrumentCommand = new DelegateCommand(AddInstrument, CanAddInstrument);
            PlayOrStopPatternCommand = new DelegateCommand(PlayOrStopPattern, CanPlayOrStopPattern);
            PlayOrStopSongCommand = new DelegateCommand(PlayOrStopSong, CanPlayOrStopSong);
            DeleteInstrumentCommand = new DelegateCommand<Instrument>(DeleteInstrument, CanDeleteInstrument);
            ExportPatternCommand = new DelegateCommand(ExportPattern, CanExportPattern);
            AddPatternCommand = new DelegateCommand(AddPattern, CanAddPattern);

            PropertyChanged += ThisPropertyChanged;
        }

        public MainViewModel()
            : this(new SoundFileValidator(), new OpenFileDialogService())
        {
        }

        public int Bpm
        {
            get
            {
                if (SelectedProject == null)
                {
                    return Project.DefaultBpm;
                }
                return SelectedProject.BeatsPerMinute;
            }
            set
            {
                if (SelectedProject == null)
                {
                    //throw new Exception("No current project.");
                    return;
                }
                SelectedProject.BeatsPerMinute = value;
                RaisePropertyChanged(() => Bpm);
            }
        }

        public bool AreAnyPlayersPlaying => PatternPlayer.IsPlaying || SongPlayer.IsPlaying;

        public bool CanChangeBpm => SelectedProject != null && !PatternPlayer.IsPlaying;

        public bool CanChangeMeasuresAndBeats => SelectedProject != null && !PatternPlayer.IsPlaying;

        public DelegateCommand AddInstrumentCommand { get; private set; }
        public DelegateCommand<Instrument> DeleteInstrumentCommand { get; private set; }
        public DelegateCommand ExportPatternCommand { get; private set; }
        public DelegateCommand AddPatternCommand { get; private set; }
        public DelegateCommand NewProjectCommand { get; private set; }
        public DelegateCommand OpenProjectCommand { get; private set; }
        public DelegateCommand PlayOrStopPatternCommand { get; private set; }
        public DelegateCommand PlayOrStopSongCommand { get; private set; }
        public DelegateCommand SaveProjectCommand { get; private set; }

        public TimeSpan PlayTime
        {
            get => _playTime;
            set
            {
                if (Equals(value, _playTime)) return;
                _playTime = value;
                RaisePropertyChanged(() => PlayTime);
            }
        }

        public PatternPlayer PatternPlayer
        {
            get => _patternPlayer ?? (_patternPlayer = PatternPlayer.EmptyPlayer);
            set
            {
                if (Equals(value, _patternPlayer)) return;
                _patternPlayer = value;
                RaisePropertyChanged(() => PatternPlayer);
            }
        }

        public SongPlayer SongPlayer
        {
            get => _songPlayer ?? (_songPlayer = SongPlayer.EmptyPlayer);
            set
            {
                if (Equals(value, _songPlayer)) return;
                _songPlayer = value;
                RaisePropertyChanged(() => SongPlayer);
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (Equals(value, _selectedProject)) return;
                _selectedProject = value;
                RaisePropertyChanged(() => SelectedProject);
                RaisePropertyChanged(() => CanChangeBpm);
                RaisePropertyChanged(() => CanChangeMeasuresAndBeats);
            }
        }

        private void AddInstrument()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            if (SelectedProject.SelectedPattern == null)
            {
                throw new Exception("No current pattern.");
            }

            _openFileDialogService.Reset();
            _openFileDialogService.Multiselect = false;

            if (_openFileDialogService.ShowDialog() == true)
            {
                var path = new Uri(_openFileDialogService.FileName);

                if (!_soundFileValidator.Validate(path))
                {
                    string fileName = Path.GetFileName(path.LocalPath);

                    MessageBox.Show(
                        "'" + fileName + "' was not recognized as a WAV file.",
                        "Unrecognized File",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                SelectedProject.SelectedPattern.AddInstrument(path);
            }

            _cmdHelper.RaiseAll();
        }

        private void AddPattern()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            SelectedProject.AddPattern("Pattern " + (SelectedProject.Patterns.Count + 1));
        }

        private bool CanAddInstrument()
        {
            return SelectedProject != null && !AreAnyPlayersPlaying;
        }

        private bool CanDeleteInstrument(Instrument arg)
        {
            return !AreAnyPlayersPlaying;
        }

        private bool CanExportPattern()
        {
            return !AreAnyPlayersPlaying 
                && SelectedProject != null 
                && SelectedProject.SelectedPattern != null
                && SelectedProject.SelectedPattern.Instruments.Count > 0
                && SelectedProject.SelectedPattern.Instruments[0].Beats.Count > 0;
        }

        private bool CanNewProject()
        {
            return !PatternPlayer.IsPlaying;
        }

        private bool CanOpenProject()
        {
            return !PatternPlayer.IsPlaying;
        }

        private bool CanPlayOrStopPattern()
        {
            return SelectedProject != null && SelectedProject.SelectedPattern != null && !SongPlayer.IsPlaying;
        }

        private bool CanPlayOrStopSong()
        {
            return SelectedProject != null && SelectedProject.SelectedPattern != null && !PatternPlayer.IsPlaying;
        }

        private bool CanSaveProject()
        {
            return SelectedProject != null && !AreAnyPlayersPlaying;
        }
        
        private bool CanAddPattern()
        {
            return SelectedProject != null && !AreAnyPlayersPlaying;
        }

        private void DeleteInstrument(Instrument instrument)
        {
            SelectedProject.SelectedPattern.Instruments.Remove(instrument);

            _cmdHelper.RaiseAll();
        }

        private bool EnsureSavePath()
        {
            if (_savePath == null)
            {
                var dlg = new SaveFileDialog { FileName = SelectedProject.Name + ".dbd" };

                if (dlg.ShowDialog() != true) return false;

                _savePath = new Uri(dlg.FileName);

                return true;
            }
            return true;
        }

        private void ExportPattern()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            var win = new ExportLoopWindow(SelectedProject);

            if (win.ShowDialog() == true)
            {
                var exp = new PatternExporter();
                exp.Export(SelectedProject.SelectedPattern, Bpm, win.SavePath, win.SampleRate, win.BitsPerSample, win.Channels);
            }
        }

        private void NewProject()
        {
            SelectedProject = new Project();
            SelectedProject.Name = "Untitled";

            _cmdHelper.RaiseAll();
        }

        private void OpenProject()
        {
            _openFileDialogService.Reset();
            _openFileDialogService.Multiselect = false;
            _openFileDialogService.DefaultExt = "dbd";
            _openFileDialogService.Filter = "Drum Beat Designer files (*.dbd)|*.dbd";
            _openFileDialogService.FilterIndex = 0;

            if (_openFileDialogService.ShowDialog() != true) return;

            _savePath = new Uri(_openFileDialogService.FileName);

            try
            {
                using (StreamReader reader = File.OpenText(_savePath.LocalPath))
                {
                    SelectedProject = ProjectSerializer.Deserialize(reader.ReadToEnd());
                }
                Bpm = SelectedProject.BeatsPerMinute;
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "This project could not be opened.",
                    "Error Opening Project",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PlayOrStopPattern()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            if (SelectedProject.SelectedPattern == null)
            {
                throw new Exception("No current pattern.");
            }

            if (PatternPlayer.IsPlaying)
            {
                _timer.Stop();

                PatternPlayer.Stopped -= OnPlayerStopped;
                PatternPlayer.Stop();
                PatternPlayer.Dispose();
            }
            else
            {
                PlayTime = new TimeSpan();

                PatternPlayer = new PatternPlayer(SelectedProject.SelectedPattern.Instruments, Bpm, true);
                PatternPlayer.Stopped += OnPlayerStopped;
                PatternPlayer.Play();

                _timer.Start();
            }

            RaisePropertyChanged(() => AreAnyPlayersPlaying);

            _cmdHelper.RaiseAll();
        }

        private void PlayOrStopSong()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            if (SelectedProject.SelectedPattern == null)
            {
                throw new Exception("No current pattern.");
            }

            if (SongPlayer.IsPlaying)
            {
                _timer.Stop();

                SongPlayer.Stopped -= OnPlayerStopped;
                SongPlayer.Stop();
                SongPlayer.Dispose();
            }
            else
            {
                //PlayTime = new TimeSpan();
                
                SongPlayer = new SongPlayer(SelectedProject.Patterns, Bpm);
                SongPlayer.Stopped += OnPlayerStopped;
                SongPlayer.Play();
            }

            _cmdHelper.RaiseAll();
        }

        private void OnPlayerStopped(object sender, EventArgs eventArgs)
        {
            _cmdHelper.RaiseAll();

            RaisePropertyChanged(() => AreAnyPlayersPlaying);
        }

        private void SaveProject()
        {
            if (!EnsureSavePath()) return;

            string serialized = ProjectSerializer.Serialize(SelectedProject);

            File.WriteAllText(_savePath.LocalPath, serialized, Encoding.UTF8);

            //SelectedProject.SetUnchanged();
        }

        private void ShowException(AggregateException ex)
        {
            var win = new ErrorWindow("An unhandled exception occurred", ex.InnerExceptions)
                {
                    Topmost = true,
                    ShowInTaskbar = true
                };
            win.ShowDialog();
        }

        private void ThisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _cmdHelper.RaiseAll();
        }
    }
}