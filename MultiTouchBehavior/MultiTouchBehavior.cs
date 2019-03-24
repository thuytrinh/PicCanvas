namespace ThuyTrinh.Behaviors
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

        private ManipulationProcessor2D _manipulationProcessor;
        private CompositeTransform _renderTransform;

        #endregion Fields

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

            CanRotate = true;
            CanTranslate = true;
            CanScale = true;
            CanManipulate = true;

            _manipulationProcessor = new ManipulationProcessor2D(Manipulations2D.All);
            _manipulationProcessor.Started += OnManipulationStarted;
            _manipulationProcessor.Delta += OnManipulationDelta;
            _manipulationProcessor.Completed += OnManipulationCompleted;

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
                _renderTransform = new CompositeTransform();
                AssociatedObject.RenderTransform = _renderTransform;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            _manipulationProcessor.Started -= OnManipulationStarted;
            _manipulationProcessor.Delta -= OnManipulationDelta;
            _manipulationProcessor.Completed -= OnManipulationCompleted;

            TouchHelper.RemoveHandlers(AssociatedObject);
        }

        private void AdjustRenderTransformOrigin(Point newOrigin)
        {
            var parent = AssociatedObject.Parent as UIElement;
            if (parent == null)
            {
                return;
            }

            var prevPoint = _renderTransform.Transform(new Point());

            var transform = parent.TransformToVisual(AssociatedObject);
            var center = transform.Transform(newOrigin);
            _renderTransform.CenterX = center.X;
            _renderTransform.CenterY = center.Y;

            var currPoint = _renderTransform.Transform(new Point());

            var offset = Vector.Subtruct(currPoint, prevPoint);
            _renderTransform.TranslateX -= offset.X;
            _renderTransform.TranslateY -= offset.Y;
        }

        private void Move(Point manipulationOrigin, Vector deltaTranslation, double deltaRotationInRadians, double deltaScale)
        {
            AdjustRenderTransformOrigin(manipulationOrigin);

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
            if (!CanManipulate)
            {
                return;
            }
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            e.TouchPoint.TouchDevice.Capture(AssociatedObject);
        }

        #endregion Methods
    }
}