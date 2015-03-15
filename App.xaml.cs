using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;

namespace AutoGarrisonMissions
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App
    {
        #region Fields

        private JumpList _jumpList;

        #endregion

        #region Properties

        public static string FileToOpen { get; private set; }

        #endregion

        #region Public Methods

        public static void AddRecentFile(string path)
        {
            var jumpList = JumpList.GetJumpList(Current);
            if (jumpList == null) return;

            var title = Path.GetFileNameWithoutExtension(path);
            var programLocation = Assembly.GetCallingAssembly().Location;
            if (path.IndexOf(' ') != -1)
                path = string.Format("\"{0}\"", path);

            var jumpTask = new JumpTask
            {
                ApplicationPath = programLocation,
                Arguments = path,
                Description = path,
                IconResourcePath = programLocation,
                Title = title
            };
            JumpList.AddToRecentCategory(jumpTask);

            jumpList.Apply();
        }

        #endregion

        #region Private Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            string path = null;
            if (e.Args.Length > 0)
            {
                path = e.Args[0];
            }

            if (path != null && File.Exists(path))
            {
                FileToOpen = path;
            }

            _jumpList = new JumpList {ShowRecentCategory = true};
            JumpList.SetJumpList(Current, _jumpList);
        }

        #endregion
    }
}