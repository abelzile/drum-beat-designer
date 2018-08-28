using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DrumBeatDesigner.Models;
using DrumBeatDesigner.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonAndSun.Commons.OSServices;


namespace DrumBeatDesigner.Test.ViewModelTests
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestMethod]
        public void CanAddNewProject()
        {
            MainViewModel vm = CreateNewMainViewModel();

            Assert.IsNull(vm.SelectedProject);

            vm.NewProjectCommand.Execute();

            Assert.IsNotNull(vm.SelectedProject);
        }

        [TestMethod]
        public void CanInitializeNewProject()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();

            Assert.AreEqual<string>("Untitled", vm.SelectedProject.Name);
        }

        [TestMethod]
        public void CanAddPattern()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();

            Assert.AreEqual(0, vm.SelectedProject.Patterns.Count);

            vm.AddPatternCommand.Execute();

            Assert.AreEqual(1, vm.SelectedProject.Patterns.Count);
        }

        [TestMethod]
        public void CanAddInstrument()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();
            vm.AddPatternCommand.Execute();

            Assert.AreEqual<int>(0, vm.SelectedProject.SelectedPattern.Instruments.Count);

            vm.AddInstrumentCommand.Execute();

            Assert.AreEqual<int>(1, vm.SelectedProject.SelectedPattern.Instruments.Count);
        }

        [TestMethod]
        public void CanPlayAndStopPlayer()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();
            vm.AddPatternCommand.Execute();

            Assert.AreEqual(false, vm.PatternPlayer.IsPlaying);

            vm.PlayOrStopPatternCommand.Execute();

            Assert.AreEqual(true, vm.PatternPlayer.IsPlaying);

            vm.PlayOrStopPatternCommand.Execute();

            Assert.AreEqual(false, vm.PatternPlayer.IsPlaying);
        }

        [TestMethod]
        public void CanDeleteInstrument()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();
            vm.AddPatternCommand.Execute();
            vm.AddInstrumentCommand.Execute();

            Instrument c = vm.SelectedProject.SelectedPattern.Instruments.First();

            vm.DeleteInstrumentCommand.Execute(c);

            Assert.AreEqual(0, vm.SelectedProject.SelectedPattern.Instruments.Count);
        }

        [TestMethod]
        public void CanOpenProject()
        {
            MainViewModel vm = new MainViewModel(new MockSoundFileValidator(), new MockOpenProjectFileDialogService());
            vm.OpenProjectCommand.Execute();

            Assert.AreEqual("Test Project", vm.SelectedProject.Name);
        }

        public void CanSaveProject()
        {
            //todo
        }

        static MainViewModel CreateNewMainViewModel()
        {
            return new MainViewModel(new MockSoundFileValidator(), new MockOpenChannelFileDialogService());
        }
    }

    public class MockSoundFileValidator : ISoundFileValidator
    {
        public bool Validate(Uri path)
        {
            return true;
        }
    }

    public class MockOpenChannelFileDialogService : IOpenFileDialogService
    {
        public bool? ShowDialog()
        {
            return true;
        }

        public void Reset()
        {
        }

        public string FileName
        {
            get { return "C:\\FakeFile.wav"; }
        }

        public bool Multiselect { get; set; }
        public string DefaultExt { get; set; }
        public string Filter { get; set; }
        public int FilterIndex { get; set; }
    }

    public class MockOpenProjectFileDialogService : IOpenFileDialogService
    {
        public bool? ShowDialog()
        {
            return true;
        }

        public void Reset()
        {
        }

        public string FileName
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestProject.dbd"); }
        }

        public bool Multiselect { get; set; }
        public string DefaultExt { get; set; }
        public string Filter { get; set; }
        public int FilterIndex { get; set; }
    }
}
