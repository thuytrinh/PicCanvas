namespace PicCanvas.Views
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using PicCanvas.Models;

    public class CollageCanvas : Canvas
    {
        #region Fields

        public static readonly DependencyProperty PhotosSourceProperty = DependencyProperty.Register(
            PhotosSourcePropertyName,
            typeof(ObservableCollection<PhotoModel>),
            typeof(CollageCanvas),
            new PropertyMetadata(null, OnPhotosSourcePropertyChanged));

        private const double InertialThreshold = 2500.0;
        private const string PhotosSourcePropertyName = "PhotosSource";

        #endregion Fields

        #region Constructors

        public CollageCanvas()
        {
            Loaded += OnCollageCanvasLoaded;
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<PhotoModel> PhotosSource
        {
            get { return (ObservableCollection<PhotoModel>)GetValue(PhotosSourceProperty); }
            set { SetValue(PhotosSourceProperty, value); }
        }

        public DataTemplate PhotoTemplate
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        private static void OnPhotosSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var canvas = dependencyObject as CollageCanvas;

            var oldPhotosSource = e.OldValue as ObservableCollection<PhotoModel>;
            if (oldPhotosSource != null)
            {
                oldPhotosSource.CollectionChanged -= canvas.OnPhotosSourceCollectionChanged;
            }

            var newPhotosSource = e.NewValue as ObservableCollection<PhotoModel>;
            if (newPhotosSource != null)
            {
                newPhotosSource.CollectionChanged += canvas.OnPhotosSourceCollectionChanged;
            }
        }

        private void OnCollageCanvasLoaded(object sender, RoutedEventArgs e)
        {
            Configs.CanvasSize = new Size(ActualWidth, ActualHeight);
        }

        /// <summary>
        /// Deletes the selected photo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPhotoControlManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var photoControl = sender as PhotoControl;

            var vX = Math.Abs(e.FinalVelocities.LinearVelocity.X);
            var vY = Math.Abs(e.FinalVelocities.LinearVelocity.Y);
            if (e.IsInertial && (vX >= InertialThreshold || vY >= InertialThreshold))
            {
                photoControl.BeginFlickAnimation();
            }
        }

        /// <summary>
        /// Brings the selected photo to the front.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPhotoControlTap(object sender, GestureEventArgs e)
        {
            var photoControl = sender as PhotoControl;
            Children.Remove(photoControl);
            Children.Add(photoControl);

            photoControl.BeginTapAnimation();
        }

        private void OnPhotosSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var newPhotoControl = PhotoTemplate.LoadContent() as PhotoControl;
                        newPhotoControl.DataContext = item;
                        newPhotoControl.Tap += OnPhotoControlTap;
                        newPhotoControl.ManipulationCompleted += OnPhotoControlManipulationCompleted;
                        newPhotoControl.Refresh();

                        Children.Add(newPhotoControl);
                    }

                    break;
            }
        }

        #endregion Methods
    }
}