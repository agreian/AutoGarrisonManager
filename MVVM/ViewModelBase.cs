using System.Windows;

namespace AutoGarrisonMissions.MVVM
{
    public abstract class ViewModelBase<T> : ObservableObject
        where T : Window
    {
        #region Fields

        private readonly T _window;

        #endregion

        #region Constructors

        protected ViewModelBase(T window)
        {
            _window = window;
        }

        #endregion

        #region Properties

        public abstract Image Icon { get; }

        public abstract string Title { get; }

        public Window Window
        {
            get { return _window; }
        }

        #endregion
    }
}