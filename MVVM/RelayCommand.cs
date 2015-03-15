using System;
using System.Windows.Input;

namespace AutoGarrisonMissions.MVVM
{
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        #endregion

        #region Constructors

        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region Properties

        public Func<bool> CanExecuteAction { get; set; }

        public Action ExecuteAction { get; set; }

        #endregion

        #region ICommand Members

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            return _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(null)) return;

            _execute.Invoke();
        }

        #endregion

        #region Public Methods

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #endregion
    }
}