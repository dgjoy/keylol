namespace Keylol.CruatorSpammer
{
    partial class Main
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.groupLinkCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.groupLinkTextBox = new System.Windows.Forms.TextBox();
            this.addButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.deleteButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.overrideExsitedCheckBox = new System.Windows.Forms.CheckBox();
            this.publishButton = new System.Windows.Forms.Button();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.contextTextBox = new System.Windows.Forms.TextBox();
            this.appNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.appIdTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.friendAppIdTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.friendStartButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser
            // 
            this.webBrowser.Dock = System.Windows.Forms.DockStyle.Left;
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.Margin = new System.Windows.Forms.Padding(5);
            this.webBrowser.MinimumSize = new System.Drawing.Size(36, 31);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.ScriptErrorsSuppressed = true;
            this.webBrowser.Size = new System.Drawing.Size(1045, 793);
            this.webBrowser.TabIndex = 0;
            this.webBrowser.Url = new System.Uri("http://steamcommunity.com/groups/keylol-player-club#curation", System.UriKind.Absolute);
            // 
            // groupLinkCheckedListBox
            // 
            this.groupLinkCheckedListBox.FormattingEnabled = true;
            this.groupLinkCheckedListBox.Location = new System.Drawing.Point(12, 15);
            this.groupLinkCheckedListBox.Margin = new System.Windows.Forms.Padding(5);
            this.groupLinkCheckedListBox.Name = "groupLinkCheckedListBox";
            this.groupLinkCheckedListBox.Size = new System.Drawing.Size(442, 137);
            this.groupLinkCheckedListBox.TabIndex = 1;
            // 
            // groupLinkTextBox
            // 
            this.groupLinkTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupLinkTextBox.Location = new System.Drawing.Point(12, 169);
            this.groupLinkTextBox.Margin = new System.Windows.Forms.Padding(5);
            this.groupLinkTextBox.Name = "groupLinkTextBox";
            this.groupLinkTextBox.Size = new System.Drawing.Size(276, 30);
            this.groupLinkTextBox.TabIndex = 2;
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(298, 169);
            this.addButton.Margin = new System.Windows.Forms.Padding(5);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(60, 30);
            this.addButton.TabIndex = 3;
            this.addButton.Text = "添加";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 273);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(279, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "先在左侧窗口登录 Steam，然后在下面发布";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 214);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(283, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "在上方输入框输入 Steam 群组链接，形如：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 240);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(381, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "http://steamcommunity.com/groups/keylol-player-club";
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(368, 169);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(5);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(86, 30);
            this.deleteButton.TabIndex = 4;
            this.deleteButton.Text = "删除选中";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.overrideExsitedCheckBox);
            this.groupBox1.Controls.Add(this.publishButton);
            this.groupBox1.Controls.Add(this.urlTextBox);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.contextTextBox);
            this.groupBox1.Controls.Add(this.appNameTextBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.appIdTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(15, 306);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(439, 386);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "发布鉴赏家推荐";
            // 
            // overrideExsitedCheckBox
            // 
            this.overrideExsitedCheckBox.AutoSize = true;
            this.overrideExsitedCheckBox.Checked = true;
            this.overrideExsitedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.overrideExsitedCheckBox.Location = new System.Drawing.Point(17, 340);
            this.overrideExsitedCheckBox.Name = "overrideExsitedCheckBox";
            this.overrideExsitedCheckBox.Size = new System.Drawing.Size(157, 24);
            this.overrideExsitedCheckBox.TabIndex = 10;
            this.overrideExsitedCheckBox.Text = "覆盖已经存在的推荐";
            this.overrideExsitedCheckBox.UseVisualStyleBackColor = true;
            // 
            // publishButton
            // 
            this.publishButton.Location = new System.Drawing.Point(270, 329);
            this.publishButton.Name = "publishButton";
            this.publishButton.Size = new System.Drawing.Size(150, 40);
            this.publishButton.TabIndex = 9;
            this.publishButton.Text = "发布（152）";
            this.publishButton.UseVisualStyleBackColor = true;
            this.publishButton.Click += new System.EventHandler(this.publishButton_Click);
            // 
            // urlTextBox
            // 
            this.urlTextBox.Location = new System.Drawing.Point(98, 101);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(322, 24);
            this.urlTextBox.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(57, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 20);
            this.label6.TabIndex = 5;
            this.label6.Text = "URL";
            // 
            // contextTextBox
            // 
            this.contextTextBox.ImeMode = System.Windows.Forms.ImeMode.On;
            this.contextTextBox.Location = new System.Drawing.Point(17, 135);
            this.contextTextBox.Multiline = true;
            this.contextTextBox.Name = "contextTextBox";
            this.contextTextBox.Size = new System.Drawing.Size(403, 178);
            this.contextTextBox.TabIndex = 8;
            this.contextTextBox.TextChanged += new System.EventHandler(this.contextTextBox_TextChanged);
            // 
            // appNameTextBox
            // 
            this.appNameTextBox.Location = new System.Drawing.Point(98, 67);
            this.appNameTextBox.Name = "appNameTextBox";
            this.appNameTextBox.Size = new System.Drawing.Size(322, 24);
            this.appNameTextBox.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 20);
            this.label5.TabIndex = 2;
            this.label5.Text = "App Name";
            // 
            // appIdTextBox
            // 
            this.appIdTextBox.Location = new System.Drawing.Point(98, 33);
            this.appIdTextBox.Name = "appIdTextBox";
            this.appIdTextBox.Size = new System.Drawing.Size(322, 24);
            this.appIdTextBox.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 20);
            this.label4.TabIndex = 0;
            this.label4.Text = "App ID";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupLinkCheckedListBox);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.groupLinkTextBox);
            this.panel1.Controls.Add(this.deleteButton);
            this.panel1.Controls.Add(this.addButton);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(1043, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(472, 793);
            this.panel1.TabIndex = 9;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.friendStartButton);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.friendAppIdTextBox);
            this.groupBox2.Location = new System.Drawing.Point(15, 698);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(439, 83);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "批量加好友";
            // 
            // friendAppIdTextBox
            // 
            this.friendAppIdTextBox.Location = new System.Drawing.Point(98, 37);
            this.friendAppIdTextBox.Name = "friendAppIdTextBox";
            this.friendAppIdTextBox.Size = new System.Drawing.Size(162, 24);
            this.friendAppIdTextBox.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(36, 40);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 20);
            this.label7.TabIndex = 1;
            this.label7.Text = "App ID";
            // 
            // friendStartButton
            // 
            this.friendStartButton.Location = new System.Drawing.Point(270, 30);
            this.friendStartButton.Name = "friendStartButton";
            this.friendStartButton.Size = new System.Drawing.Size(150, 38);
            this.friendStartButton.TabIndex = 2;
            this.friendStartButton.Text = "启动";
            this.friendStartButton.UseVisualStyleBackColor = true;
            this.friendStartButton.Click += new System.EventHandler(this.friendStartButton_Click);
            // 
            // Main
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1515, 793);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.webBrowser);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimumSize = new System.Drawing.Size(500, 749);
            this.Name = "Main";
            this.Text = "Steam 鉴赏家推广工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.CheckedListBox groupLinkCheckedListBox;
        private System.Windows.Forms.TextBox groupLinkTextBox;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button publishButton;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox contextTextBox;
        private System.Windows.Forms.TextBox appNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox overrideExsitedCheckBox;
        private System.Windows.Forms.TextBox appIdTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button friendStartButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox friendAppIdTextBox;
    }
}

