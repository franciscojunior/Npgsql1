using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using ItcCodeLib;


namespace ItcSockets
{
	/// <summary>
	/// Zusammendfassende Beschreibung für Form1.
	/// </summary>
	public class FormServer : System.Windows.Forms.Form
	{
		// Listener: Gate to the server
		ItcSocketTest.Listener Listener = new ItcSocketTest.Listener();
		
		private System.Windows.Forms.Button buttonStartListening;
		private System.Windows.Forms.Button buttonStop;
		
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		
		private System.ComponentModel.Container components = null;

		public FormServer()
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
			this.buttonStartListening = new System.Windows.Forms.Button();
			this.buttonStop = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonStartListening
			// 
			this.buttonStartListening.Location = new System.Drawing.Point(8, 8);
			this.buttonStartListening.Name = "buttonStartListening";
			this.buttonStartListening.Size = new System.Drawing.Size(160, 23);
			this.buttonStartListening.TabIndex = 0;
			this.buttonStartListening.Text = "Start Listen";
			this.buttonStartListening.Click += new System.EventHandler(this.buttonStartListening_Click);
			// 
			// buttonStop
			// 
			this.buttonStop.Enabled = false;
			this.buttonStop.Location = new System.Drawing.Point(8, 40);
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(160, 23);
			this.buttonStop.TabIndex = 1;
			this.buttonStop.Text = "Stop";
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// FormServer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.buttonStop,
																		  this.buttonStartListening});
			this.Name = "FormServer";
			this.Text = "Server Form";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		
		[STAThread]	static void Main() 
		{
			Application.Run(new FormServer());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		
		}
	
		//	buttonStartListening()
		// ------------------------------------------------------------------------------
		//	Beschreibung
		//		Startet den Listener Thread, der auf eingehende Connections wartet.
		
		private void buttonStartListening_Click(object sender, System.EventArgs e)
		{
			/*
			if ( this.ServerThread == null )
			{
				Console.WriteLine( "Creating a new thread object" );
				// Setup ThreadStart Delegate
				System.Threading.ThreadStart ListenerDelegate;
				ListenerDelegate = new System.Threading.ThreadStart( this.Listen );
				// Create Thread
				this.ServerThread = new System.Threading.Thread( ListenerDelegate );
				this.ServerThread.Name = "ListenerThread";
			}				
			Console.WriteLine( "Start: Starting listener thread" );
			this.ServerThread.Start();
			Console.WriteLine( "Start: Listener thread started" );
			*/
			this.buttonStartListening.Enabled = false;
			this.Listener.Start();
			this.buttonStop.Enabled = true;	
			this.buttonStop.Focus();
		}
		
		//	buttonStop_Click()
		// ------------------------------------------------------------------------------
		//	Beschreibung
		//		Bricht den Listener Thread ab. Sofern er noch abbrechbar ist...

		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			/*
			Console.WriteLine( "Stop: Aborting the thread" );
			this.Listener.Shutdown( System.Net.Sockets.SocketShutdown.Both );
			this.Listener.Close();
			this.ServerThread.Abort();
			Console.WriteLine( "Stop: Thread aborted" );
			*/
			this.buttonStop.Enabled = false;
			this.Listener.Stop();
			this.buttonStartListening.Enabled = true;
			this.buttonStartListening.Focus();
		}
	}		
}
