namespace PicCanvas.Views.Behaviors
{
    using System.Windows;

    public struct Vector
    {
        #region Fields

        private double x;
        private double y;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Vector structure.
        /// </summary>
        /// <param name="x">The X-offset of the new Vector.</param>
        /// <param name="y">The Y-offset of the new Vector.</param>
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the X component of this vector.
        /// </summary>
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Gets or sets the Y component of this vector.
        /// </summary>
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Subtracts one specified point from another.
        /// </summary>
        /// <param name="point1">The point1 from which point2 is subtracted.</param>
        /// <param name="point2">The point2 to subtract from point1.</param>
        /// <returns>The difference between point1 and point2.</returns>
        public static Vector Subtruct(Point point1, Point point2)
        {
            return new Vector(point1.X - point2.X, point1.Y - point2.Y);
        }

        #endregion Methods
    }
}