namespace PicCanvas
{
    using System.Windows;

    public class Configs
    {
        #region Fields

        public const string PhotoFileNamePattern = "MMddyyHmmss";

        #endregion Fields

        #region Properties

        public static Size CanvasSize
        {
            get;
            set;
        }

        #endregion Properties
    }
}