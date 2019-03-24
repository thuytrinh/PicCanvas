namespace PicCanvas.Views
{
    using System;

    using Microsoft.Phone.Controls;

    using PicCanvas.ViewModels;

    public partial class CollagePage : PhoneApplicationPage
    {
        #region Fields

        private CollageViewModel _collageViewModel;

        #endregion Fields

        #region Constructors

        public CollagePage()
        {
            InitializeComponent();

            _collageViewModel = new CollageViewModel();
            CollageCanvas.DataContext = _collageViewModel;
        }

        #endregion Constructors

        #region Methods

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            _collageViewModel.AddPhotoFromPhotosHubCommand.Execute(null);
        }

        #endregion Methods
    }
}