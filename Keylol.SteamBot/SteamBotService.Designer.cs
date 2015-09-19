namespace Keylol.SteamBot
{
    partial class SteamBotService
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
            this._eventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this._eventLog)).BeginInit();
            // 
            // _eventLog
            // 
            this._eventLog.Log = "Application";
            this._eventLog.Source = "Keylol Steam Bot";
            // 
            // SteamBotService
            // 
            this.ServiceName = "SteamBotService";
            ((System.ComponentModel.ISupportInitialize)(this._eventLog)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog _eventLog;
    }
}
