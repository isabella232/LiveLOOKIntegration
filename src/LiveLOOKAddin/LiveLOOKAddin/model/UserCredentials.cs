using System.Security;
using System.Windows.Media;
using WpfConfiguratorLib;
using WpfConfiguratorLib.attributes;
using WpfConfiguratorLib.entities;

namespace ININ.Alliances.LiveLOOKAddin.model
{
    public class UserCredentials : ConfigGroup
    {
        public override string DisplayName
        {
            get { return "LiveLOOK Credentials"; }
        }

        public override string Description
        {
            get { return "Credentials for your LiveLOOK account"; }
        }

        public override SolidColorBrush Brush { get { return new SolidColorBrush(Colors.Transparent); } }

        [ConfigProperty("Username", DefaultValue = "", Description = "LiveLOOK username")]
        public string Username { get; set; }

        [ConfigProperty("Password", Description = "LiveLOOK password", DefaultValue = default(SecureString))]
        public SecureString Password { get; set; }

        public override string ToString()
        {
            return string.Format("username: {0}; password: {1}", Username,
                "".PadLeft(SecureStringSerializer.ConvertToUnsecureString(Password).Length, '*'));
        }
    }
}
