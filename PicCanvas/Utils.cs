namespace PicCanvas
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using PicCanvas.Views.Behaviors;

    public class Utils
    {
        #region Methods

        public static void AdjustCompositeTransformCenter(FrameworkElement element, Point newCenter)
        {
            var parent = element.Parent as Panel;
            if (parent == null)
            {
                throw new ArgumentException();
            }

            var compositeTransform = element.RenderTransform as CompositeTransform;
            if (compositeTransform == null)
            {
                throw new ArgumentException();
            }

            var prevPoint = compositeTransform.Transform(new Point());

            var center = parent.TransformToVisual(element).Transform(newCenter);
            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            var currPoint = compositeTransform.Transform(new Point());

            var offset = Vector.Subtruct(currPoint, prevPoint);
            compositeTransform.TranslateX -= offset.X;
            compositeTransform.TranslateY -= offset.Y;
        }

        public static void ResetCompositeTransformCenter(FrameworkElement element, Point initialCenter)
        {
            var parent = element.Parent as Panel;
            if (parent == null)
            {
                throw new ArgumentException();
            }

            var center = element.TransformToVisual(parent).Transform(initialCenter);
            AdjustCompositeTransformCenter(element, center);
        }

        #endregion Methods
    }
}