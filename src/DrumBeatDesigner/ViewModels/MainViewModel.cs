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
        readonly CommandHelper _cmdHelper;
        readonly TimeSpan _span = new TimeSpan(0, 0, 0, 1);
        readonly DispatcherTimer _timer;
        readonly ISoundFileValidator _soundFileValidator;
        readonly IOpenFileDialogService _openFileDialogService;
        TimeSpan _playTime;
        Player _player;
        Uri _savePath;
        Project _selectedProject;

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
            AddChannelCommand = new DelegateCommand(AddChannel, CanAddChannel);
            PlayOrStopCommand = new DelegateCommand(PlayOrStop, CanPlayOrStop);
            DeleteChannelCommand = new DelegateCommand<Channel>(DeleteChannel, CanDeleteChannel);
            ExportLoopCommand = new DelegateCommand(ExportLoop, CanExportLoop);

            PropertyChanged += ThisPropertyChanged;
        }

        public MainViewModel()
            : this(new SoundFileValidator(), new OpenFileDialogService())
        {
        }

        public DelegateCommand AddChannelCommand { get; private set; }

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

        public bool CanChangeBpm
        {
            get { return SelectedProject != null && !Player.IsPlaying; }
        }
        public bool CanChangeMeasuresAndBeats
        {
            get { return SelectedProject != null && !Player.IsPlaying; }
        }

        public DelegateCommand<Channel> DeleteChannelCommand { get; private set; }

        public DelegateCommand ExportLoopCommand { get; private set; }

        public DelegateCommand NewProjectCommand { get; private set; }

        public DelegateCommand OpenProjectCommand { get; private set; }

        public DelegateCommand PlayOrStopCommand { get; private set; }

        public TimeSpan PlayTime
        {
            get { return _playTime; }
            set
            {
                if (Equals(value, _playTime)) return;
                _playTime = value;
                RaisePropertyChanged(() => PlayTime);
            }
        }

        public Player Player
        {
            get { return _player ?? (_player = Player.EmptyPlayer); }
            set
            {
                if (Equals(value, _player)) return;
                _player = value;
                RaisePropertyChanged(() => Player);
            }
        }

        public DelegateCommand SaveProjectCommand { get; private set; }

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if (Equals(value, _selectedProject)) return;
                _selectedProject = value;
                RaisePropertyChanged(() => SelectedProject);
                RaisePropertyChanged(() => CanChangeBpm);
                RaisePropertyChanged(() => CanChangeMeasuresAndBeats);
            }
        }

        void AddChannel()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
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

                SelectedProject.AddChannel(path);
            }

            _cmdHelper.RaiseAll();
        }

        bool CanAddChannel()
        {
            return SelectedProject != null && !Player.IsPlaying;
        }

        bool CanDeleteChannel(Channel arg)
        {
            return !Player.IsPlaying;
        }

        bool CanExportLoop()
        {
            return !Player.IsPlaying && SelectedProject != null && SelectedProject.Channels.Count > 0
                   && SelectedProject.Channels[0].Measures.Count > 0
                   && SelectedProject.Channels[0].Measures[0].Beats.Count > 0;
        }

        bool CanNewProject()
        {
            return !Player.IsPlaying;
        }

        bool CanOpenProject()
        {
            return !Player.IsPlaying;
        }

        bool CanPlayOrStop()
        {
            return SelectedProject != null;
        }

        bool CanSaveProject()
        {
            return SelectedProject != null && !Player.IsPlaying;
        }

        void DeleteChannel(Channel channel)
        {
            _selectedProject.Channels.Remove(channel);

            _cmdHelper.RaiseAll();
        }

        bool EnsureSavePath()
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

        void ExportLoop()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            var win = new ExportLoopWindow(SelectedProject);

            if (win.ShowDialog() == true)
            {
                var exp = new LoopExporter();
                exp.Export(SelectedProject, win.SavePath, win.SampleRate, win.BitsPerSample, win.Channels);
            }
        }

        void NewProject()
        {
            SelectedProject = new Project();
            SelectedProject.Name = "Untitled";

            _cmdHelper.RaiseAll();
        }

        void OpenProject()
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

        void PlayOrStop()
        {
            if (SelectedProject == null)
            {
                throw new Exception("No current project.");
            }

            if (Player.IsPlaying)
            {
                _timer.Stop();

                Player.Stop();
                Player.Dispose();
            }
            else
            {
                PlayTime = new TimeSpan();

                Player = new Player(SelectedProject.Channels, Bpm);
                Player.Play();

                _timer.Start();
            }

            RaisePropertyChanged(() => CanChangeBpm);
            RaisePropertyChanged(() => CanChangeMeasuresAndBeats);
        }

        void SaveProject()
        {
            if (!EnsureSavePath()) return;

            string serialized = ProjectSerializer.Serialize(SelectedProject);

            File.WriteAllText(_savePath.LocalPath, serialized, Encoding.UTF8);

            //SelectedProject.SetUnchanged();
        }

        void ShowException(AggregateException ex)
        {
            var win = new ErrorWindow("An unhandled exception occurred", ex.InnerExceptions)
                {
                    Topmost = true,
                    ShowInTaskbar = true
                };
            win.ShowDialog();
        }

        void ThisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _cmdHelper.RaiseAll();
        }
    }
}