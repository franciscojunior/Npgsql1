// created on 10/5/2002 at 23:01

// Npgsql.NpgsqlConnection.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 Francisco Jr.
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




using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Specialized;


namespace Npgsql
{
  /// <summary>
  /// This class represents a connection to 
  /// PostgreSQL Server.
  /// </summary>
  /// 
  public sealed class NpgsqlConnection : IDbConnection
  {
    private ConnectionState connection_state;
    private String      	connection_string;
    private ListDictionary	connection_string_values;

    // In the connection string
    private readonly char CONN_DELIM = ';';  // Delimeter
    private readonly char CONN_ASSIGN = '=';
    private readonly String CONN_SERVER = "SERVER";
    private readonly String CONN_USERID = "USER ID";
    private readonly String CONN_PASSWORD = "PASSWORD";
    private readonly String CONN_DATABASE = "DATABASE";
    private readonly String CONN_PORT = "PORT";

    // Postgres default port
    private readonly Int32 PG_PORT = 5432;
		
    // These are for ODBC connection string compatibility
    private readonly String ODBC_USERID = "UID";
    private readonly String ODBC_PASSWORD = "PWD";

    // Values for LogMsg levels
    private static readonly Int32 LOG_NONE = 1;
    private static readonly Int32 LOG_ERR = 2;
    private static readonly Int32 LOG_SQL = 3;
    private static readonly Int32 LOG_INFO = 4;
    private static readonly Int32 LOG_FUNC = 5;

    // Logging related values
    private static readonly String LOG_FILENAME = "Npgsql.log";
    private String    logfile;
    private Int32     loglevel;
    private readonly String CLASSNAME = "NpgsqlConnection";
      		
    // Values for possible CancelRequest messages.
    private Int32			cancel_proc_id;
    private Int32			cancel_secret_key;  	
  		
    private TcpClient 		connection;
    private BufferedStream	output_stream;
    private Byte[]			input_buffer;
    private Encoding		connection_encoding;
  		
    // Protocol specific fields
		
    private readonly Int32 PROTOCOL_VERSION_MAJOR = 2;
    private readonly Int32 PROTOCOL_VERSION_MINOR = 0;
  		
    private readonly Int32 AUTH_OK = 0;
    private readonly Int32 AUTH_CLEARTEXT_PASSWORD = 3;
  		
  		
    public NpgsqlConnection() : this("", LOG_NONE, LOG_FILENAME){}
    public NpgsqlConnection(String ConnectionString) : this(ConnectionString, LOG_NONE, LOG_FILENAME){}
    public NpgsqlConnection(String ConnectionString, Int32 LogLevel) : this(ConnectionString, LogLevel, LOG_FILENAME){}
    
    public NpgsqlConnection(String ConnectionString, Int32 LogLevel, String LogFile)
    {
      connection_state = ConnectionState.Closed;
      connection_string = ConnectionString;
      loglevel = LogLevel;
      logfile = LogFile;
      connection_string_values = new ListDictionary();
      connection_encoding = Encoding.Default;
      ParseConnectionString();
    }

    public string ConnectionString
    {
      get
      {
        return connection_string;
      }
      set
      {
        connection_string = value;
        LogMsg("Set " + CLASSNAME + ".ConnectionString = " + value, 1);
        ParseConnectionString();
      }
    }
	
    public Int32 ConnectionTimeout
    {
      get
      {
        return 0;
      }
    }
    ///<summary>
    /// 
    /// </summary>	
    public String Database
    {
      get
      {
        return "";
      }
    }
	
    public ConnectionState State
    {
      get 
      {	    		
        return connection_state; 
      }
    }
    
    ///<summary>
    /// Sets/Returns the level of information to log to the logfile.
    /// 1 - None
    /// 2 - (1) + Errors
    /// 3 - (2) + SQL queries
    /// 4 - (3) + Debug information
    /// 5 - (4) + Function parameters
    /// </summary>	
    public Int32 LogLevel
    {
      get
      {
        return loglevel;
      }
      set
      {
        loglevel = value;
        LogMsg("Set " + CLASSNAME + ".LogLevel = " + value, LOG_INFO);
      }
    }
    
