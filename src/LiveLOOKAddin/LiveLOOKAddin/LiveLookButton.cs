using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using ININ.Alliances.LiveLOOKAddin.model;
using ININ.Alliances.LiveLOOKAddin.view;
using ININ.Client.Common.Interactions;
using ININ.IceLib.Configuration;
using ININ.IceLib.Connection;
using ININ.InteractionClient.Interactions;
using WpfConfiguratorLib;

namespace ININ.Alliances.LiveLOOKAddin
{
    public class LiveLookButton : IInteractionButton
    {
        #region Private Fields

        private readonly Session _session;
        private static string _agentName = "Agent";
        private SynchronizationContext _context;
        private BackgroundWorker _loaderWorker = new BackgroundWorker();

        #endregion



        #region Public Properties

        public string Id
        {
            get { return "LiveLOOK_BUTTON"; }
        }

        public Icon Icon
        {
            get { return Resources.logo; }
        }

        public string Text
        {
            get { return "LiveLOOK"; }
        }

        public string ToolTipText
        {
            get { return "Click to start a new LiveLOOK session"; }
        }

        public SupportedInteractionTypes SupportedInteractionTypes
        {
            get { return SupportedInteractionTypes.All; }
        }

        public static string AgentName
        {
            get { return _agentName; }
            private set { _agentName = value; }
        }

        public static string Username { get; private set; }

        public static SecureString Password { get; private set; }

        #endregion



        public LiveLookButton(Session session)
        {
            using (LiveLookAddin.AddinTracer.Scope("LiveLookButton.ctor"))
            {
                try
                {
                    _context = SynchronizationContext.Current;
                    _loaderWorker.DoWork += LoaderWorkerOnDoWork;
                    _loaderWorker.RunWorkerAsync();

                    _session = session;

                    LiveLookAddin.AddinTracer.Verbose("Config working directory: {}", ConfigManager.WorkingDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    LiveLookAddin.AddinTracer.Exception(ex);
                    MessageBox.Show(
                        "A critical error was encountered on initilization. The LiveLOOK integration will not function. " +
                        "Please contact your system administrator.\n\nError message: " + ex.Message,
                        "LiveLOOK Integration Initialization Failure!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Private Methods

        private void LoaderWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                GetAgentName();

                /* Sleep for 4 seconds to let the client finish initializing. This prevents the dialog 
                 * from showing behind the client. Note that if the user clicks the button before this
                 * has fired, they will be prompted for their credentials. That won't hurt anything.
                 */
                Thread.Sleep(4000);

                // Load credentials
                LoadCredentials();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading credentials!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetExceptionDetails(Exception exception)
        {
            var properties = exception.GetType()
                                    .GetProperties();
            var fields = properties
                             .Select(property => new
                             {
                                 Name = property.Name,
                                 Value = property.GetValue(exception, null)
                             })
                             .Select(x => String.Format(
                                 "{0} = {1}",
                                 x.Name,
                                 x.Value != null ? x.Value.ToString() : String.Empty
                             ));
            return String.Join("\n", fields);
        }

        private void GetAgentName()
        {
            // Create list
            var userConfigurationList = new UserConfigurationList(ConfigurationManager.GetInstance(_session));

            // Create query
            var query = userConfigurationList.CreateQuerySettings();
            query.SetPropertiesToRetrieve(UserConfiguration.Property.Id, UserConfiguration.Property.Mailbox_DisplayName);
            query.SetRightsFilter(UserConfiguration.Rights.LoggedInUser);
            query.SetFilterDefinition(UserConfiguration.Property.Id, _session.UserId);

            // Get list
            userConfigurationList.StartCaching(query);
            var userList = userConfigurationList.GetConfigurationList();
            
            // Get user
            var user =
                userList.FirstOrDefault(
                    u =>
                        u.ConfigurationId.Id.ToString()
                            .Equals(_session.UserId, StringComparison.InvariantCultureIgnoreCase));

            if (user == null)
            {
                AgentName = "Agent";
            }
            else
            {
                // Set name (the display name is always the mailbox name even if there is no mailbox)
                AgentName = string.IsNullOrEmpty(user.Mailbox.DisplayName.Value)
                    ? "Agent"
                    : user.Mailbox.DisplayName.Value;
            }

            // Stop caching
            userConfigurationList.StopCaching();
        }

        private void LoadCredentials()
        {
            // Try to load from disk
            var credentials = ConfigManager.Load<UserCredentials>("LiveLOOK Credentials");

            // Prompt if we didn't get anything
            if (credentials == null)
            {
                VerifyAndGetCredentials(true);
                return;
            }

            // Set local properties
            Username = credentials.Username;
            Password = credentials.Password;
        }

        private bool VerifyAndGetCredentials(bool askForCredentials = false)
        {
            // Verify credentials
            if (string.IsNullOrEmpty(Username) || Password == null || Password == default(SecureString))
            {
                // Skip?
                if (!askForCredentials) return false;

                var gotCreds = false;
                _context.Send(s =>
                {
                    try
                    {
                        // Show dialog to get credentials (returns true if saved)
                        var dialog = new UserCredentialsDialog(null);
                        gotCreds = dialog.ShowDialog() == true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error initializing credentials!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }, null);

                return gotCreds;
            }
            return true;
        }

        #endregion



        #region Public Methods

        public bool CanExecute(IInteraction selectedInteraction)
        {
            // Always enable so user can join a session without regard to interactions since we don't do anything with the interaction
            return true;
        }

        public void Execute(IInteraction selectedInteraction)
        {
            try
            {
                // Check credentials
                if (!VerifyAndGetCredentials(true)) return;

                // Show LiveLOOK window
                new LiveLookDialog().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error executing LiveLOOK Button", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SetCredentials(string username, SecureString password)
        {
            try
            {
                Username = username;
                Password = password;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LiveLookAddin.AddinTracer.Exception(ex);
            }
        }

        #endregion
    }
}
