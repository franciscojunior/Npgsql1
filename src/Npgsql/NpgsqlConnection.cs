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
    private readonly Char CONN_DELIM = ';';  // Delimeter
    private readonly Char CONN_ASSIGN = '=';
    private readonly String CONN_SERVER = "SERVER";
    private readonly String CONN_USERID = "USER ID";
    private readonly String CONN_PASSWORD = "PASSWORD";
    private readonly String CONN_DATABASE = "DATABASE";
    private readonly String CONN_PORT = "PORT";

    // Postgres default port
    private readonly String PG_PORT = "5432";
		
    // These are for ODBC connection string compatibility
    private readonly String ODBC_USERID = "UID";
    private readonly String ODBC_PASSWORD = "PWD";
      		
    // Values for possible CancelRequest messages.
    private Int32			cancel_proc_id;
    private Int32			cancel_secret_key;
  	
    // Logging related values
    private readonly String CLASSNAME = "NpgsqlConnection";
  		
    private TcpClient 		connection;
    private BufferedStream	output_stream;
    private Byte[]		input_buffer;
    private Encoding		connection_encoding;
  		
    // Protocol specific fields
		
    private readonly Int32 PROTOCOL_VERSION_MAJOR = 2;
    private readonly Int32 PROTOCOL_VERSION_MINOR = 0;
  		
    private readonly Int32 AUTH_OK = 0;
    private readonly Int32 AUTH_CLEARTEXT_PASSWORD = 3;
  		
  		
    public NpgsqlConnection() : this(""){}

    public NpgsqlConnection(String ConnectionString)
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NpgsqlConnection()", LogLevel.Debug);
      
      connection_state = ConnectionState.Closed;
      connection_string = ConnectionString;
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
        NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".ConnectionString = " + value, LogLevel.Normal);
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
    		
    public IDbTransaction BeginTransaction()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".BeginTransaction()", LogLevel.Debug);
      throw new NotImplementedException();
    }
	
    public IDbTransaction BeginTransaction(IsolationLevel level)
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".BeginTransaction(" + level + ")", LogLevel.Debug);
      throw new NotImplementedException();
    }

    public void ChangeDatabase(String dbName)
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ChangeDatabase(" + dbName + ")", LogLevel.Debug);
      throw new NotImplementedException();
    }
	
    public void Open()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Open()", LogLevel.Debug);
	    	
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
		    NpgsqlEventLog.LogMsg("Connected to: " + ep_server.Address + ":" + ep_server.Port, LogLevel.Normal);
		    
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
	    		
        throw new NpgsqlException("Error in Open()", e);
      }
	    	
    }
	
    public void Close()
    
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Close()", LogLevel.Debug);
    
      try
      {
        if (connection_state == ConnectionState.Open)
        {
          // Terminate the connection sending Terminate message.
          output_stream.WriteByte((Byte)'X');
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
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateCommand()", LogLevel.Debug);
      throw new NotImplementedException();
    }
    // Implement the IDisposable interface.
    public void Dispose()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Dispose()", LogLevel.Debug);
	    		    	
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
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ParseConnectionString()", LogLevel.Debug);
	    	
      // Get the key-value pairs delimited by CONN_DELIM
      String[] pairs = connection_string.Split(new Char[] {CONN_DELIM});
	    	
      String[] keyvalue;
      // Now, for each pair, get its key-value.
      foreach(String s in pairs)
      {
        // This happen when there are trailling/empty CONN_DELIMs
        // Just ignore them.
        if (s == "")	
          continue;
	    		
        keyvalue = s.Split(new Char[] {CONN_ASSIGN});
	    		
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
	    	
	NpgsqlEventLog.LogMsg("Connection string option: " + keyvalue[0] + " = " + keyvalue[1], LogLevel.Normal);
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
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".WritestartupPacket()", LogLevel.Debug);
	    	
      NpgsqlStartupPacket startup_packet = new NpgsqlStartupPacket(296,
			                                                             PROTOCOL_VERSION_MAJOR,
			                                                             PROTOCOL_VERSION_MINOR,
			                                                             (String)connection_string_values[CONN_DATABASE],
			                                                             (String)connection_string_values[CONN_USERID],
			                                                             "",
			                                                             "",
			                                                             "");
    	
    	startup_packet.WriteToStream(output_stream, connection_encoding);
    	
      output_stream.Flush();
	    	
    }
	    
    private void HandleStartupPacketResponse()
    {
      NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".HandleStartupPacketResponse()", LogLevel.Debug);
	    	
      // The startup packet was sent.
      // Handle possible error messages or password requests.
	    	
      NetworkStream 	ns = connection.GetStream(); // Stream to read from network.
      Int32 			num_Bytes_read;
      Int32			auth_type;
      Boolean			ready_query = false;
	    	
      while (!ready_query)
      {
        // Check the first Byte of response.
        switch (ns.ReadByte())
        {
          case 'E':
            NpgsqlEventLog.LogMsg("ErrorResponse message from Server", LogLevel.Debug);
            // An error occured.
            // Copy the message and throw an exception.
            // Close the connection.
            Close();
            String error_message = PGUtil.ReadString(ns, connection_encoding);
            throw new NpgsqlException(error_message);
		    		
          case 'R':
            NpgsqlEventLog.LogMsg("AuthenticationRequest message from Server", LogLevel.Debug);
            // Received an Authentication Request.
		    			
            // Read the request.
            num_Bytes_read = ns.Read(input_buffer, 0, 4);
            auth_type = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
		    			
            if (auth_type == AUTH_OK)
            {
              // Authentication is ok.
              // Wait for ReadForQuery message
              NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
              continue;
            }
		    			
            if (auth_type == AUTH_CLEARTEXT_PASSWORD)
            {
              NpgsqlEventLog.LogMsg("Server requested cleartext password authentication.", LogLevel.Debug);
              
              // Send the PasswordPacket.
              
              /*String password = ((String) connection_string_values[CONN_PASSWORD]);
              // Add the null string terminator
              password = password.PadRight(password.Length + 1, '\x00');
			    			
              output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(password.Length + 4)), 0, 4);
              output_stream.Write(connection_encoding.GetBytes(password), 0, password.Length);
            	*/
            	
            	NpgsqlPasswordPacket password_packet = new NpgsqlPasswordPacket((String) connection_string_values[CONN_PASSWORD]);
            	password_packet.WriteToStream(output_stream, connection_encoding);
              output_stream.Flush();
		    				
              // Console.WriteLine("Going listen");
              // Wait for ReadForQuery message
              NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
              continue;  			
            }
		    			
            // Only AuthenticationClearTextPassword supported for now.
            // Close the connection.
            Close();
            throw new NpgsqlException("Only AuthenticationClearTextPassword supported for now.");
		    		
          case 'Z':
            NpgsqlEventLog.LogMsg("ReadyForQuery message from Server", LogLevel.Debug);
            // Ready for query response.
            // Exit loops.
            ready_query = true;
            NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
            continue;
		    		
          case 'K':
            NpgsqlEventLog.LogMsg("BackendKeyData message from Server", LogLevel.Debug);
            // BackendKeyData message.
            // Read the BackendKeyData message contents. Two Int32 integers = 8 Bytes.
            num_Bytes_read = ns.Read(input_buffer, 0, 8);
            cancel_proc_id = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
            cancel_secret_key = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 4));
		    			
            NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
            // Wait for ReadForQuery message
            continue;
          case 'N':
            NpgsqlEventLog.LogMsg("NoticeResponse message from Server", LogLevel.Debug);
            // NoticeResponse message.
            // [TODO] Check what to do with the NoticeResponse message. 
            // For now, just ignore (ugly!!).
		    					    			
            String notice_response = PGUtil.ReadString(ns, connection_encoding);
		    			
            NpgsqlEventLog.LogMsg("Listening for next message", LogLevel.Debug);
            // Wait for ReadForQuery message
            continue;
        }
      }
	    	    	
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
