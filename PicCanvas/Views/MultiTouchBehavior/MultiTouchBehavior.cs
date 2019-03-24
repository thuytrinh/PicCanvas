namespace PicCanvas.Views.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input.Manipulations;
    using System.Windows.Interactivity;
    using System.Windows.Media;

    public class MultiTouchBehavior : Behavior<FrameworkElement>
    {
        #region Fields

        private Point _initialCenter;
        private ManipulationProcessor2D _manipulationProcessor;
        private CompositeTransform _renderTransform;

        #endregion Fields

        #region Constructors

        public MultiTouchBehavior()
        {
            CanManipulate = true;
            CanRotate = true;
            CanScale = true;
            CanTranslate = true;
        }

        #endregion Constructors

        #region Properties

        public bool CanManipulate
        {
            get;
            set;
        }

        public bool CanRotate
        {
            get;
            set;
        }

        public bool CanScale
        {
            get;
            set;
        }

        public bool CanTranslate
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            // Initializes the manipulation processor.
            _manipulationProcessor = new ManipulationProcessor2D(Manipulations2D.All);
            _manipulationProcessor.Started += OnManipulationStarted;
            _manipulationProcessor.Delta += OnManipulationDelta;
            _manipulationProcessor.Completed += OnManipulationCompleted;

            // Initializes the touch helper.
            TouchHelper.AddHandlers(AssociatedObject, new TouchHandlers
            {
                TouchDown = OnTouchDown,
                CapturedTouchReported = OnCapturedTouchReported
            });
            TouchHelper.EnableInput(true);
            TouchHelper.SetRootElement(TouchHelper.GetRootElement(AssociatedObject));

            _renderTransform = AssociatedObject.RenderTransform as CompositeTransform;
            if (_renderTransform == null)
            {
                throw new InvalidCastException();
            }

            _initialCenter = new Point(_renderTransform.CenterX, _renderTransform.CenterY);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            _manipulationProcessor.Started -= OnManipulationStarted;
            _manipulationProcessor.Delta -= OnManipulationDelta;
            _manipulationProcessor.Completed -= OnManipulationCompleted;

            TouchHelper.RemoveHandlers(AssociatedObject);
        }

        private void Move(Point manipulationOrigin, Vector deltaTranslation, double deltaRotationInRadians, double deltaScale)
        {
            Utils.AdjustCompositeTransformCenter(AssociatedObject, manipulationOrigin);

            if (CanTranslate)
            {
                _renderTransform.TranslateX += deltaTranslation.X;
                _renderTransform.TranslateY += deltaTranslation.Y;
            }

            if (CanRotate)
            {
                var deltaRotationInDegrees = deltaRotationInRadians * 180.0 / Math.PI;
                _renderTransform.Rotation += deltaRotationInDegrees;
            }

            if (CanScale)
            {
                _renderTransform.ScaleX *= deltaScale;
                _renderTransform.ScaleY *= deltaScale;
            }
        }

        private void OnCapturedTouchReported(object sender, TouchReportedEventArgs e)
        {
            var parent = AssociatedObject.Parent as UIElement;
            if (parent == null)
            {
                return;
            }

            var root = TouchHelper.GetRootElement(parent);
            if (root == null)
            {
                return;
            }

            List<Manipulator2D> manipulators = null;

            if (e.TouchPoints.FirstOrDefault() != null)
            {
                // Get transformation to convert positions to the parent's coordinate system.
                var transform = root.TransformToVisual(parent);
                foreach (var touchPoint in e.TouchPoints)
                {
                    var position = touchPoint.Position;

                    // Convert to the parent's coordinate system.
                    position = transform.Transform(position);

                    // Create a manipulator.
                    var manipulator = new Manipulator2D(
                        touchPoint.TouchDevice.Id,
                        (float)(position.X),
                        (float)(position.Y));

                    if (manipulators == null)
                    {
                        // Lazy initialization.
                        manipulators = new List<Manipulator2D>();
                    }

                    manipulators.Add(manipulator);
                }
            }

            // Process manipulations.
            _manipulationProcessor.ProcessManipulators(DateTime.UtcNow.Ticks, manipulators);
        }

        /// <summary>
        /// Here when manipulation completes.
        /// </summary>
        private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e)
        {
            Utils.ResetCompositeTransformCenter(AssociatedObject, _initialCenter);
        }

        /// <summary>
        /// Here when manipulation gives a delta.
        /// </summary>
        private void OnManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            if (!CanManipulate)
            {
                return;
            }

            Move(new Point(e.OriginX, e.OriginY), new Vector(e.Delta.TranslationX, e.Delta.TranslationY), e.Delta.Rotation, e.Delta.ScaleX);
        }

        /// <summary>
        /// Here when manipulation starts.
        /// </summary>
        private void OnManipulationStarted(object sender, Manipulation2DStartedEventArgs e)
        {
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            e.TouchPoint.TouchDevice.Capture(AssociatedObject);
        }

        #endregion Methods
    }
}