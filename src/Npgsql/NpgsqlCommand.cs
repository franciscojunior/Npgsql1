// created on 21/5/2002 at 20:03

// Npgsql.NpgsqlCommand.cs
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

namespace Npgsql
{
	public sealed class NpgsqlCommand : IDbCommand
	{
		
		private NpgsqlConnection 			connection;
		//private TcpClient					tcp_connection;
		private String						text;
		private Int32						timeout;
		private CommandType					type;
		private NpgsqlParameterCollection	parameters;
		
    // Logging related values
    private static readonly String CLASSNAME = "NpgsqlCommand";
		
		// Constructors
		
		public NpgsqlCommand() : this(null, null){}
		
		public NpgsqlCommand(String cmdText) : this(cmdText, null){}
		
		
		public NpgsqlCommand(String cmdText, NpgsqlConnection connection)
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NpgsqlCommand()", LogLevel.Debug);
			
			text = cmdText;
			this.connection = connection;
			parameters = new NpgsqlParameterCollection();
			timeout = 20;
			type = CommandType.Text;			
		}
				
		// Public properties.
		
		public String CommandText
		{
			get
			{
				return text;
			}
			
			set
			{
				// [TODO] Validate commandtext.
				text = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".CommandText = " + value, LogLevel.Normal);
			}
		}
		
		public Int32 CommandTimeout
		{
			get
			{
				return timeout;
			}
			
			set
			{
				if (value < 0)
					throw new ArgumentException("CommandTimeout can't be less than zero");
				
				timeout = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".CommandTimeout = " + value, LogLevel.Normal);
			}
		}
		
		public CommandType CommandType
		{
			get
			{
				return type;
			}
			
			set
			{
				type = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".CommandType = " + value, LogLevel.Normal);
			}
			
		}
		
		IDbConnection IDbCommand.Connection
		{
			get
			{
				return Connection;
			}
			
			set
			{
				connection = (NpgsqlConnection) value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".IDbCommand.Connection", LogLevel.Normal);
			}
		}
		
		public NpgsqlConnection Connection
		{
			get
			{
				return connection;
			}
			
			set
			{
				connection = value;
				NpgsqlEventLog.LogMsg("Set " + CLASSNAME + ".Connection", LogLevel.Normal);
			}
		}
		
		IDataParameterCollection IDbCommand.Parameters
		{
			get
			{
				return Parameters;
			}
		}
		
		public NpgsqlParameterCollection Parameters
		{
			get
			{
				return parameters;
			}
		}
		
		public IDbTransaction Transaction
		{
			get
			{
				throw new NotImplementedException();
			}
			
			set
			{
				throw new NotImplementedException();	
			}
		}
		
		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				throw new NotImplementedException();
			}
			
			set
			{
				throw new NotImplementedException();
			}
		}
		
		
		public void Cancel()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Cancel()", LogLevel.Debug);
		  
			// [TODO] Finish method implementation.
			throw new NotImplementedException();
		}
		
		
		IDbDataParameter IDbCommand.CreateParameter()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".IDbCommand.CreateParameter()", LogLevel.Debug);
		  
			return (NpgsqlParameter) CreateParameter();
		}
		
		public NpgsqlParameter CreateParameter()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CreateParameter()", LogLevel.Debug);
		  
			return new NpgsqlParameter();
		}
		
		public Int32 ExecuteNonQuery()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteNonQuery()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			NetworkStream network_stream = connection.tcp_connection.GetStream();
			BufferedStream output_stream = new BufferedStream(network_stream);
			
			// Send the query to server.
			// Write the byte 'Q' to identify a query message.
			output_stream.WriteByte((byte)'Q');
			
			// Write the query. In this case it is the CommandText text.
			// It is a string terminated by a C NULL character.
			output_stream.Write(connection.encoding.GetBytes(text + '\x00') , 0, text.Length + 1);
			
			// Send bytes.
			output_stream.Flush();
			
			// Now, enter in the loop to process the response.
			
			Boolean ready_for_query = false;
			String error_message = null;			
			
			Int32 rows_affected = -1;
			
			
			while (!ready_for_query)
			{
				switch (network_stream.ReadByte())
				{
					case 'C':
						// This is the CompletedResponse message.
						
						// Get the string returned.
						String ret_string = PGUtil.ReadString(network_stream, connection.encoding);
						String[] ret_string_tokens = ret_string.Split(null);	// whitespace separator.
						
						// Check if the command was insert, delete or update.
						// Only theses commands return rows affected.
						// [FIXME] Is there a better way to check this??
						if ((String.Compare(ret_string_tokens[0], "INSERT", true) == 0) ||
						    (String.Compare(ret_string_tokens[0], "UPDATE", true) == 0) ||
						    (String.Compare(ret_string_tokens[0], "DELETE", true) == 0))
						    
							// The number of rows affected is in the third token for insert queries
							// and in the second token for update and delete queries.
							// In other words, it is the last token in the 0-based array.
													
							rows_affected = Int32.Parse(ret_string_tokens[ret_string_tokens.Length - 1]);
					
					
						// Now wait for ReadyForQuery message.
						break;
					
					case 'P':
						// This is the cursor response message. 
						// It is followed by a C NULL terminated string with the name of 
						// the cursor in a FETCH case or 'blank' otherwise.
						// In this case it should be always 'blank'.
						// [FIXME] Get another name for this function.
						
						//String cursor_name = GetStringFromNetStream(network_stream);
						String cursor_name = PGUtil.ReadString(network_stream, connection.encoding);
						// Continue wainting for ReadyForQuery message.
						break;

					case 'I':
						// This is the EmptyQueryResponse.
						
						// [FIXME] Just ignore it this way?
						PGUtil.ReadString(network_stream, connection.encoding);
						break;
					
					case 'E':
						// This is the ErrorResponse.
						
						// Get the string error returned and throw a NpgsqlException.
						error_message = PGUtil.ReadString(network_stream, connection.encoding);
					
						// Even when there is an error, the backend send the ReadyForQuery message.
						// So, continue waiting for this message.
						break;
					case 'Z':
						// This is the ReadyForQuery message.
						// It indicates the process termination.
						ready_for_query = true;
						break;
					
					default:
						// This is a message that we should handle and we don't.
						// Throw a NpgsqlException saying that.
						// [FIXME] Better exception handling. Close the connection???
						// This is really ugly!! Right now, the message that wasn`t handled
						// isn't specified! 
						
						throw new NpgsqlException("Bug! A message should be handled in ExecuteNonQuery.");
				}
			}
			
			if (error_message != null)
				throw new NpgsqlException(error_message);
			
			return rows_affected;
		}
		
		public IDataReader ExecuteReader()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteReader()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			throw new NotImplementedException();	
		}
		
		public IDataReader ExecuteReader(CommandBehavior cb)
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteReader()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			throw new NotImplementedException();	
			
		}
		
		public Object ExecuteScalar()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ExecuteScalar()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			// [FIXME] This code was copied from ExecuteNonQuery.
			// Maybe should it be put in a private util method?? 
			
			NetworkStream network_stream = connection.tcp_connection.GetStream();
			BufferedStream output_stream = new BufferedStream(network_stream);
			
			// Send the query to server.
			// Write the byte 'Q' to identify a query message.
			output_stream.WriteByte((byte)'Q');
			
			// Write the query. In this case it is the CommandText text.
			// It is a string terminated by a C NULL character.
			PGUtil.WriteString(text, output_stream, connection.encoding);
			
			// Send bytes.
			output_stream.Flush();
			
			// Now, enter in the loop to process the response.
			
			Boolean ready_for_query = false;
			String error_message = null;			
			
			Object result = null;
			
			// Used in the RowDescription, AsciiRow and BinaryRow messages.
			NpgsqlRowDescription rd = null;
			
			Int32 message;
			
			while (!ready_for_query)
			{
				message = network_stream.ReadByte();
				switch (message)
				{
					case 'C':
						// This is the CompletedResponse message.
						// Get the string returned.
						
						String ret_string = PGUtil.ReadString(network_stream, connection.encoding);
						String[] ret_string_tokens = ret_string.Split(null);	// whitespace separator.
						
						// [FIXME] Just ignore the command string returned?
						
						// Now wait for ReadyForQuery message.
						break;
					
					case 'P':
						// This is the cursor response message. 
						// It is followed by a C NULL terminated string with the name of 
						// the cursor in a FETCH case or 'blank' otherwise.
						// In this case it should be always 'blank'.
						// [FIXME] Get another name for this function.
						// String cursor_name = GetStringFromNetStream(network_stream);
						String cursor_name = PGUtil.ReadString(network_stream, connection.encoding);
						
						// Continue wainting for ReadyForQuery message.
						break;

					case 'T':
						// This is the RowDescription message.
						
						rd = new NpgsqlRowDescription();
						rd.ReadFromStream(network_stream, connection.encoding);
						
						// Now wait for the AsciiRow messages.
						break;
						
					case 'D':
						// This is the AsciiRow message.
						
						NpgsqlAsciiRow ascii_row = new NpgsqlAsciiRow(rd);
						ascii_row.ReadFromStream(network_stream, connection.encoding);
						
						// Just get the first row.
						if(result == null)
							// Now convert the string to the field type.
							// [FIXME] Hardcoded values for int types and string.
							// Change to NpgsqlDbType.
							// For while only int4 and string are strong typed.
							// Any other type will be returned as string.
							
							switch (rd[0].type_oid)
							{
								case 23:	// int4, integer.
									result = Convert.ToInt32(ascii_row[0]);
									break;
								case 25:  // text
									// Get only the first column.
									result = ascii_row[0];
									break;
								default:
									NpgsqlEventLog.LogMsg("Unrecognized datatype returned by ExecuteScalar():" + 
									                      rd[0].type_oid + " Returning String...", LogLevel.Debug);
									result = ascii_row[0];
									break;
							}
						
						// Now wait for CompletedResponse message.
						break;
					
					case 'I':
						// This is the EmptyQueryResponse.
						
						// [FIXME] Just ignore it this way?
						PGUtil.ReadString(network_stream, connection.encoding);
						break;
					
					case 'E':
						// This is the ErrorResponse.
						// Get the string error returned and throw a NpgsqlException.
						
						error_message = PGUtil.ReadString(network_stream, connection.encoding);
					
						// Even when there is an error, the backend send the ReadyForQuery message.
						// So, continue waiting for this message.
						break;
					case 'Z':
						// This is the ReadyForQuery message.
						// It indicates the process termination.
						ready_for_query = true;
						break;
					
					default:
						// This is a message that we should handle and we don't.
						// Throw a NpgsqlException saying that.
						// [FIXME] Better exception handling. Close the connection???
												
						throw new NpgsqlException("Bug! A message should be handled in ExecuteNonQuery. Message as Int32 = " + message);
				}
			}
			
			if (error_message != null)
				throw new NpgsqlException(error_message);
			
			return result;
			
		}
		
		
		public void Prepare()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Prepare()", LogLevel.Debug);
		  
			// Check the connection state.
			CheckConnectionState();
			
			// [TODO] Finish method implementation.
			throw new NotImplementedException();
			
		}
		
		public void Dispose()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Dispose()", LogLevel.Debug);
			
		}
		
		///<summary>
		/// This method checks the connection state to see if the connection
		/// is set or it is open. If one of this conditions is not met, throws
		/// an InvalidOperationException
		///</summary>
		
		private void CheckConnectionState()
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".CheckConnectionState()", LogLevel.Debug);
		  
			// Check the connection state.
			if (connection == null)
				throw new InvalidOperationException("The Connection is not set");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("The Connection is not open");
			
		}						
	}
}
