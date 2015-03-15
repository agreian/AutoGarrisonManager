﻿namespace AutoGarrisonMissions
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel(this);
        }

        #endregion
    }
}