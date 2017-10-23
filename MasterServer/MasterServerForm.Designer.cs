using System.ComponentModel;
using System.Windows.Forms;

namespace MasterServer
{
    partial class MasterServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MasterServerForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblServers = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.serversGridView = new System.Windows.Forms.DataGridView();
            this.serverBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Version = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.serversGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.serverBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Master server is still running...";
            this.notifyIcon.BalloonTipTitle = "Master server";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // tbStatus
            // 
            this.tbStatus.Location = new System.Drawing.Point(15, 84);
            this.tbStatus.Multiline = true;
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbStatus.Size = new System.Drawing.Size(996, 435);
            this.tbStatus.TabIndex = 0;
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(90, 30);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(73, 22);
            this.tbPort.TabIndex = 2;
            this.tbPort.Text = "6005";
            this.tbPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbPort_KeyPress);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(13, 33);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(71, 17);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "UDP Port:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(13, 64);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(52, 17);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status:";
            // 
            // lblServers
            // 
            this.lblServers.AutoSize = true;
            this.lblServers.Location = new System.Drawing.Point(12, 522);
            this.lblServers.Name = "lblServers";
            this.lblServers.Size = new System.Drawing.Size(61, 17);
            this.lblServers.TabIndex = 5;
            this.lblServers.Text = "Servers:";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(169, 25);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(108, 32);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(293, 19);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(723, 51);
            this.lblInfo.TabIndex = 7;
            this.lblInfo.Text = resources.GetString("lblInfo.Text");
            // 
            // serversGridView
            // 
            this.serversGridView.AllowUserToAddRows = false;
            this.serversGridView.AllowUserToDeleteRows = false;
            this.serversGridView.AllowUserToResizeRows = false;
            this.serversGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.serversGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Address,
            this.Version,
            this.ServerName,
            this.Description});
            this.serversGridView.Location = new System.Drawing.Point(15, 543);
            this.serversGridView.Name = "serversGridView";
            this.serversGridView.RowTemplate.Height = 24;
            this.serversGridView.Size = new System.Drawing.Size(996, 223);
            this.serversGridView.TabIndex = 8;
            // 
            // Address
            // 
            this.Address.HeaderText = "Address";
            this.Address.Name = "Address";
            this.Address.ReadOnly = true;
            this.Address.Width = 120;
            // 
            // Version
            // 
            this.Version.HeaderText = "Version";
            this.Version.Name = "Version";
            this.Version.ReadOnly = true;
            this.Version.Width = 50;
            // 
            // ServerName
            // 
            this.ServerName.HeaderText = "Name";
            this.ServerName.Name = "ServerName";
            this.ServerName.ReadOnly = true;
            this.ServerName.Width = 200;
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Width = 330;
            // 
            // MasterServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1023, 792);
            this.Controls.Add(this.serversGridView);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblServers);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.tbPort);
            this.Controls.Add(this.tbStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MasterServerForm";
            this.Text = "Master server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MasterServerForm_FormClosed);
            this.Resize += new System.EventHandler(this.MasterServerForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.serversGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.serverBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NotifyIcon notifyIcon;
        private TextBox tbStatus;
        private TextBox tbPort;
        private Label lblPort;
        private Label lblStatus;
        private Label lblServers;
        private Button btnStart;
        private Label lblInfo;
        private DataGridView serversGridView;
        private BindingSource serverBindingSource;
        private DataGridViewTextBoxColumn Address;
        private DataGridViewTextBoxColumn Version;
        private DataGridViewTextBoxColumn ServerName;
        private DataGridViewTextBoxColumn Description;
    }
}