    ///<summary>
    /// Sets/Returns the filename to use for logging.
    /// </summary>	
    public String LogFile
    {
      get
      {
        return logfile;
      }
      set
      {
        logfile = value;
        LogMsg("Set " + CLASSNAME + ".LogFile = " + value, LOG_INFO);
      }
    }
		
    public IDbTransaction BeginTransaction()
    {
      LogMsg("Entering " + CLASSNAME + ".BeginTransaction()", LOG_FUNC);
      throw new NotImplementedException();
    }
	
    public IDbTransaction BeginTransaction(IsolationLevel level)
    {
      LogMsg("Entering " + CLASSNAME + ".BeginTransaction(" + level + ")", LOG_FUNC);
      throw new NotImplementedException();
    }

    public void ChangeDatabase(String dbName)
    {
      LogMsg("Entering " + CLASSNAME + ".ChangeDatabase(" + dbName + ")", LOG_FUNC);
      throw new NotImplementedException();
    }
	
    public void Open()
    {
      LogMsg("Entering " + CLASSNAME + ".Open()", LOG_FUNC);
	    	
      try
      {
		    		
		    	
        // Check if the connection is already open.
        if (connection_state == ConnectionState.Open)
          throw new NpgsqlException("Connection already open");
		    	
        // Change the state of connection to open.
        connection_state = ConnectionState.Open;
		    	
        IPEndPoint ep_server;
	    		
        // Open the connection to the backend.
        connection = new TcpClient();
		    			    	
        // If it was specified an IP address in doted notation 
        // (i.e.:192.168.0.1), there may be a long delay trying
        // resolve it when it is not necessary.
        // So, try first connect as if it was a dotted ip address.
		    	
        try
        {
          IPAddress ipserver = IPAddress.Parse((String)connection_string_values[CONN_SERVER]);
          ep_server = new IPEndPoint(ipserver, Int32.Parse((String)connection_string_values[CONN_PORT]));
        }
        catch(FormatException)	// The exception isn't used.
        {
          // Server isn't in dotted decimal format. Just connect using DNS resolves.
          IPHostEntry he_server = Dns.GetHostByName((String)connection_string_values[CONN_SERVER]);
          ep_server = new IPEndPoint(he_server.AddressList[0], Int32.Parse((String)connection_string_values[CONN_PORT]));	
        }
		    	
        // Connect to the server.
        connection.Connect(ep_server);	
		    LogMsg("Connected to: " + ep_server.Address + ":" + ep_server.Port, LOG_INFO);
		    
        output_stream = new BufferedStream(connection.GetStream());
        input_buffer = new Byte[8192];
		    	
		    	
        // Write the startup packet to server.
        WriteStartupPacket();
		    	
        // Now, process the response. 
        HandleStartupPacketResponse();
	    		
        // Connection completed.
	    			    		
      }
      catch(SocketException e)
      {
        // [TODO] Very ugly message. Needs more working.
        throw new NpgsqlException("A SocketException occured", e);
      }
	    	
      catch(IOException e)
      {
        // This exception was thrown by StartupPacket handling functions.
        // So, close the connection and throw the exception.
        // [TODO] Better exception handling. :)
        Close();
	    		
        //Console.WriteLine(e.StackTrace);
        throw new NpgsqlException("Error in Open()", e);
      }
	    	
    }
	
    public void Close()
    
    {
      LogMsg("Entering " + CLASSNAME + ".Close()", LOG_FUNC);
    
      try
      {
        if (connection_state == ConnectionState.Open)
        {
          // Terminate the connection sending Terminate message.
          output_stream.WriteByte((byte)'X');
          output_stream.Flush();
		    		
        }
      }
      catch (IOException e)
      {
        throw new NpgsqlException("Error in Close()", e);
      }
      finally
      {
        // Even if an exception occurs, let object in a consistent state.
        connection.Close();
        connection_state = ConnectionState.Closed;
      }
    }
	    
