using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace ConnPoolDesign
{
	/// <summary>
	/// Zusammendfassende Beschreibung für Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonRequest;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkboxShared;
		private System.Windows.Forms.CheckBox checkboxPooled;
		private System.Windows.Forms.TextBox textboxConnectionString;
		private System.Windows.Forms.ListBox listboxUsedConnectors;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonReleaseConnector;
		private System.Windows.Forms.Panel panel1;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();

			//
			// TODO: Fügen Sie den Konstruktorcode nach dem Aufruf von InitializeComponent hinzu
			//
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonRequest = new System.Windows.Forms.Button();
			this.textboxConnectionString = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkboxShared = new System.Windows.Forms.CheckBox();
			this.checkboxPooled = new System.Windows.Forms.CheckBox();
			this.listboxUsedConnectors = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonReleaseConnector = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// buttonRequest
			// 
			this.buttonRequest.Location = new System.Drawing.Point(496, 24);
			this.buttonRequest.Name = "buttonRequest";
			this.buttonRequest.Size = new System.Drawing.Size(120, 23);
			this.buttonRequest.TabIndex = 0;
			this.buttonRequest.Text = "Request Connector";
			this.buttonRequest.Click += new System.EventHandler(this.buttonRequest_Click);
			// 
			// textboxConnectionString
			// 
			this.textboxConnectionString.Location = new System.Drawing.Point(112, 24);
			this.textboxConnectionString.Name = "textboxConnectionString";
			this.textboxConnectionString.Size = new System.Drawing.Size(168, 20);
			this.textboxConnectionString.TabIndex = 1;
			this.textboxConnectionString.Text = "DSN=MyServer; UID=MyAccount; PWD=MyPassword;";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Connection String:";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// checkboxShared
			// 
			this.checkboxShared.Location = new System.Drawing.Point(296, 24);
			this.checkboxShared.Name = "checkboxShared";
			this.checkboxShared.Size = new System.Drawing.Size(72, 24);
			this.checkboxShared.TabIndex = 3;
			this.checkboxShared.Text = "Shared";
			// 
			// checkboxPooled
			// 
			this.checkboxPooled.Checked = true;
			this.checkboxPooled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkboxPooled.Location = new System.Drawing.Point(376, 24);
			this.checkboxPooled.Name = "checkboxPooled";
			this.checkboxPooled.Size = new System.Drawing.Size(72, 24);
			this.checkboxPooled.TabIndex = 4;
			this.checkboxPooled.Text = "Pooled";
			// 
			// listboxUsedConnectors
			// 
			this.listboxUsedConnectors.Location = new System.Drawing.Point(112, 72);
			this.listboxUsedConnectors.Name = "listboxUsedConnectors";
			this.listboxUsedConnectors.Size = new System.Drawing.Size(360, 134);
			this.listboxUsedConnectors.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Connectors:";
			// 
			// buttonReleaseConnector
			// 
			this.buttonReleaseConnector.Location = new System.Drawing.Point(496, 184);
			this.buttonReleaseConnector.Name = "buttonReleaseConnector";
			this.buttonReleaseConnector.Size = new System.Drawing.Size(120, 23);
			this.buttonReleaseConnector.TabIndex = 7;
			this.buttonReleaseConnector.Text = "Release Connector";
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(8, 56);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(608, 4);
			this.panel1.TabIndex = 8;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 221);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1,
																		  this.buttonReleaseConnector,
																		  this.label2,
																		  this.listboxUsedConnectors,
																		  this.checkboxPooled,
																		  this.checkboxShared,
																		  this.label1,
																		  this.textboxConnectionString,
																		  this.buttonRequest});
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		/// <summary>
		/// Requests a new connector object from the connector 
		/// pool manager.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonRequest_Click(object sender, System.EventArgs e)
		{
			// Request a new connector from the pool manager
			Npgsql.Connector Connector = Npgsql.ConnectorPool.ConnectorPoolMgr.RequestConnector(
				this.textboxConnectionString.Text, this.checkboxShared.Checked
				);
			Connector.Pooled = this.checkboxPooled.Checked;
			// and add it to the used connectors list.
			// todo...
		}

		private void label1_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}
