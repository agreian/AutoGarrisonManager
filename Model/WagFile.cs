using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace AutoGarrisonMissions.Model
{
    [Serializable]
    public class WagFile
    {
        #region Fields

        private readonly ObservableCollection<AutoAction> _missionActions = new ObservableCollection<AutoAction>();
        private readonly ObservableCollection<AutoAction> _rerollActions = new ObservableCollection<AutoAction>();

        [XmlIgnore] private TimeSpan _altsInterval = new TimeSpan(0, 30, 0);

        [XmlIgnore] private TimeSpan _jumpInterval = new TimeSpan(0, 20, 0);

        #endregion

        #region Constructors

        public WagFile()
        {
            FileName = "New file";
        }

        public WagFile(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public TimeSpan AltsInterval
        {
            get { return _altsInterval; }
            set { _altsInterval = value; }
        }

        [XmlElement("AltsInterval")]
        public string AltsIntervalString
        {
            get { return AltsInterval.ToString(); }
            set { AltsInterval = TimeSpan.Parse(value); }
        }

        [XmlIgnore]
        public string FileName { get; private set; }

        [XmlIgnore]
        public string FilePath { get; private set; }

        [XmlIgnore]
        public TimeSpan JumpInterval
        {
            get { return _jumpInterval; }
            set { _jumpInterval = value; }
        }

        [XmlElement("JumpInterval")]
        public string JumpIntervalString
        {
            get { return JumpInterval.ToString(); }
            set { JumpInterval = TimeSpan.Parse(value); }
        }

        public ObservableCollection<AutoAction> MissionActions
        {
            get { return _missionActions; }
        }

        public string Name { get; set; }

        public ObservableCollection<AutoAction> RerollActions
        {
            get { return _rerollActions; }
        }

        #endregion

        #region Public Methods

        public static WagFile Deserialize(string path)
        {
            try
            {
                WagFile wagFile;

                using (Stream stream = File.Open(path, FileMode.Open))
                {
                    var bin = new XmlSerializer(typeof (WagFile));

                    wagFile = (WagFile) bin.Deserialize(stream);
                }

                wagFile.FilePath = path;
                wagFile.FileName = Path.GetFileNameWithoutExtension(path);

                return wagFile;
            }
            catch
            {
                MessageBox.Show("Unable to load the selected file");
                return null;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Name : {0}", Name));
            sb.AppendLine("MissionActions :");
            foreach (var action in MissionActions)
                sb.AppendLine(action.ToString());
            sb.AppendLine("RerollActions :");
            foreach (var action in RerollActions)
                sb.AppendLine(action.ToString());

            return sb.ToString();
        }

        public bool IsSaveNeeded()
        {
            if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
                return true;

            try
            {
                byte[] byteArray1;
                byte[] byteArray2;

                using (var stream1 = new MemoryStream())
                {
                    Serialize(stream1);
                    byteArray1 = stream1.ToArray();
                }

                using (var stream2 = new MemoryStream())
                {
                    using (var fileStream2 = File.Open(FilePath, FileMode.Open))
                    {
                        fileStream2.CopyTo(stream2);
                    }
                    byteArray2 = stream2.ToArray();
                }

                if (byteArray1.Length != byteArray2.Length) return true;

                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < byteArray1.Length; ++i)
                    if (byteArray1[i] != byteArray2[i])
                        return true;

                return false;
            }
            catch
            {
                return true;
            }
        }

        public void Serialize(string path)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Create))
                {
                    Serialize(stream);
                }

                FilePath = path;
                FileName = Path.GetFileNameWithoutExtension(path);
            }
            catch
            {
                MessageBox.Show("Unable to save wag file");
            }
        }

        #endregion

        #region Private Methods

        private void Serialize(Stream stream)
        {
            var bin = new XmlSerializer(typeof (WagFile));
            bin.Serialize(stream, this);
        }

        #endregion
    }
}