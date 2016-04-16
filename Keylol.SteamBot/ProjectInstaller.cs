using System.ComponentModel;
using System.Configuration.Install;

namespace Keylol.SteamBot
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
