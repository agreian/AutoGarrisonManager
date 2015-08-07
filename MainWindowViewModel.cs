using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using AutoGarrisonMissions.HotkeyHelper;
using AutoGarrisonMissions.Model;
using AutoGarrisonMissions.MVVM;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace AutoGarrisonMissions
{
    internal class MainWindowViewModel : ViewModelBase<MainWindow>
    {
        #region Fields

        private const string WOW_AUTO_GARRISON_MISSIONS = "WoW - Auto garrison missions";

        private readonly Image _add = new Image(@"Resources\add.png");
        private readonly Task _canSaveRefresh;
        private readonly Image _down = new Image(@"Resources\arrow_down.png");
        private readonly Image _icon = new Image(@"Resources\garrison.png");
        private readonly Image _minus = new Image(@"Resources\minus.png");
        private readonly Image _up = new Image(@"Resources\arrow_up.png");

        private AutoClickProcessing _autoClickProcessing;
        private Thread _canSaveRefreshThread;
        private Hotkey _controlLHotkey;
        private Hotkey _controlPHotkey;
        private WagFile _currentWagFile;
        private HotkeyHost _hotkeyHost;
        private AutoAction _lastSelectedAction;
        private bool _minOrMax = true;
        private string _mousePosition1;
        private string _mousePosition2;
        private AutoAction _selectedMissionAction;
        private AutoAction _selectedRerollAction;

        #endregion

        #region Constructors

        public MainWindowViewModel(MainWindow window)
            : base(window)
        {
            Window.Loaded += OnWindowLoaded;
            Window.Unloaded += OnWindowUnloaded;

            Window.Closing += OnWindowClosing;
            Window.Closed += OnWindowClosed;

            NewFileCommand = new RelayCommand(NewFile);
            SaveFileCommand = new RelayCommand(SaveFile, CanSaveFile);
            OpenFileCommand = new RelayCommand(OpenFileWithDialog);
            CloseFileCommand = new RelayCommand(CloseFile, IsWagFileLoaded);

            AddMissionActionCommand = new RelayCommand(AddMissionAction, IsWagFileLoaded);
            RemoveMissionActionCommand = new RelayCommand(RemoveMissionAction, CanRemoveMissionAction);
            UpMissionActionCommand = new RelayCommand(UpMissionAction, CanRemoveMissionAction);
            DownMissionActionCommand = new RelayCommand(DownMissionAction, CanRemoveMissionAction);

            AddRerollActionCommand = new RelayCommand(AddRerollAction, IsWagFileLoaded);
            RemoveRerollActionCommand = new RelayCommand(RemoveRerollAction, CanRemoveRerollAction);
            UpRerollActionCommand = new RelayCommand(UpRerollAction, CanRemoveRerollAction);
            DownRerollActionCommand = new RelayCommand(DownRerollAction, CanRemoveRerollAction);

            GetMousePositionCommand = new RelayCommand(GetMousePosition);
            StartAutoClickCommand = new RelayCommand(StartAutoClick, IsWagFileLoaded);

            _canSaveRefresh = new Task(CanSaveRefresh);
            _canSaveRefresh.Start();

            if (App.FileToOpen != null)
                OpenFile(App.FileToOpen);
        }

        #endregion

        #region Properties

        public override Image Icon
        {
            get { return _icon; }
        }

        public override string Title
        {
            get
            {
                return CurrentWagFile == null
                    ? WOW_AUTO_GARRISON_MISSIONS
                    : string.Format("{0} - {1}", WOW_AUTO_GARRISON_MISSIONS, CurrentWagFile.FileName);
            }
        }

        public Image Add
        {
            get { return _add; }
        }

        public RelayCommand AddMissionActionCommand { get; private set; }
        public RelayCommand AddRerollActionCommand { get; private set; }

        public string AltsInterval
        {
            get { return CurrentWagFile == null ? null : CurrentWagFile.AltsIntervalString; }
            set
            {
                if (CurrentWagFile == null)
                    return;

                CurrentWagFile.AltsIntervalString = value;

                RaisePropertyChanged(() => AltsInterval);
            }
        }

        public bool CanEditWagFileName
        {
            get { return CurrentWagFile != null; }
        }

        public RelayCommand CloseFileCommand { get; private set; }

        public WagFile CurrentWagFile
        {
            get { return _currentWagFile; }
            set
            {
                _currentWagFile = value;

                RaiseWagFileChanged();
            }
        }

        public Image Down
        {
            get { return _down; }
        }

        public RelayCommand DownMissionActionCommand { get; private set; }
        public RelayCommand DownRerollActionCommand { get; private set; }

        public RelayCommand GetMousePositionCommand { get; private set; }

        public string JumpInterval
        {
            get { return CurrentWagFile == null ? null : CurrentWagFile.JumpIntervalString; }
            set
            {
                if (CurrentWagFile == null)
                    return;

                CurrentWagFile.JumpIntervalString = value;

                RaisePropertyChanged(() => JumpInterval);
            }
        }

        public Image Minus
        {
            get { return _minus; }
        }

        public ObservableCollection<AutoAction> MissionActions
        {
            get { return CurrentWagFile == null ? null : CurrentWagFile.MissionActions; }
        }

        public string MousePosition1
        {
            get { return _mousePosition1; }
            set
            {
                _mousePosition1 = value;
                RaisePropertyChanged(() => MousePosition1);
            }
        }

        public string MousePosition2
        {
            get { return _mousePosition2; }
            set
            {
                _mousePosition2 = value;
                RaisePropertyChanged(() => MousePosition2);
            }
        }

        public string Name
        {
            get { return CurrentWagFile == null ? null : CurrentWagFile.Name; }
            set
            {
                if (CurrentWagFile == null)
                    return;

                CurrentWagFile.Name = value;
            }
        }

        public RelayCommand NewFileCommand { get; private set; }
        public RelayCommand OpenFileCommand { get; private set; }
        public RelayCommand RemoveMissionActionCommand { get; private set; }
        public RelayCommand RemoveRerollActionCommand { get; private set; }

        public ObservableCollection<AutoAction> RerollActions
        {
            get { return CurrentWagFile == null ? null : CurrentWagFile.RerollActions; }
        }

        public RelayCommand SaveFileCommand { get; private set; }

        public AutoAction SelectedMissionAction
        {
            get { return _selectedMissionAction; }
            set
            {
                _selectedMissionAction = value;
                _minOrMax = true;
                _lastSelectedAction = value;
                RaisePropertyChanged(() => SelectedMissionAction);

                RemoveMissionActionCommand.RaiseCanExecuteChanged();
                UpMissionActionCommand.RaiseCanExecuteChanged();
                DownMissionActionCommand.RaiseCanExecuteChanged();
            }
        }

        public AutoAction SelectedRerollAction
        {
            get { return _selectedRerollAction; }
            set
            {
                _selectedRerollAction = value;
                _minOrMax = true;
                _lastSelectedAction = value;
                RaisePropertyChanged(() => SelectedRerollAction);

                RemoveRerollActionCommand.RaiseCanExecuteChanged();
                UpRerollActionCommand.RaiseCanExecuteChanged();
                DownRerollActionCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand StartAutoClickCommand { get; private set; }

        public string StartStopText
        {
            get
            {
                if (_autoClickProcessing == null || !_autoClickProcessing.IsRunning)
                    return "Start";

                return "Stop";
            }
        }

        public Image Up
        {
            get { return _up; }
        }

        public RelayCommand UpMissionActionCommand { get; private set; }
        public RelayCommand UpRerollActionCommand { get; private set; }

        #endregion

        #region Public Methods

        public void OpenFile(string path)
        {
            SetCurrentWagFile(WagFile.Deserialize(path));

            App.AddRecentFile(path);
        }

        #endregion

        #region Private Methods

        private void AddMissionAction()
        {
            CurrentWagFile.MissionActions.Add(new AutoAction());
        }

        private void AddRerollAction()
        {
            CurrentWagFile.RerollActions.Add(new AutoAction());
        }

        private bool CanRemoveMissionAction()
        {
            return IsWagFileLoaded() && SelectedMissionAction != null;
        }

        private bool CanRemoveRerollAction()
        {
            return IsWagFileLoaded() && SelectedRerollAction != null;
        }

        private bool CanSaveFile()
        {
            return IsWagFileLoaded() && CurrentWagFile.IsSaveNeeded();
        }

        private void CanSaveRefresh()
        {
            _canSaveRefreshThread = Thread.CurrentThread;

            while (true)
            {
                Thread.Sleep(200);

                Application.Current.Dispatcher.BeginInvoke(new Action(() => SaveFileCommand.RaiseCanExecuteChanged()));
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void CloseFile()
        {
            SetCurrentWagFile(null);
        }

        private void DownMissionAction()
        {
            var index = MissionActions.IndexOf(SelectedMissionAction);

            if (index >= 0 && index < MissionActions.Count - 1)
                MissionActions.Move(index, index + 1);
        }

        private void DownRerollAction()
        {
            var index = RerollActions.IndexOf(SelectedRerollAction);

            if (index >= 0 && index < RerollActions.Count - 1)
                RerollActions.Move(index, index + 1);
        }

        private void GetMousePosition()
        {
            if (!string.IsNullOrWhiteSpace(MousePosition1) && string.IsNullOrWhiteSpace(MousePosition2))
            {
                var position = Control.MousePosition;
                MousePosition2 = string.Format("X = {0}, Y = {1}", position.X, position.Y);
            }
            else
            {
                MousePosition1 = string.Empty;
                MousePosition2 = string.Empty;

                var position = Control.MousePosition;
                MousePosition1 = string.Format("X = {0}, Y = {1}", position.X, position.Y);
            }

            if (_lastSelectedAction != null)
            {
                if (_minOrMax)
                {
                    _lastSelectedAction.SetXMin(Control.MousePosition.X);
                    _lastSelectedAction.SetYMin(Control.MousePosition.Y);
                }
                else
                {
                    _lastSelectedAction.SetXMax(Control.MousePosition.X);
                    _lastSelectedAction.SetYMax(Control.MousePosition.Y);
                }

                _minOrMax = !_minOrMax;
            }
        }

        private bool IsWagFileLoaded()
        {
            return CurrentWagFile != null;
        }

        private void NewFile()
        {
            SetCurrentWagFile(new WagFile());
        }

        private void OnControlLHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            GetMousePositionCommand.Execute(null);
        }

        private void OnControlPHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            if (!StartAutoClickCommand.CanExecute(null)) return;

            StartAutoClickCommand.Execute(null);
        }

        private void OnIsRunningChanged()
        {
            RaisePropertyChanged(() => StartStopText);
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _canSaveRefreshThread.Abort();

            Thread.Sleep(50);

            _canSaveRefresh.Dispose();

            Application.Current.Shutdown();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (CurrentWagFile != null)
            {
                if (SetCurrentWagFile(null) == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _hotkeyHost = new HotkeyHost((HwndSource) PresentationSource.FromVisual(Application.Current.MainWindow));
            _controlPHotkey = new Hotkey(Key.P, ModifierKeys.Control);
            _controlPHotkey.HotKeyPressed += OnControlPHotkeyPressed;

            _controlLHotkey = new Hotkey(Key.L, ModifierKeys.Control);
            _controlLHotkey.HotKeyPressed += OnControlLHotkeyPressed;

            _hotkeyHost.AddHotKey(_controlPHotkey);
            _hotkeyHost.AddHotKey(_controlLHotkey);
        }

        private void OnWindowUnloaded(object sender, RoutedEventArgs e)
        {
            _hotkeyHost.RemoveHotKey(_controlPHotkey);
        }

        private void OpenFileWithDialog()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".wag",
                Multiselect = false,
                Filter = "Wow Auto Garrison Missions files (*.wag)|*.wag"
            };

            var result = dlg.ShowDialog();
            if (result != true) return;

            OpenFile(dlg.FileName);
        }

        private void RaiseWagFileChanged()
        {
            if (_autoClickProcessing != null)
            {
                _autoClickProcessing.IsRunningChanged -= OnIsRunningChanged;
                _autoClickProcessing.Dispose();
                _autoClickProcessing = null;
            }

            RaisePropertyChanged(() => MissionActions);
            RaisePropertyChanged(() => RerollActions);

            RaisePropertyChanged(() => Name);
            RaisePropertyChanged(() => AltsInterval);
            RaisePropertyChanged(() => JumpInterval);

            RaisePropertyChanged(() => Title);

            RaisePropertyChanged(() => CanEditWagFileName);

            CloseFileCommand.RaiseCanExecuteChanged();
            SaveFileCommand.RaiseCanExecuteChanged();

            AddMissionActionCommand.RaiseCanExecuteChanged();
            RemoveMissionActionCommand.RaiseCanExecuteChanged();
            UpMissionActionCommand.RaiseCanExecuteChanged();
            DownMissionActionCommand.RaiseCanExecuteChanged();

            AddRerollActionCommand.RaiseCanExecuteChanged();
            RemoveRerollActionCommand.RaiseCanExecuteChanged();
            UpRerollActionCommand.RaiseCanExecuteChanged();
            DownRerollActionCommand.RaiseCanExecuteChanged();

            StartAutoClickCommand.RaiseCanExecuteChanged();
        }

        private void RemoveMissionAction()
        {
            CurrentWagFile.MissionActions.Remove(SelectedMissionAction);
        }

        private void RemoveRerollAction()
        {
            CurrentWagFile.RerollActions.Remove(SelectedRerollAction);
        }

        private void SaveFile()
        {
            if (CurrentWagFile == null) return;

            var path = CurrentWagFile.FilePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    DefaultExt = ".wag",
                    Filter = "Wow Auto Garrison Missions files (*.wag)|*.wag"
                };

                var result = dlg.ShowDialog();
                if (result != true) return;

                path = dlg.FileName;
            }

            CurrentWagFile.Serialize(path);

            RaisePropertyChanged(() => Title);
        }

        private MessageBoxResult SetCurrentWagFile(WagFile newWagFile)
        {
            var result = MessageBoxResult.None;
            if (CurrentWagFile != null && CurrentWagFile.IsSaveNeeded())
            {
                result = MessageBox.Show("Do you want to save the current wag file ?", "Save ?",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Cancel)
                    return result;

                if (result == MessageBoxResult.Yes)
                    SaveFileCommand.Execute(null);
            }

            CurrentWagFile = newWagFile;

            return result;
        }

        private void StartAutoClick()
        {
            if (_autoClickProcessing == null)
            {
                _autoClickProcessing = new AutoClickProcessing(CurrentWagFile);
                _autoClickProcessing.IsRunningChanged += OnIsRunningChanged;
            }

            if (_autoClickProcessing.IsRunning)
                _autoClickProcessing.Stop();
            else
                _autoClickProcessing.Start();
        }

        private void UpMissionAction()
        {
            var index = MissionActions.IndexOf(SelectedMissionAction);

            if (index > 0)
                MissionActions.Move(index, index - 1);
        }

        private void UpRerollAction()
        {
            var index = RerollActions.IndexOf(SelectedRerollAction);

            if (index > 0)
                RerollActions.Move(index, index - 1);
        }

        #endregion
    }
}