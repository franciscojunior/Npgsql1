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
		System.Collections.ArrayList ConnectorList;
		
		// ---------------------------------------------------
		
		private System.Windows.Forms.Button buttonRequest;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkboxShared;
		private System.Windows.Forms.TextBox textboxConnectionString;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonReleaseConnector;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListBox listboxConnectors;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ListBox listboxSharedConnectors;
		private System.Windows.Forms.ListBox listboxPooledConnectors;
		private System.Windows.Forms.Button buttonRefreshSharedConnectors;
		private System.Windows.Forms.Button buttonRefreshPooledConnectors;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkboxListPooled;
		private System.Windows.Forms.CheckBox checkboxListShared;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button buttonRefreshConnections;
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			InitializeComponent();
			this.ConnectorList = new System.Collections.ArrayList();
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
			this.listboxConnectors = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonReleaseConnector = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.listboxSharedConnectors = new System.Windows.Forms.ListBox();
			this.listboxPooledConnectors = new System.Windows.Forms.ListBox();
			this.buttonRefreshSharedConnectors = new System.Windows.Forms.Button();
			this.buttonRefreshPooledConnectors = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkboxListPooled = new System.Windows.Forms.CheckBox();
			this.checkboxListShared = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.buttonRefreshConnections = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonRequest
			// 
			this.buttonRequest.Location = new System.Drawing.Point(512, 40);
			this.buttonRequest.Name = "buttonRequest";
			this.buttonRequest.Size = new System.Drawing.Size(104, 23);
			this.buttonRequest.TabIndex = 0;
			this.buttonRequest.Text = "Request";
			this.buttonRequest.Click += new System.EventHandler(this.buttonRequest_Click);
			// 
			// textboxConnectionString
			// 
			this.textboxConnectionString.Location = new System.Drawing.Point(16, 40);
			this.textboxConnectionString.Name = "textboxConnectionString";
			this.textboxConnectionString.Size = new System.Drawing.Size(480, 20);
			this.textboxConnectionString.TabIndex = 1;
			this.textboxConnectionString.Text = "DSN=MyServer; UID=MyAccount; PWD=MyPassword;";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Connection String:";
			// 
			// checkboxShared
			// 
			this.checkboxShared.Location = new System.Drawing.Point(440, 8);
			this.checkboxShared.Name = "checkboxShared";
			this.checkboxShared.Size = new System.Drawing.Size(64, 24);
			this.checkboxShared.TabIndex = 3;
			this.checkboxShared.Text = "Shared";
			// 
			// listboxConnectors
			// 
			this.listboxConnectors.Location = new System.Drawing.Point(16, 104);
			this.listboxConnectors.Name = "listboxConnectors";
			this.listboxConnectors.Size = new System.Drawing.Size(480, 121);
			this.listboxConnectors.TabIndex = 5;
			this.listboxConnectors.SelectedIndexChanged += new System.EventHandler(this.listboxConnectors_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(18, 84);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(182, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Connections working on Connectors:";
			// 
			// buttonReleaseConnector
			// 
			this.buttonReleaseConnector.Location = new System.Drawing.Point(512, 176);
			this.buttonReleaseConnector.Name = "buttonReleaseConnector";
			this.buttonReleaseConnector.Size = new System.Drawing.Size(104, 23);
			this.buttonReleaseConnector.TabIndex = 7;
			this.buttonReleaseConnector.Text = "Release";
			this.buttonReleaseConnector.Click += new System.EventHandler(this.buttonReleaseConnector_Click);
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(8, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(608, 4);
			this.panel1.TabIndex = 8;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(8, 240);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(616, 4);
			this.panel2.TabIndex = 13;
			// 
			// listboxSharedConnectors
			// 
			this.listboxSharedConnectors.Location = new System.Drawing.Point(16, 288);
			this.listboxSharedConnectors.Name = "listboxSharedConnectors";
			this.listboxSharedConnectors.Size = new System.Drawing.Size(296, 186);
			this.listboxSharedConnectors.TabIndex = 14;
			// 
			// listboxPooledConnectors
			// 
			this.listboxPooledConnectors.Location = new System.Drawing.Point(320, 288);
			this.listboxPooledConnectors.Name = "listboxPooledConnectors";
			this.listboxPooledConnectors.Size = new System.Drawing.Size(296, 186);
			this.listboxPooledConnectors.TabIndex = 15;
			// 
			// buttonRefreshSharedConnectors
			// 
			this.buttonRefreshSharedConnectors.Location = new System.Drawing.Point(208, 256);
			this.buttonRefreshSharedConnectors.Name = "buttonRefreshSharedConnectors";
			this.buttonRefreshSharedConnectors.Size = new System.Drawing.Size(104, 23);
			this.buttonRefreshSharedConnectors.TabIndex = 16;
			this.buttonRefreshSharedConnectors.Text = "Refresh";
			this.buttonRefreshSharedConnectors.Click += new System.EventHandler(this.buttonRefreshSharedConnectors_Click);
			// 
			// buttonRefreshPooledConnectors
			// 
			this.buttonRefreshPooledConnectors.Location = new System.Drawing.Point(512, 256);
			this.buttonRefreshPooledConnectors.Name = "buttonRefreshPooledConnectors";
			this.buttonRefreshPooledConnectors.Size = new System.Drawing.Size(104, 23);
			this.buttonRefreshPooledConnectors.TabIndex = 17;
			this.buttonRefreshPooledConnectors.Text = "Refresh";
			this.buttonRefreshPooledConnectors.Click += new System.EventHandler(this.buttonRefreshPooledConnectors_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.checkboxListPooled,
																					this.checkboxListShared});
			this.groupBox1.Location = new System.Drawing.Point(512, 80);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(104, 80);
			this.groupBox1.TabIndex = 18;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Modify";
			// 
			// checkboxListPooled
			// 
			this.checkboxListPooled.Checked = true;
			this.checkboxListPooled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkboxListPooled.Location = new System.Drawing.Point(24, 48);
			this.checkboxListPooled.Name = "checkboxListPooled";
			this.checkboxListPooled.Size = new System.Drawing.Size(72, 24);
			this.checkboxListPooled.TabIndex = 12;
			this.checkboxListPooled.Text = "Pooled";
			this.checkboxListPooled.Click += new System.EventHandler(this.checkboxListPooled_Click);
			// 
			// checkboxListShared
			// 
			this.checkboxListShared.Location = new System.Drawing.Point(24, 24);
			this.checkboxListShared.Name = "checkboxListShared";
			this.checkboxListShared.Size = new System.Drawing.Size(72, 24);
			this.checkboxListShared.TabIndex = 11;
			this.checkboxListShared.Text = "Shared";
			this.checkboxListShared.Click += new System.EventHandler(this.checkboxListShared_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(24, 264);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(119, 13);
			this.label3.TabIndex = 19;
			this.label3.Text = "Shared Connectors List:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(344, 264);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(118, 13);
			this.label4.TabIndex = 20;
			this.label4.Text = "Pooled Connectors List:";
			// 
			// buttonRefreshConnections
			// 
			this.buttonRefreshConnections.Location = new System.Drawing.Point(512, 208);
			this.buttonRefreshConnections.Name = "buttonRefreshConnections";
			this.buttonRefreshConnections.Size = new System.Drawing.Size(104, 23);
			this.buttonRefreshConnections.TabIndex = 21;
			this.buttonRefreshConnections.Text = "Refresh";
			this.buttonRefreshConnections.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 485);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.buttonRefreshConnections,
																		  this.label4,
																		  this.label3,
																		  this.groupBox1,
																		  this.buttonRefreshPooledConnectors,
																		  this.buttonRefreshSharedConnectors,
																		  this.listboxPooledConnectors,
																		  this.listboxSharedConnectors,
																		  this.panel2,
																		  this.panel1,
																		  this.buttonReleaseConnector,
																		  this.label2,
																		  this.listboxConnectors,
																		  this.checkboxShared,
																		  this.label1,
																		  this.textboxConnectionString,
																		  this.buttonRequest});
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}

		private void buttonRequest_Click(object sender, System.EventArgs e)
		{
			Npgsql.Connector Connector = Npgsql.ConnectorPool.ConnectorPoolMgr.RequestConnector(
				this.textboxConnectionString.Text, this.checkboxShared.Checked
				);
			this.ConnectorList.Add( Connector );
			this.listboxConnectors.Items.Add( this.ConnectorInfoString( Connector )); 
			this.buttonRefreshPooledConnectors_Click( null, null );
			this.buttonRefreshSharedConnectors_Click( null, null );
		}

		private void listboxConnectors_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int Index = this.listboxConnectors.SelectedIndex;
			if ( Index == -1 ) return;
			Npgsql.Connector Connector = (Npgsql.Connector)this.ConnectorList[ Index ];
			this.checkboxListPooled.Checked = Connector.Pooled;
			this.checkboxListShared.Checked = Connector.Shared;
		}

		private void checkboxListShared_Click(object sender, System.EventArgs e)
		{
			int Index = this.listboxConnectors.SelectedIndex;
			if ( Index == -1 ) return;
			Npgsql.Connector Connector = (Npgsql.Connector)this.ConnectorList[ Index ];
			Connector.Shared = this.checkboxListShared.Checked;
			// Read back the value, changes might have been rejected...
			this.checkboxListShared.Checked = Connector.Shared;
			// Reflect the changes in the Used Connectors Listbox
			this.listboxConnectors.Text = this.ConnectorInfoString( Connector );
		}

		private void checkboxListPooled_Click(object sender, System.EventArgs e)
		{
			int Index = this.listboxConnectors.SelectedIndex;
			if ( Index == -1 ) return;
			Npgsql.Connector Connector = (Npgsql.Connector)this.ConnectorList[ Index ];
			Connector.Pooled = this.checkboxListPooled.Checked;
			// Reflect the changes in the Used Connectors Listbox
			this.listboxConnectors.Items.RemoveAt( Index );
			this.listboxConnectors.Items.Insert( Index, this.ConnectorInfoString( Connector ));
			this.listboxConnectors.SelectedIndex = Index;
		}

		private void buttonReleaseConnector_Click(object sender, System.EventArgs e)
		{
			int Index = this.listboxConnectors.SelectedIndex;
			if ( Index == -1 ) return;
			Npgsql.Connector Connector = (Npgsql.Connector)this.ConnectorList[ Index ];
			try 
			{
				Connector.Release();
				this.listboxConnectors.Items.RemoveAt( Index );
				this.ConnectorList.RemoveAt( Index );
			}
			finally { }
			this.buttonRefreshPooledConnectors_Click( null, null );
			this.buttonRefreshSharedConnectors_Click( null, null );			
		}

		private void buttonRefreshSharedConnectors_Click(object sender, System.EventArgs e)
		{
			this.listboxSharedConnectors.Items.Clear();
			foreach( Npgsql.Connector Connector in Npgsql.ConnectorPool.ConnectorPoolMgr.DebugSharedConnectors )
			{
				this.listboxSharedConnectors.Items.Add( 
					this.ConnectorInfoString( Connector )
				);
			}
		}

		private void buttonRefreshPooledConnectors_Click(object sender, System.EventArgs e)
		{
			this.listboxPooledConnectors.Items.Clear();
			foreach( Npgsql.Connector Connector in Npgsql.ConnectorPool.ConnectorPoolMgr.DebugPooledConnectors )
			{
				this.listboxPooledConnectors.Items.Add( 
					this.ConnectorInfoString( Connector )
				);
			}
		}
		
		private string ConnectorInfoString ( Npgsql.Connector Connector )
		{
			// Formats the connector properties into a string representíon
			string Shared = "s";
			if ( Connector.Shared ) Shared = "S";
			string Pooled = "p";
			if ( Connector.Pooled ) Pooled = "P";			
			return "[ " + Connector.InstanceNumber.ToString() + " ]"
				+ "[ " + Pooled + " ]" 
				+ "[ " + Shared + " ]" 
				+ "[ " + Connector.ShareCount.ToString() + " ] " 
				+ Connector.ConnectString
				; 
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.listboxConnectors.Items.Clear();
			foreach( Npgsql.Connector Connector in this.ConnectorList )
			{
				this.listboxConnectors.Items.Add( this.ConnectorInfoString( Connector ));
			}
		}
	}
}
