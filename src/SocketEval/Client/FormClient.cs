using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
	/// <summary>
	/// Zusammendfassende Beschreibung für Form1.
	/// </summary>
	
	public class FormClient : System.Windows.Forms.Form
	{		
		private System.Windows.Forms.Button buttonConnect;
		
		/// <summary>
		/// The client object, connects to the server von port 11000 by default.
		/// </summary>
		
		ItcSocketTest.Client Client = new ItcSocketTest.Client();		
		
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		
		private System.ComponentModel.Container components = null;

		public FormClient()
		{
			InitializeComponent();
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
			this.buttonConnect = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonConnect
			// 
			this.buttonConnect.Location = new System.Drawing.Point(8, 8);
			this.buttonConnect.Name = "buttonConnect";
			this.buttonConnect.TabIndex = 0;
			this.buttonConnect.Text = "Connect";
			this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
			// 
			// FormClient
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.buttonConnect});
			this.Name = "FormClient";
			this.Text = "Client";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>

		[STAThread]	static void Main() 
		{
			Application.Run(new FormClient());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}

		private void buttonConnect_Click(object sender, System.EventArgs e)
		{
			this.Client.Connect();
		}
	}
}