    public IDbCommand CreateCommand()
    {
      LogMsg("Entering " + CLASSNAME + ".CreateCommand()", LOG_FUNC);
      throw new NotImplementedException();
    }
    // Implement the IDisposable interface.
    public void Dispose()
    {
      LogMsg("Entering " + CLASSNAME + ".Dispose()", LOG_FUNC);
	    		    	
    }
	    
    // Private util methods
	    
    /// <summary>
    /// This method parses the connection string.
    /// It translates it to a list of key-value pairs.
    /// Valid values are:
    /// Server 		- Address/Name of Postgresql Server
    /// Port		- Port to connect to.
    /// Database 	- Database name. Defaults to user name if not specified
    /// User		- User name
    /// Password	- Password for clear text authentication
    /// </summary>
    private void ParseConnectionString()
    {
      LogMsg("Entering " + CLASSNAME + ".ParseConnectionString()", LOG_FUNC);
	    	
      // Get the key-value pairs delimited by CONN_DELIM
      String[] pairs = connection_string.Split(new char[] {CONN_DELIM});
	    	
      String[] keyvalue;
      // Now, for each pair, get its key-value.
      foreach(String s in pairs)
      {
        // This happen when there are trailling/empty CONN_DELIMs
        // Just ignore them.
        if (s == "")	
          continue;
	    		
        keyvalue = s.Split(new char[] {CONN_ASSIGN});
	    		
        // Check if there is a key-value pair.
        if (keyvalue.Length != 2)
          throw new ArgumentException("key=value argument incorrect in ConnectionString", connection_string);
	    	
	    	// Shift the key to upper case, and substitute ODBC style keys
	    	keyvalue[0] = keyvalue[0].ToUpper();
	    	if (keyvalue[0] == ODBC_USERID)
	    	  keyvalue[0] = CONN_USERID;
        if (keyvalue[0] == ODBC_PASSWORD)
          keyvalue[0] = CONN_PASSWORD;	    	 
	    	
	    	// Add the pair to the dictionary. The key is shifted to upper
	    	// case for case insensitivity.
	    	
	    	LogMsg("Connection string option: " + keyvalue[0] + " = " + keyvalue[1], LOG_INFO);
        connection_string_values.Add(keyvalue[0], keyvalue[1]);
      }
	    	
      // Now check if there is any missing argument.
      if (connection_string_values[CONN_SERVER] == null)
        throw new ArgumentException("Connection string argument missing!", CONN_SERVER);
      if ((connection_string_values[CONN_USERID] == null) & (connection_string_values[ODBC_USERID] == null))
        throw new ArgumentException("Connection string argument missing!", CONN_USERID);
      if ((connection_string_values[CONN_PASSWORD] == null) & (connection_string_values[ODBC_PASSWORD] == null))
        throw new ArgumentException("Connection string argument missing!", CONN_PASSWORD);
      if (connection_string_values[CONN_DATABASE] == null)
        // Database is optional. "[...] defaults to the user name if empty"
        connection_string_values[CONN_DATABASE] = connection_string_values[CONN_USERID];
      if (connection_string_values[CONN_PORT] == null)
        // Port is optional. Defaults to PG_PORT.
        connection_string_values[CONN_PORT] = PG_PORT;
    }
	    
