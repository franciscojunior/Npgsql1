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
		  
			return CreateParameter();
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
						
						// String ret_string = GetStringFromNetStream(network_stream);
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
						// network_stream.Read(input_buffer, 0, 1);
						//GetStringFromNetStream(network_stream);
						PGUtil.ReadString(network_stream, connection.encoding);
						break;
					
					case 'E':
						// This is the ErrorResponse.
						// Get the string error returned and throw a NpgsqlException.
						
						
						// error_message = GetStringFromNetStream(network_stream);
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
			
			// [TODO] Finish method implementation.
			//throw new NotImplementedException();
			
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
						
			// [FIXME] Is it really necessary? How big?
			Byte[] input_buffer = new Byte[1500];
			
			// Now, enter in the loop to process the response.
			
			Boolean ready_for_query = false;
			String error_message = null;			
			
			Int16 num_fields = 0;
			
			Object result = null;
			
			// [FIXME] Change from Int32 to NpgsqlDbType enum when ready.
			Int32 result_type = 0;
			
			
			Int32 message;
			
			while (!ready_for_query)
			{
				message = network_stream.ReadByte();
				switch (message)
				{
					case 'C':
						// This is the CompletedResponse message.
						// Get the string returned.
						
						//String ret_string = GetStringFromNetStream(network_stream);
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
						
						// Get the number of fields in a row.
						network_stream.Read(input_buffer, 0, 2);
						num_fields = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 0));
						
						// [FIXME] Just ignore for now the RowDescription message data.
						// We only care about the first field of the first row.
						String field_name;
						Int32 field_type_oid;
						Int16 field_type_size;
						Int32 field_type_modifier;
						
						field_name = PGUtil.ReadString(network_stream, connection.encoding);
							
						network_stream.Read(input_buffer, 0, 4 + 2 + 4);
						
						field_type_oid = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
						field_type_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 4));
						field_type_modifier = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 6));
					
						// Cache the result type of 
						result_type = field_type_oid;
					
						// Ignore other fields.
						// [FIXME] Change to a single Read method ??
						// Note that we are starting from the second field if it exists.
						for (Int16 i = 1; i < num_fields; i++)
						{
							//field_name = GetStringFromNetStream(network_stream);
							field_name = PGUtil.ReadString(network_stream, connection.encoding);
							
							network_stream.Read(input_buffer, 0, 4 + 2 + 4);
						}
						
						// Now wait for the AsciiRow messages.
						break;
						
					case 'D':
						// This is the AsciiRow message.
						
						// Read the bitmap of null fields of the row.
						
						// The expression gets the number of bytes necessary to
						// hold the number of num_fields as a number of bits.
						// In reality, this should be almost always 1, because it
						// is the only field that must be processed.
						
						// [FIXME] For now, ignore the field mask. 
						network_stream.Read(input_buffer, 0, (num_fields + 7)/8 );
							
						for (Int32 field_count = 0; field_count < num_fields; field_count++)
						{
							
							// Read the first data of the first row.
							// Read the size of the field + sizeof(Int32)
							network_stream.Read(input_buffer, 0, 4);
							Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
							
							// Now, read just the field value.
							network_stream.Read(input_buffer, 0, field_value_size - 4);
					
							// Get only the first value.
							if (result == null)
							{
								
								// Read the bytes as string.
								result = new String(connection.encoding.GetChars(input_buffer, 0, field_value_size - 4));
								
								// Now convert the string to the field type.
								// [FIXME] Hardcoded values for int types and string.
								// Change to NpgsqlDbType.
								// For while only int4 and string are strong typed.
								// Any other type will be returned as string.
								switch (result_type)
								{
									case 23:	// int4, integer.
										result = Convert.ToInt32(result);
										break;
									
								}
							}
							else 
							{
								// Just ignore the fields values.
								connection.encoding.GetChars(input_buffer, 0, field_value_size - 4);
							}
						}
						
						// Now wait for CompletedResponse message.
						break;
					
					case 'I':
						// This is the EmptyQueryResponse.
						// [FIXME] Just ignore it this way?
						// network_stream.Read(input_buffer, 0, 1);
						// GetStringFromNetStream(network_stream);
						PGUtil.ReadString(network_stream, connection.encoding);
						break;
					
					case 'E':
						// This is the ErrorResponse.
						// Get the string error returned and throw a NpgsqlException.
						
						// [FIXME] The function GetStringFromNetStream should be used in NpgsqlConnection class too.
						// So, this function is a cadidate in potential to be a static method in a util class.
						
						// error_message = GetStringFromNetStream(network_stream);
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
			
			// [TODO] Finish method implementation.
			//throw new NotImplementedException();
			
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
