#region Licence
// created on 05/06/2002 at 20:17

// frmMain.cs
// 
// Author:
//	Dave Page (dpage@postgresql.org)
//
//	Copyright (C) 2002 Dave Page
//

// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#endregion

using System;
using System.Windows.Forms;
using System.Security;
using System.Security.Principal;
using Npgsql;


  /// <summary>
  /// The main form for the application
  /// </summary>
  public class frmMain : System.Windows.Forms.Form
  {
    private System.Windows.Forms.TabPage Connection;
    private System.Windows.Forms.TabPage ExecuteNonQuery;
    private System.Windows.Forms.TabPage ExecuteScalar;
    private System.Windows.Forms.Button cmdConnect;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.TextBox txtPort;
    private System.Windows.Forms.TextBox txtHostname;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.Label lblHostname;
    private System.Windows.Forms.TextBox txtLog;
    private System.Windows.Forms.TabControl tabset;
    private System.Windows.Forms.TextBox txtNonQuery;
    private System.Windows.Forms.Label lblNonQuery;
    private System.Windows.Forms.Button cmdNonQuery;
    private System.Windows.Forms.Button cmdScalar;
    private System.Windows.Forms.Label lblScalar;
    private System.Windows.Forms.TextBox txtScalar;
    private NpgsqlConnection cnDB;

    public frmMain()
    {
      // Initialise the form
      InitializeComponent();

      // Bind functions to the buttons
      cmdConnect.Click += new System.EventHandler(cmdConnect_Click);
      cmdNonQuery.Click += new System.EventHandler(cmdNonQuery_Click);
      cmdScalar.Click += new System.EventHandler(cmdScalar_Click);

      // Set the default username
      AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
      WindowsPrincipal objUser = (WindowsPrincipal)System.Threading.Thread.CurrentPrincipal; 
      txtUsername.Text = objUser.Identity.Name.Substring(objUser.Identity.Name.IndexOf("\\") + 1);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      base.Dispose( disposing );
    }

    private void cmdConnect_Click(object sender, System.EventArgs e) 
    {
		
		  txtLog.Text = txtLog.Text + "Connecting to PostgreSQL:\r\n";
		  
      // Check the data
      if (txtHostname.Text == "")
      {
        txtLog.Text = txtLog.Text + "Error: No hostname was specified!\r\n";
        txtLog.Text = txtLog.Text + "Finished connecting!\r\n\n"; 
        return;
      }
      if (txtPort.Text == "")
      {
        txtLog.Text = txtLog.Text + "Error: No port was specified!\r\n";
        txtLog.Text = txtLog.Text + "Finished connecting!\r\n\n"; 
        return;
      }
      if (txtUsername.Text == "")
      {
        txtLog.Text = txtLog.Text + "Error: No username was specified!\r\n";
        txtLog.Text = txtLog.Text + "Finished connecting!\r\n\n"; 
        return;
      }

      // Setup a connection string
      string szConnect = "Database=template1;Server=" + txtHostname.Text + ";Port=" + int.Parse(txtPort.Text) + ";User ID=" + txtUsername.Text + ";Password=" + txtPassword.Text + ";";
      txtLog.Text = txtLog.Text + "Connection String: " + szConnect + "\r\n";

      // Attempt to open a connection
      cnDB = new NpgsqlConnection(szConnect);
     
      try 
      {
        cnDB.Open();
      } 
      catch (NpgsqlException ex) 
      {
        txtLog.Text = txtLog.Text + "Error: " + ex.Message + "\r\n" + "StackTrace: \r\n" + ex.StackTrace;
        return;
      }
      catch(InvalidOperationException ex)
      {
        txtLog.Text = txtLog.Text + "Error: " + ex.Message + "\r\n" + "StackTrace: \r\n" + ex.StackTrace;
        return;
      } 
      finally
      {
        txtLog.Text = txtLog.Text + "Finished connecting!\r\n\n"; 
      }
      
    }
    
    private void cmdNonQuery_Click(object sender, System.EventArgs e) 
    {
    
      txtLog.Text = txtLog.Text + "Executing Non-Query:\r\n";
      txtLog.Text = txtLog.Text + "Query: " + txtNonQuery.Text + "\r\n";
      
      try
      {
        NpgsqlCommand cmdNQ = new NpgsqlCommand(txtNonQuery.Text, cnDB);
        cmdNQ.ExecuteNonQuery();
      }
      catch(NpgsqlException ex)
      {
        txtLog.Text = txtLog.Text + "Error: " + ex.Message + "\r\n" + "StackTrace: \r\n" + ex.StackTrace;
        return;
      }
      catch(InvalidOperationException ex)
      {
        txtLog.Text = txtLog.Text + "Error: " + ex.Message + "\r\n" + "StackTrace: \r\n" + ex.StackTrace;
        return;
      } 
      finally
      {
        txtLog.Text = txtLog.Text + "Finished executing Non-Query!\r\n\r\n"; 
      }
      
    }
    
    private void cmdScalar_Click(object sender, System.EventArgs e) 
    {
      txtLog.Text = txtLog.Text + "Executing Scalar:\r\n";
      txtLog.Text = txtLog.Text + "Query: " + txtScalar.Text + "\r\n";
    
      try
      {
        NpgsqlCommand cmdNQ = new NpgsqlCommand(txtNonQuery.Text, cnDB);
        Object iRes = cmdNQ.ExecuteScalar();
        txtLog.Text = txtLog.Text + "Result: " + iRes.ToString()  + "\r\n";
      }
      catch(NpgsqlException ex)
      {
        txtLog.Text = txtLog.Text + "Error: " + ex.Message + "\r\n" + "StackTrace: \r\n" + ex.StackTrace;
        return;
      } 
      catch(InvalidOperationException ex)
      {
        txtLog.Text = txtLog.Text + "Error: " + ex.Message + "\r\n" + "StackTrace: \r\n" + ex.StackTrace;
        return;
      } 
      finally
      {
        txtLog.Text = txtLog.Text + "Finished executing Scalar!\r\n\r\n"; 
      }

    }

		#region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    ///
    /// Why not?? Works for me - Dave :-)
    /// </summary>
    private void InitializeComponent()
    {
      this.tabset = new System.Windows.Forms.TabControl();
      this.Connection = new System.Windows.Forms.TabPage();
      this.cmdConnect = new System.Windows.Forms.Button();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.txtUsername = new System.Windows.Forms.TextBox();
      this.txtPort = new System.Windows.Forms.TextBox();
      this.txtHostname = new System.Windows.Forms.TextBox();
      this.lblPassword = new System.Windows.Forms.Label();
      this.lblUsername = new System.Windows.Forms.Label();
      this.lblPort = new System.Windows.Forms.Label();
      this.lblHostname = new System.Windows.Forms.Label();
      this.ExecuteNonQuery = new System.Windows.Forms.TabPage();
      this.ExecuteScalar = new System.Windows.Forms.TabPage();
      this.txtLog = new System.Windows.Forms.TextBox();
      this.txtNonQuery = new System.Windows.Forms.TextBox();
      this.lblNonQuery = new System.Windows.Forms.Label();
      this.cmdNonQuery = new System.Windows.Forms.Button();
      this.cmdScalar = new System.Windows.Forms.Button();
      this.txtScalar = new System.Windows.Forms.TextBox();
      this.lblScalar = new System.Windows.Forms.Label();
      this.tabset.SuspendLayout();
      this.Connection.SuspendLayout();
      this.ExecuteNonQuery.SuspendLayout();
      this.ExecuteScalar.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabset
      // 
      this.tabset.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                         this.Connection,
                                                                         this.ExecuteNonQuery,
                                                                         this.ExecuteScalar});
      this.tabset.Location = new System.Drawing.Point(8, 8);
      this.tabset.Name = "tabset";
      this.tabset.SelectedIndex = 0;
      this.tabset.Size = new System.Drawing.Size(600, 88);
      this.tabset.TabIndex = 10;
      // 
      // Connection
      // 
      this.Connection.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                             this.cmdConnect,
                                                                             this.txtPassword,
                                                                             this.txtUsername,
                                                                             this.txtPort,
                                                                             this.txtHostname,
                                                                             this.lblPassword,
                                                                             this.lblUsername,
                                                                             this.lblPort,
                                                                             this.lblHostname});
      this.Connection.Location = new System.Drawing.Point(4, 22);
      this.Connection.Name = "Connection";
      this.Connection.Size = new System.Drawing.Size(592, 62);
      this.Connection.TabIndex = 0;
      this.Connection.Text = "Connection";
      // 
      // cmdConnect
      // 
      this.cmdConnect.Location = new System.Drawing.Point(488, 16);
      this.cmdConnect.Name = "cmdConnect";
      this.cmdConnect.Size = new System.Drawing.Size(88, 32);
      this.cmdConnect.TabIndex = 27;
      this.cmdConnect.Text = "&Connect";
      // 
      // txtPassword
      // 
      this.txtPassword.Location = new System.Drawing.Point(320, 32);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.PasswordChar = '*';
      this.txtPassword.Size = new System.Drawing.Size(152, 20);
      this.txtPassword.TabIndex = 26;
      this.txtPassword.Text = "";
      // 
      // txtUsername
      // 
      this.txtUsername.Location = new System.Drawing.Point(320, 8);
      this.txtUsername.Name = "txtUsername";
      this.txtUsername.Size = new System.Drawing.Size(152, 20);
      this.txtUsername.TabIndex = 25;
      this.txtUsername.Text = "postgres";
      // 
      // txtPort
      // 
      this.txtPort.Location = new System.Drawing.Point(80, 32);
      this.txtPort.Name = "txtPort";
      this.txtPort.Size = new System.Drawing.Size(64, 20);
      this.txtPort.TabIndex = 24;
      this.txtPort.Text = "5432";
      // 
      // txtHostname
      // 
      this.txtHostname.Location = new System.Drawing.Point(80, 8);
      this.txtHostname.Name = "txtHostname";
      this.txtHostname.Size = new System.Drawing.Size(152, 20);
      this.txtHostname.TabIndex = 23;
      this.txtHostname.Text = "localhost";
      // 
      // lblPassword
      // 
      this.lblPassword.AutoSize = true;
      this.lblPassword.Location = new System.Drawing.Point(248, 40);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(54, 13);
      this.lblPassword.TabIndex = 22;
      this.lblPassword.Text = "Password";
      // 
      // lblUsername
      // 
      this.lblUsername.AutoSize = true;
      this.lblUsername.Location = new System.Drawing.Point(248, 16);
      this.lblUsername.Name = "lblUsername";
      this.lblUsername.Size = new System.Drawing.Size(56, 13);
      this.lblUsername.TabIndex = 21;
      this.lblUsername.Text = "Username";
      // 
      // lblPort
      // 
      this.lblPort.AutoSize = true;
      this.lblPort.Location = new System.Drawing.Point(8, 40);
      this.lblPort.Name = "lblPort";
      this.lblPort.Size = new System.Drawing.Size(25, 13);
      this.lblPort.TabIndex = 20;
      this.lblPort.Text = "Port";
      // 
      // lblHostname
      // 
      this.lblHostname.AutoSize = true;
      this.lblHostname.Location = new System.Drawing.Point(8, 16);
      this.lblHostname.Name = "lblHostname";
      this.lblHostname.Size = new System.Drawing.Size(56, 13);
      this.lblHostname.TabIndex = 19;
      this.lblHostname.Text = "Hostname";
      // 
      // ExecuteNonQuery
      // 
      this.ExecuteNonQuery.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                  this.cmdNonQuery,
                                                                                  this.txtNonQuery,
                                                                                  this.lblNonQuery});
      this.ExecuteNonQuery.Location = new System.Drawing.Point(4, 22);
      this.ExecuteNonQuery.Name = "ExecuteNonQuery";
      this.ExecuteNonQuery.Size = new System.Drawing.Size(592, 62);
      this.ExecuteNonQuery.TabIndex = 1;
      this.ExecuteNonQuery.Text = "ExecuteNonQuery";
      // 
      // ExecuteScalar
      // 
      this.ExecuteScalar.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                this.cmdScalar,
                                                                                this.txtScalar,
                                                                                this.lblScalar});
      this.ExecuteScalar.Location = new System.Drawing.Point(4, 22);
      this.ExecuteScalar.Name = "ExecuteScalar";
      this.ExecuteScalar.Size = new System.Drawing.Size(592, 62);
      this.ExecuteScalar.TabIndex = 2;
      this.ExecuteScalar.Text = "ExecuteScalar";
      // 
      // txtLog
      // 
      this.txtLog.AutoSize = false;
      this.txtLog.Location = new System.Drawing.Point(8, 104);
      this.txtLog.Multiline = true;
      this.txtLog.Name = "txtLog";
      this.txtLog.ReadOnly = true;
      this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtLog.Size = new System.Drawing.Size(600, 224);
      this.txtLog.TabIndex = 11;
      this.txtLog.Text = "";
      // 
      // txtNonQuery
      // 
      this.txtNonQuery.Location = new System.Drawing.Point(80, 8);
      this.txtNonQuery.Multiline = true;
      this.txtNonQuery.Name = "txtNonQuery";
      this.txtNonQuery.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtNonQuery.Size = new System.Drawing.Size(392, 48);
      this.txtNonQuery.TabIndex = 25;
      this.txtNonQuery.Text = "UPDATE pg_description SET description = \'I am a non-query\' WHERE 1 = 2";
      // 
      // lblNonQuery
      // 
      this.lblNonQuery.AutoSize = true;
      this.lblNonQuery.Location = new System.Drawing.Point(8, 16);
      this.lblNonQuery.Name = "lblNonQuery";
      this.lblNonQuery.Size = new System.Drawing.Size(60, 13);
      this.lblNonQuery.TabIndex = 24;
      this.lblNonQuery.Text = "Non-Query";
      // 
      // cmdNonQuery
      // 
      this.cmdNonQuery.Location = new System.Drawing.Point(488, 16);
      this.cmdNonQuery.Name = "cmdNonQuery";
      this.cmdNonQuery.Size = new System.Drawing.Size(88, 32);
      this.cmdNonQuery.TabIndex = 28;
      this.cmdNonQuery.Text = "&Execute Non-Query";
      // 
      // cmdScalar
      // 
      this.cmdScalar.Location = new System.Drawing.Point(488, 16);
      this.cmdScalar.Name = "cmdScalar";
      this.cmdScalar.Size = new System.Drawing.Size(88, 32);
      this.cmdScalar.TabIndex = 31;
      this.cmdScalar.Text = "&Execute Scalar";
      // 
      // txtScalar
      // 
      this.txtScalar.Location = new System.Drawing.Point(80, 8);
      this.txtScalar.Multiline = true;
      this.txtScalar.Name = "txtScalar";
      this.txtScalar.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtScalar.Size = new System.Drawing.Size(392, 48);
      this.txtScalar.TabIndex = 30;
      this.txtScalar.Text = "SELECT count(*) AS record_count  FROM pg_class";
      // 
      // lblScalar
      // 
      this.lblScalar.AutoSize = true;
      this.lblScalar.Location = new System.Drawing.Point(8, 16);
      this.lblScalar.Name = "lblScalar";
      this.lblScalar.Size = new System.Drawing.Size(36, 13);
      this.lblScalar.TabIndex = 29;
      this.lblScalar.Text = "Scalar";
      // 
      // frmMain
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(618, 336);
      this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                  this.txtLog,
                                                                  this.tabset});
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "frmMain";
      this.Text = "Npgsql Test Suite";
      this.tabset.ResumeLayout(false);
      this.Connection.ResumeLayout(false);
      this.ExecuteNonQuery.ResumeLayout(false);
      this.ExecuteScalar.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
      Application.Run(new frmMain());
    }
  }
