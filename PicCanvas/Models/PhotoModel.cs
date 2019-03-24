namespace PicCanvas.Models
{
    using GalaSoft.MvvmLight;

    public class PhotoModel : ObservableObject
    {
        #region Fields

        public double _centerX;
        public double _centerY;
        public string _fileName;
        public double _rotation;
        public double _scale = 1.0;
        public double _translateX;
        public double _translateY;

        #endregion Fields

        #region Properties

        public double CenterX
        {
            get { return _centerX; }
            set
            {
                _centerX = value;
                RaisePropertyChanged("CenterX");
            }
        }

        public double CenterY
        {
            get { return _centerY; }
            set
            {
                _centerY = value;
                RaisePropertyChanged("CenterY");
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public double Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                RaisePropertyChanged("Rotation");
            }
        }

        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                RaisePropertyChanged("Scale");
            }
        }

        public double TranslateX
        {
            get { return _translateX; }
            set
            {
                _translateX = value;
                RaisePropertyChanged("TranslateX");
            }
        }

        public double TranslateY
        {
            get { return _translateY; }
            set
            {
                _translateY = value;
                RaisePropertyChanged("TranslateY");
            }
        }

        #endregion Properties
    }
}