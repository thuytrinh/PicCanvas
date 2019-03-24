namespace PicCanvas.Views
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;
    using System.Windows.Interactivity;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;

    using PicCanvas.Views.Behaviors;

    public class PhotoControl : System.Windows.Shapes.Path
    {
        #region Fields

        public static readonly DependencyProperty PhotoFileNameProperty = DependencyProperty.Register(
            PhotoFileNamePropertyName,
            typeof(string),
            typeof(PhotoControl),
            new PropertyMetadata(null, OnPhotoFileNamePropertyChanged));

        private const double FlickRotateAnimationDelta = 360.0;
        private const double FlickScaleAnimationDelta = 0.3;
        private const int PhotoAnimationDuration = 170;
        private const string PhotoFileNamePropertyName = "PhotoFileName";
        private const double TapScaleAnimationDelta = 1.3;

        private DoubleAnimation _flickOpacityAnimation;
        private DoubleAnimation _flickRotateAnimation;
        private DoubleAnimation _flickScaleXAnimation;
        private DoubleAnimation _flickScaleYAnimation;
        private Storyboard _flickStoryboard;
        private bool _isAnimating;
        private MultiTouchBehavior _multiTouchBehavior;
        private DoubleAnimation _tapScaleXAnimation;
        private DoubleAnimation _tapScaleYAnimation;
        private Storyboard _tapStoryboard;

        #endregion Fields

        #region Constructors

        public PhotoControl()
        {
        }

        #endregion Constructors

        #region Properties

        public bool CanManipulate
        {
            get
            {
                return _multiTouchBehavior == null ? false : _multiTouchBehavior.CanManipulate;
            }

            set
            {
                // Lazy initialization.
                if (_multiTouchBehavior == null)
                {
                    _multiTouchBehavior = new MultiTouchBehavior();

                    var behaviors = Interaction.GetBehaviors(this);
                    behaviors.Add(_multiTouchBehavior);
                }

                _multiTouchBehavior.CanManipulate = value;
            }
        }

        public string PhotoFileName
        {
            get { return (string)GetValue(PhotoFileNameProperty); }
            set { SetValue(PhotoFileNameProperty, value); }
        }

        #endregion Properties

        #region Methods

        public void BeginFlickAnimation()
        {
            // Lazy initialization.
            if (_flickStoryboard == null)
            {
                CreateFlickAnimation();
            }

            if (_isAnimating)
            {
                return;
            }

            var transform = RenderTransform as CompositeTransform;

            _flickScaleXAnimation.From = transform.ScaleX;
            _flickScaleXAnimation.To = transform.ScaleX * FlickScaleAnimationDelta;

            _flickScaleYAnimation.From = transform.ScaleY;
            _flickScaleYAnimation.To = transform.ScaleY * FlickScaleAnimationDelta;

            _flickRotateAnimation.From = transform.Rotation;
            _flickRotateAnimation.To = transform.Rotation + FlickRotateAnimationDelta;

            _flickOpacityAnimation.From = Opacity;
            _flickOpacityAnimation.To = 0.0;

            CanManipulate = false;

            _isAnimating = true;
            _flickStoryboard.Begin();
        }

        public void BeginTapAnimation()
        {
            // Lazy initialization.
            if (_tapStoryboard == null)
            {
                CreateTapAnimation();
            }

            if (_isAnimating)
            {
                return;
            }

            var transform = RenderTransform as CompositeTransform;

            _tapScaleXAnimation.From = transform.ScaleX;
            _tapScaleXAnimation.To = transform.ScaleX * TapScaleAnimationDelta;

            _tapScaleYAnimation.From = transform.ScaleY;
            _tapScaleYAnimation.To = transform.ScaleY * TapScaleAnimationDelta;

            CanManipulate = false;

            _isAnimating = true;
            _tapStoryboard.Begin();
        }

        public void Refresh()
        {
            var loadPhotoFileWorker = new BackgroundWorker();
            loadPhotoFileWorker.DoWork += OnLoadPhotoFileWorkerDoWork;
            loadPhotoFileWorker.RunWorkerCompleted += OnLoadPhotoFileWorkerRunWorkerCompleted;
            loadPhotoFileWorker.RunWorkerAsync(PhotoFileName);
        }

        private static void OnPhotoFileNamePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
        }

        private void CreateFlickAnimation()
        {
            _flickScaleXAnimation = new DoubleAnimation();
            _flickScaleXAnimation.Duration = TimeSpan.FromMilliseconds(PhotoAnimationDuration);

            _flickScaleYAnimation = new DoubleAnimation();
            _flickScaleYAnimation.Duration = TimeSpan.FromMilliseconds(PhotoAnimationDuration);

            _flickRotateAnimation = new DoubleAnimation();
            _flickRotateAnimation.Duration = TimeSpan.FromMilliseconds(PhotoAnimationDuration);

            _flickOpacityAnimation = new DoubleAnimation();
            _flickOpacityAnimation.Duration = TimeSpan.FromMilliseconds(PhotoAnimationDuration);

            Storyboard.SetTarget(_flickScaleXAnimation, RenderTransform);
            Storyboard.SetTargetProperty(_flickScaleXAnimation, new PropertyPath("CompositeTransform.ScaleX"));

            Storyboard.SetTarget(_flickScaleYAnimation, RenderTransform);
            Storyboard.SetTargetProperty(_flickScaleYAnimation, new PropertyPath("CompositeTransform.ScaleY"));

            Storyboard.SetTarget(_flickRotateAnimation, RenderTransform);
            Storyboard.SetTargetProperty(_flickRotateAnimation, new PropertyPath("CompositeTransform.Rotation"));

            Storyboard.SetTarget(_flickOpacityAnimation, this);
            Storyboard.SetTargetProperty(_flickOpacityAnimation, new PropertyPath("Opacity"));

            _flickStoryboard = new Storyboard();
            _flickStoryboard.Completed += OnStoryboardCompleted;
            _flickStoryboard.AutoReverse = true;
            _flickStoryboard.Children.Add(_flickScaleXAnimation);
            _flickStoryboard.Children.Add(_flickScaleYAnimation);
            _flickStoryboard.Children.Add(_flickRotateAnimation);
            _flickStoryboard.Children.Add(_flickOpacityAnimation);
        }

        private void CreateTapAnimation()
        {
            _tapScaleXAnimation = new DoubleAnimation();
            _tapScaleXAnimation.Duration = TimeSpan.FromMilliseconds(PhotoAnimationDuration);

            _tapScaleYAnimation = new DoubleAnimation();
            _tapScaleYAnimation.Duration = TimeSpan.FromMilliseconds(PhotoAnimationDuration);

            Storyboard.SetTarget(_tapScaleXAnimation, RenderTransform);
            Storyboard.SetTargetProperty(_tapScaleXAnimation, new PropertyPath("CompositeTransform.ScaleX"));

            Storyboard.SetTarget(_tapScaleYAnimation, RenderTransform);
            Storyboard.SetTargetProperty(_tapScaleYAnimation, new PropertyPath("CompositeTransform.ScaleY"));

            _tapStoryboard = new Storyboard();
            _tapStoryboard.AutoReverse = true;
            _tapStoryboard.Completed += OnStoryboardCompleted;
            _tapStoryboard.Children.Add(_tapScaleXAnimation);
            _tapStoryboard.Children.Add(_tapScaleYAnimation);
        }

        private Stream LoadPhotoStreamFromIsolatedStorage(string fileName)
        {
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isolatedStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read);
            }
        }

        private void OnLoadPhotoFileWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = LoadPhotoStreamFromIsolatedStorage(e.Argument as string);
        }

        /// <summary>
        /// Shows and positions the photo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadPhotoFileWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            var photoStream = e.Result as Stream;
            if (photoStream == null)
            {
                return;
            }

            var imageSource = new BitmapImage();
            imageSource.SetSource(photoStream);

            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = imageSource;
            Fill = imageBrush;

            // Update the geometry so that the photo can be visible on Canvas.
            if (Data == null)
            {
                var rectangleGeometry = new RectangleGeometry();
                rectangleGeometry.Rect = new Rect(0, 0, imageSource.PixelWidth, imageSource.PixelHeight);
                Data = rectangleGeometry;

                SetDefaultPosition(imageSource.PixelWidth, imageSource.PixelHeight);
            }

            CanManipulate = true;
        }

        /// <summary>
        /// Enables manipulation after the animation completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStoryboardCompleted(object sender, EventArgs e)
        {
            _isAnimating = false;
            CanManipulate = true;
        }

        /// <summary>
        /// Aligns the photo with the center of the canvas.
        /// </summary>
        /// <param name="photoWidth"></param>
        /// <param name="photoHeight"></param>
        private void SetDefaultPosition(double photoWidth, double photoHeight)
        {
            var renderTransform = RenderTransform as CompositeTransform;
            renderTransform.CenterX = photoWidth / 2;
            renderTransform.CenterY = photoHeight / 2;
            renderTransform.Rotation = (new Random()).Next(-30, 30);
            renderTransform.TranslateX = (Configs.CanvasSize.Width - photoWidth) / 2;
            renderTransform.TranslateY = (Configs.CanvasSize.Height - photoHeight) / 2;
        }

        #endregion Methods
    }
}