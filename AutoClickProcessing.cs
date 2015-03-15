using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoGarrisonMissions.InputSimulator;
using AutoGarrisonMissions.Model;

namespace AutoGarrisonMissions
{
    internal class AutoClickProcessing : IDisposable
    {
        #region Fields

#if DEBUG
        private const int DEFAULT_JUMP_SLEEP = 2000;
        private const int DEFAULT_MISSION_SLEEP = 500;
        private const int DEFAULT_REROLL_SLEEP = 4000;
#else
        private const int DEFAULT_JUMP_SLEEP = 60000;
        private const int DEFAULT_MISSION_SLEEP = 1000;
        private const int DEFAULT_REROLL_SLEEP = 1200000;
#endif

        private static readonly Random _random = new Random();

        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly Semaphore _taskSemaphore = new Semaphore(1, 1);
        private readonly WagFile _wagFile;

        private bool _isRunning;

        private Task _jumpTask;
        private AutoAction[] _missionActions;
        private Task _missionTask;
        private AutoAction[] _rerollActions;
        private Task _rerollTask;

        private Thread _missionThread;
        private Thread _jumpThread;
        private Thread _rerollThread;

        #endregion

        #region Constructors

        public AutoClickProcessing(WagFile currentWagFile)
        {
            if (currentWagFile == null) throw new ArgumentException("currentWagFile");

            _wagFile = currentWagFile;
        }

        #endregion

        #region Events

        public event Action IsRunningChanged;

        #endregion

        #region Properties

        public bool IsRunning
        {
            get { return _isRunning; }
            private set
            {
                if (value == IsRunning) return;

                _isRunning = value;

                RaiseIsRunningChanged();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
            _semaphore.Dispose();
            _taskSemaphore.Dispose();
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _semaphore.WaitOne();

            if (_isRunning)
            {
                _semaphore.Release();
                return;
            }

            IsRunning = true;

            // Start thread stuff
            _missionTask = new Task(MissionAction);
            _rerollTask = new Task(RerollAction);
            _jumpTask = new Task(JumpAction);

            _missionTask.Start();
            _rerollTask.Start();
            _jumpTask.Start();

            _semaphore.Release();
        }

        public void Stop()
        {
            _semaphore.WaitOne();

            if (!_isRunning)
            {
                _semaphore.Release();
                return;
            }

            IsRunning = false;

            // Stop thread stuff
            _missionThread.Abort();
            _rerollThread.Abort();
            _jumpThread.Abort();

            Thread.Sleep(50);

            _missionTask.Dispose();
            _rerollTask.Dispose();
            _jumpTask.Dispose();

            _missionTask = null;
            _rerollTask = null;
            _jumpTask = null;

            _semaphore.Release();

            try
            {
                _taskSemaphore.Release();
            }
            catch (SemaphoreFullException)
            { }
        }

        #endregion

        #region Private Methods

        private static int GetSleepTime(TimeSpan sleepTime)
        {
            var sleep = (int)sleepTime.TotalMilliseconds;
            sleep = _random.Next((int)(sleep * 0.80), (int)(sleep * 1.20));

            return sleep;
        }

        private void JumpAction()
        {
            _jumpThread = Thread.CurrentThread;

            while (true)
            {
                var sleep = GetSleepTime(_wagFile.JumpInterval);
#if DEBUG
                sleep = DEFAULT_JUMP_SLEEP;
#else
                if (sleep < DEFAULT_JUMP_SLEEP)
                    sleep = DEFAULT_JUMP_SLEEP;
#endif

                Thread.Sleep(sleep);

                _taskSemaphore.WaitOne();

                KeyboardInputSimulator.SimulateSpace();

                _taskSemaphore.Release();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void MissionAction()
        {
            _missionActions = _wagFile.MissionActions.ToArray();
            _missionThread = Thread.CurrentThread;

            Thread.Sleep(1000);

            while (true)
            {
                _taskSemaphore.WaitOne();

                foreach (var missionAction in _missionActions)
                {
                    var x = _random.Next(missionAction.XMin, missionAction.XMax + 1);
                    var y = _random.Next(missionAction.YMin, missionAction.YMax + 1);

                    MouseClickSimulator.SimulateLeftClick(x, y);

                    var sleep = GetSleepTime(missionAction.Interval);
#if DEBUG
                    sleep = DEFAULT_MISSION_SLEEP;
#else
                    if (sleep < DEFAULT_MISSION_SLEEP)
                        sleep = DEFAULT_MISSION_SLEEP;
#endif

                    Thread.Sleep(sleep);
                }

                _taskSemaphore.Release();

                Thread.Sleep(5000);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void RaiseIsRunningChanged()
        {
            if (IsRunningChanged != null)
                IsRunningChanged();
        }

        private void RerollAction()
        {
            _rerollActions = _wagFile.RerollActions.ToArray();
            _rerollThread = Thread.CurrentThread;

            while (true)
            {
                var longSleep = GetSleepTime(_wagFile.AltsInterval);
#if DEBUG
                longSleep = DEFAULT_REROLL_SLEEP;
#else
                if (longSleep < DEFAULT_REROLL_SLEEP)
                    longSleep = DEFAULT_REROLL_SLEEP;
#endif

                Thread.Sleep(longSleep);

                _taskSemaphore.WaitOne();

                foreach (var rerollAction in _rerollActions)
                {
                    var x = _random.Next(rerollAction.XMin, rerollAction.XMax + 1);
                    var y = _random.Next(rerollAction.YMin, rerollAction.YMax + 1);

                    MouseClickSimulator.SimulateLeftClick(x, y);

                    var sleep = GetSleepTime(rerollAction.Interval);
#if DEBUG
                    sleep = DEFAULT_MISSION_SLEEP;
#else
                    if (sleep < DEFAULT_MISSION_SLEEP)
                        sleep = DEFAULT_MISSION_SLEEP;
#endif

                    Thread.Sleep(sleep);
                }

                _taskSemaphore.Release();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        #endregion
    }
}