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
        public void CanAddChannel()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();

            Assert.AreEqual<int>(vm.SelectedProject.Channels.Count, 0);

            vm.AddChannelCommand.Execute();

            Assert.AreEqual<int>(vm.SelectedProject.Channels.Count, 1);
        }

        [TestMethod]
        public void CanPlayAndStopPlayer()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();

            Assert.AreEqual(vm.Player.IsPlaying, false);

            vm.PlayOrStopCommand.Execute();

            Assert.AreEqual(vm.Player.IsPlaying, true);

            vm.PlayOrStopCommand.Execute();

            Assert.AreEqual(vm.Player.IsPlaying, false);
        }

        [TestMethod]
        public void CanDeleteChannel()
        {
            MainViewModel vm = CreateNewMainViewModel();
            vm.NewProjectCommand.Execute();

            vm.AddChannelCommand.Execute();

            Channel c = vm.SelectedProject.Channels.First();

            vm.DeleteChannelCommand.Execute(c);

            Assert.AreEqual(vm.SelectedProject.Channels.Count, 0);
        }

        [TestMethod]
        public void CanOpenProject()
        {
            MainViewModel vm = new MainViewModel(new MockSoundFileValidator(), new MockOpenProjectFileDialogService());
            vm.OpenProjectCommand.Execute();

            Assert.AreEqual(vm.SelectedProject.Name, "Test Project");
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

        public string FileName { 
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
