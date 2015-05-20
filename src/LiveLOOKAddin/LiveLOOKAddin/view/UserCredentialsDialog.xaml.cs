using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ININ.Alliances.LiveLOOKAddin.Annotations;
using ININ.Alliances.LiveLOOKAddin.model;
using WpfConfiguratorLib;

namespace ININ.Alliances.LiveLOOKAddin.view
{
    /// <summary>
    /// Interaction logic for UserCredentialsDialog.xaml
    /// </summary>
    public partial class UserCredentialsDialog : INotifyPropertyChanged
    {
        private UserCredentials _credentials = new UserCredentials();

        #region Private Members



        #endregion



        #region Public Members

        public event PropertyChangedEventHandler PropertyChanged;

        public UserCredentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }

        #endregion



        public UserCredentialsDialog(UserCredentials credentials)
        {
            // Used passed in value if not null
            if (credentials != null)
                Credentials = credentials;

            DataContext = this;

            InitializeComponent();
        }



        #region Private Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save credentials to disk
                ConfigManager.Save(Credentials);

                // Set credentials for use in addin
                LiveLookButton.SetCredentials(Credentials.Username, Credentials.Password);

                // Set success
                DialogResult = true;

                // Done. Close the window
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LiveLookAddin.AddinTracer.Exception(ex);
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LiveLookAddin.AddinTracer.Exception(ex);
            }
        }

        #endregion



        #region Public Methods



        #endregion
    }
}
