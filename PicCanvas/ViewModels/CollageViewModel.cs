namespace PicCanvas.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows.Media.Imaging;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using Microsoft.Phone.Tasks;

    using PicCanvas.Models;

    public class CollageViewModel : ViewModelBase
    {
        #region Fields

        private const string PhotosDirectory = "Photos";

        private PhotoChooserTask _photoChooserTask;
        private ObservableCollection<PhotoModel> _photoCollection;

        #endregion Fields

        #region Constructors

        public CollageViewModel()
        {
            AddPhotoFromPhotosHubCommand = new RelayCommand(AddPhotoFromPhotosHub);
            PhotoCollection = new ObservableCollection<PhotoModel>();
        }

        #endregion Constructors

        #region Properties

        public RelayCommand AddPhotoFromPhotosHubCommand
        {
            get;
            private set;
        }

        public ObservableCollection<PhotoModel> PhotoCollection
        {
            get { return _photoCollection; }
            private set
            {
                _photoCollection = value;
                RaisePropertyChanged("PhotoCollection");
            }
        }

        #endregion Properties

        #region Methods

        private void AddPhotoFromPhotosHub()
        {
            // Lazy initialization.
            if (_photoChooserTask == null)
            {
                _photoChooserTask = new PhotoChooserTask();
                _photoChooserTask.Completed += OnPhotoChooserTaskCompleted;
            }

            _photoChooserTask.Show();
        }

        private string GenerateNewFileName()
        {
            return string.Format("{0}.jpg", DateTime.Now.ToString(Configs.PhotoFileNamePattern));
        }

        private void OnPhotoChooserTaskCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                var photoSource = new BitmapImage();
                photoSource.SetSource(e.ChosenPhoto);

                var savePhotoWorker = new BackgroundWorker();
                savePhotoWorker.DoWork += OnSavePhotoWorkerDoWork;
                savePhotoWorker.RunWorkerCompleted += OnSavePhotoWorkerRunWorkerCompleted;
                savePhotoWorker.RunWorkerAsync(new WriteableBitmap(photoSource));
            }
        }

        private void OnSavePhotoWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var bitmap = e.Argument as WriteableBitmap;
            if (bitmap == null)
            {
                return;
            }

            e.Result = SaveBitmapToIsolatedStorage(bitmap);
        }

        private void OnSavePhotoWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var photoFileName = e.Result as string;
                if (photoFileName == null)
                {
                    return;
                }

                var newPhoto = new PhotoModel();
                newPhoto.FileName = photoFileName;
                PhotoCollection.Add(newPhoto);
            }
        }

        private string SaveBitmapToIsolatedStorage(WriteableBitmap bitmap)
        {
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create a virtual store to contain all the photo files.
                if (!isolatedStorage.DirectoryExists(PhotosDirectory))
                {
                    isolatedStorage.CreateDirectory(PhotosDirectory);
                }

                // Create a file name for the photo file in the isolated storage.
                var newFileName = Path.Combine(PhotosDirectory, GenerateNewFileName());
                System.Diagnostics.Debug.WriteLine(newFileName);

                // Check for duplicate photo files.
                if (isolatedStorage.FileExists(newFileName))
                {
                    isolatedStorage.DeleteFile(newFileName);
                }

                var newFileStream = isolatedStorage.CreateFile(newFileName);

                // The photo photoWidth will never be larger than Canvas's photoWidth.
                var ratio = (double)Configs.CanvasSize.Width / bitmap.PixelWidth;
                var targetHeight = (int)Math.Round(bitmap.PixelHeight * ratio);

                // Downsample the photo.
                Extensions.SaveJpeg(bitmap, newFileStream, (int)Configs.CanvasSize.Width, targetHeight, 0, 100);

                newFileStream.Close();
                return newFileName;
            }
        }

        #endregion Methods
    }
}