    private void WriteStartupPacket()
    {
      LogMsg("Entering " + CLASSNAME + ".WritestartupPacket()", 2);
	    	
      // Packet length = 296
      output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int32)296)), 0, 4);
	    	
      // Protocol version = 2.0
      output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((Int32)((PROTOCOL_VERSION_MAJOR<<16) | PROTOCOL_VERSION_MINOR))), 0, 4);
	    	
      // Database name.
      String dbname = (String)connection_string_values[CONN_DATABASE];
      // Pad with nulls to get the 64 LimString
      dbname = dbname.PadRight(64, '\x00');
      output_stream.Write(connection_encoding.GetBytes(dbname), 0, 64);
	    	
      // User name.
      String username = (String)connection_string_values[CONN_USERID];
      // Pad with nulls to get the 32 LimString
      username = username.PadRight(32, '\x00');
      output_stream.Write(connection_encoding.GetBytes(username), 0, 32);
	    	
      // Write the other unused fields
      String unused = "";
      unused = unused.PadRight(192, '\x00');
      output_stream.Write(connection_encoding.GetBytes(unused), 0, 64 + 64 + 64);
      output_stream.Flush();
	    	
    }
	    
    private void HandleStartupPacketResponse()
    {
      LogMsg("Entering " + CLASSNAME + ".HandleStartupPacketResponse()", 2);
	    	
      // The startup packet was sent.
      // Handle possible error messages or password requests.
	    	
      NetworkStream 	ns = connection.GetStream(); // Stream to read from network.
      Int32 			num_bytes_read;
      Int32			auth_type;
      Boolean			ready_query = false;
	    	
      while (!ready_query)
      {
        // Check the first byte of response.
        switch (ns.ReadByte())
        {
          case 'E':
            // Console.WriteLine("ErrorResponse");
            // An error occured.
            // Copy the message and throw an exception.
            // Close the connection.
            Close();
            String error_message = PGUtil.ReadString(ns, connection_encoding);
            throw new NpgsqlException(error_message);
		    		
          case 'R':
            // Console.WriteLine("AuthenticationRequest");
            // Received an Authentication Request.
		    			
            // Read the request.
            num_bytes_read = ns.Read(input_buffer, 0, 4);
            auth_type = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
		    			
            if (auth_type == AUTH_OK)
            {
              // Authentication is ok.
              // Wait for ReadForQuery message
              continue;
            }
		    			
            if (auth_type == AUTH_CLEARTEXT_PASSWORD)
            {
              LogMsg("Server requested cleartext password authentication.", LOG_INFO);
              
              // Send the PasswordPacket.
              String password = ((String) connection_string_values[CONN_PASSWORD]);
              // Add the null string terminator
              password = password.PadRight(password.Length + 1, '\x00');
			    			
              output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(password.Length + 4)), 0, 4);
              output_stream.Write(connection_encoding.GetBytes(password), 0, password.Length);
              output_stream.Flush();
		    				
              // Console.WriteLine("Going listen");
              // Wait for ReadForQuery message
              continue;  			
            }
		    			
            // Only AuthenticationClearTextPassword supported for now.
            // Close the connection.
            Close();
            throw new NpgsqlException("Only AuthenticationClearTextPassword supported for now.");
		    		
          case 'Z':
            // Console.WriteLine("ReadyForQuery");
            // Ready for query response.
            // Exit loops.
            ready_query = true;
            continue;
		    		
          case 'K':
            // BackendKeyData message.
            // Console.WriteLine("BackendKeyData");
            // Read the BackendKeyData message contents. Two Int32 integers = 8 bytes.
            num_bytes_read = ns.Read(input_buffer, 0, 8);
            cancel_proc_id = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
            cancel_secret_key = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 4));
		    			
            // Console.WriteLine("Going listen");
            // Wait for ReadForQuery message
            continue;
          case 'N':
            // NoticeResponse message.
            // Console.WriteLine("NoticeResponse");
            // [TODO] Check what to do with the NoticeResponse message. 
            // For now, just ignore (ugly!!).
		    					    			
            String notice_response = PGUtil.ReadString(ns, connection_encoding);
		    			
            // Console.WriteLine("Going listen");
            // Wait for ReadForQuery message
            continue;
        }
      }
	    	    	
    }
    
    // Event/Debug Logging
    internal void LogMsg(String message, Int32 level) 
    {
      if (level > loglevel)
        return;
        
      StreamWriter writer = new StreamWriter(logfile, true);
      writer.WriteLine("[" + level + "] " + System.DateTime.Now + " - " + message);
      writer.Close(); 
    }
	    
    // Internal properties
	    
    internal TcpClient tcp_connection
    {
      get
      {
        return connection;
      }
	    	
    }
	    
    internal Encoding encoding
    {
      get
      {
        return connection_encoding;
      }
    }
  }
}
