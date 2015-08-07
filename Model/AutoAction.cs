using System;
using System.Xml.Serialization;
using AutoGarrisonMissions.MVVM;

namespace AutoGarrisonMissions.Model
{
    [Serializable]
    public class AutoAction : ObservableObject
    {
        #region Fields

        private ushort _xMax;
        private ushort _xMin;
        private ushort _yMax;
        private ushort _yMin;

        #endregion

        #region Properties

        [XmlIgnore]
        public TimeSpan Interval { get; set; }

        [XmlElement("Interval")]
        public string IntervalString
        {
            get { return Interval.ToString(); }
            set
            {
                Interval = TimeSpan.Parse(value);

                RaisePropertyChanged(() => IntervalString);
            }
        }

        public string Name { get; set; }

        public ushort XMax
        {
            get { return _xMax; }
            set
            {
                if (XMax == value) return;

                _xMax = value;
                if (XMin > XMax)
                {
                    var temp = XMin;
                    XMin = XMax;
                    XMax = temp;
                }

                RaisePropertyChanged(() => XMax);
            }
        }

        public ushort XMin
        {
            get { return _xMin; }
            set
            {
                if (XMin == value) return;

                _xMin = value;
                if (XMin > XMax)
                {
                    var temp = XMin;
                    XMin = XMax;
                    XMax = temp;
                }

                RaisePropertyChanged(() => XMin);
            }
        }

        public ushort YMax
        {
            get { return _yMax; }
            set
            {
                if (YMax == value) return;

                _yMax = value;
                if (YMin > YMax)
                {
                    var temp = YMin;
                    YMin = YMax;
                    YMax = temp;
                }

                RaisePropertyChanged(() => YMax);
            }
        }

        public ushort YMin
        {
            get { return _yMin; }
            set
            {
                if (YMin == value) return;

                _yMin = value;
                if (YMin > YMax)
                {
                    var temp = YMin;
                    YMin = YMax;
                    YMax = temp;
                }

                RaisePropertyChanged(() => YMin);
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format("Name : {0}, XMin = {2}, XMax = {3}, YMin = {4}, YMax = {5}, Interval : {1}", Name, Interval, XMin, XMax, YMin, YMax);
        }

        public void SetXMax(int xMax)
        {
            XMax = Convert.ToUInt16(xMax);
            //RaisePropertyChanged(() => XMax);
        }

        public void SetXMin(int xMin)
        {
            _xMin = Convert.ToUInt16(xMin);
            RaisePropertyChanged(() => XMin);
        }

        public void SetYMax(int yMax)
        {
            YMax = Convert.ToUInt16(yMax);
            //RaisePropertyChanged(() => YMax);
        }

        public void SetYMin(int yMin)
        {
            _yMin = Convert.ToUInt16(yMin);
            RaisePropertyChanged(() => YMin);
        }

        #endregion
    }
}