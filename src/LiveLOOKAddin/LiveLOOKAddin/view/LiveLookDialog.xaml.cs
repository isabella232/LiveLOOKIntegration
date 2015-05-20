using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using ININ.Alliances.LiveLOOKAddin.Annotations;
using WpfConfiguratorLib;

namespace ININ.Alliances.LiveLOOKAddin.view
{
    public partial class LiveLookDialog : INotifyPropertyChanged
    {
        #region Private Members

        private string _joinUrl;
        private string _sessionId;

        #endregion



        #region Public Members

        public event PropertyChangedEventHandler PropertyChanged;

        public string JoinUrl
        {
            get { return _joinUrl; }
            set
            {
                _joinUrl = value;
                OnPropertyChanged();
                OnPropertyChanged("HasLink");
            }
        }

        public string SessionId
        {
            get { return _sessionId; }
            set
            {
                // Check for duplicates
                if (value == _sessionId) return;

                // Remove invalid characters and set value
                _sessionId = value != null
                    ? Regex.Replace(value, @"[^\d]", "")
                    : "";

                // Raise notifications
                OnPropertyChanged();
                OnPropertyChanged("IsSessionIdValid");
                OnPropertyChanged("HasAnySessionIdDigits");

                if (value.EndsWith("\r\n") && IsSessionIdValid)
                    StartSession();
            }
        }

        public bool HasLink
        {
            get { return !string.IsNullOrEmpty(JoinUrl); }
        }

        public bool IsSessionIdValid
        {
            get
            {
                int id;
                return
                    int.TryParse(SessionId, out id) &&
                    SessionId.Length == 6;
            }
        }

        public bool HasAnySessionIdDigits { get { return !string.IsNullOrEmpty(SessionId); } }

        #endregion



        public LiveLookDialog()
        {
            DataContext = this;

            InitializeComponent();

            SessionIdTextBox.Focus();
        }



        #region Private Methods

        private void StartSession()
        {
            try
            {
                // Make URL
                JoinUrl =
                    string.Format("https://www.livelook.com/new_agent.asp?pc={0}&login={1}&password={2}&agentname={3}",
                        SessionId, LiveLookButton.Username,
                        SecureStringSerializer.ConvertToUnsecureString(LiveLookButton.Password),
                        LiveLookButton.AgentName);

                // Launch in default browser
                Process.Start(JoinUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LiveLookAddin.AddinTracer.Exception(ex);
            }
        }

        private void StartSession_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StartSession();
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

        private void CopyLink_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(JoinUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LiveLookAddin.AddinTracer.Exception(ex);
            }
        }

        private void OpenLink_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(JoinUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LiveLookAddin.AddinTracer.Exception(ex);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



        #region Public Methods



        #endregion
    }
}
