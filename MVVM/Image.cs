using System;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoGarrisonMissions.MVVM
{
    public class Image : ObservableObject
    {
        #region Fields

        private readonly static ImageSourceConverter _isc = new ImageSourceConverter();
        private double _height;
        private ImageSource _source;
        private double _width;

        #endregion

        #region Constructors

        public Image(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            var bitmapImage = LoadBitmapFromResource(pathInApplication, assembly);

            Source = bitmapImage;
            Width = bitmapImage.PixelWidth;
            Height = bitmapImage.PixelHeight;
        }

        public Image(Uri uri)
        {
            var bitmapImage = new BitmapImage(uri);

            Source = bitmapImage;
            Width = bitmapImage.PixelWidth;
            Height = bitmapImage.PixelHeight;
        }

        #endregion

        #region Properties

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged(() => Height);
            }
        }

        public ImageSource Source
        {
            get { return _source; }
            set
            {
                _source = value;
                RaisePropertyChanged(() => Source);
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged(() => Width);
            }
        }

        #endregion

        #region Private Methods

        private static BitmapSource LoadBitmapFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication.StartsWith("pack://application:"))
            {
                var source = _isc.ConvertFromInvariantString(pathInApplication);
                var result = (BitmapSource) source;
                return result;
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }

            return _isc.ConvertFromInvariantString(String.Format(@"pack://application:,,,/{0};component/{1}", assembly.GetName().Name, pathInApplication)) as BitmapSource;
        }

        #endregion
    }
}