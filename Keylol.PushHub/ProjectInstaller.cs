using System.ComponentModel;
using System.Configuration.Install;

namespace Keylol.PushHub
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