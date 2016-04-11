namespace Keylol.ImageGarage
{
    partial class ServiceInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.installer = new System.ServiceProcess.ServiceInstaller();
            // 
            // processInstaller
            // 
            this.processInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.processInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.installer});
            this.processInstaller.Password = null;
            this.processInstaller.Username = null;
            // 
            // installer
            // 
            this.installer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.processInstaller});

        }

        #endregion

        public System.ServiceProcess.ServiceProcessInstaller processInstaller;
        public System.ServiceProcess.ServiceInstaller installer;
    }
}