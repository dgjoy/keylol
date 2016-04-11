using System.ComponentModel;
using System.Configuration.Install;

namespace Keylol.ImageGarage
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