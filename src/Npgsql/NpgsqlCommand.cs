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
		
		
		
		
		// Constructors
		
		public NpgsqlCommand() : this(null, null){}
		
		public NpgsqlCommand(String cmdText) : this(cmdText, null){}
		
		
		public NpgsqlCommand(String cmdText, NpgsqlConnection connection)
		{
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
			// [TODO] Finish method implementation.
			throw new NotImplementedException();
		}
		
		
		IDbDataParameter IDbCommand.CreateParameter()
		{
			return CreateParameter();
		}
		
		public NpgsqlParameter CreateParameter()
		{
			return new NpgsqlParameter();
		}
		
		public Int32 ExecuteNonQuery()
		{
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
						
						String ret_string = GetStringFromNetStream(network_stream);
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
						String cursor_name = GetStringFromNetStream(network_stream);
						
						// Continue wainting for ReadyForQuery message.
						break;

					case 'I':
						// This is the EmptyQueryResponse.
						// [FIXME] Just ignore it this way?
						// network_stream.Read(input_buffer, 0, 1);
						GetStringFromNetStream(network_stream);
						break;
					
					case 'E':
						// This is the ErrorResponse.
						// Get the string error returned and throw a NpgsqlException.
						
						// [FIXME] The function GetStringFromNetStream should be used in NpgsqlConnection class too.
						// So, this function is a cadidate in potential to be a static method in a util class.
						
						error_message = GetStringFromNetStream(network_stream);
					
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
			// Check the connection state.
			CheckConnectionState();
			
			throw new NotImplementedException();	
		}
		
		public IDataReader ExecuteReader(CommandBehavior cb)
		{
			// Check the connection state.
			CheckConnectionState();
			
			throw new NotImplementedException();	
			
		}
		
		public Object ExecuteScalar()
		{
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
			output_stream.Write(connection.encoding.GetBytes(text + '\x00') , 0, text.Length + 1);
			
			// Send bytes.
			output_stream.Flush();
			
			//throw new NotImplementedException();
			
			
			// [FIXME] Is it really necessary? How big?
			Byte[] input_buffer = new Byte[1500];
			
			// Now, enter in the loop to process the response.
			
			Boolean ready_for_query = false;
			String error_message = null;			
			
			Int16 num_fields;
			
			Object result = null;
			
			Int32 message;
			
			while (!ready_for_query)
			{
				message = network_stream.ReadByte();
				switch (message)
				{
					case 'C':
						// This is the CompletedResponse message.
						// Get the string returned.
						
						String ret_string = GetStringFromNetStream(network_stream);
						String[] ret_string_tokens = ret_string.Split(null);	// whitespace separator.
						
						/*
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
					
						*/
						// Now wait for ReadyForQuery message.
						break;
					
					case 'P':
						// This is the cursor response message. 
						// It is followed by a C NULL terminated string with the name of 
						// the cursor in a FETCH case or 'blank' otherwise.
						// In this case it should be always 'blank'.
						// [FIXME] Get another name for this function.
						String cursor_name = GetStringFromNetStream(network_stream);
						
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
						// Get the data about each field.
						for (Int16 i = 0; i < num_fields; i++)
						{
							field_name = GetStringFromNetStream(network_stream);
							
							network_stream.Read(input_buffer, 0, 4 + 2 + 4);
						}
						
						field_type_oid = BitConverter.ToInt32(input_buffer, 0);
						field_type_size = BitConverter.ToInt16(input_buffer, 4);
						field_type_modifier = BitConverter.ToInt32(input_buffer, 6);
					
						
						// Now wait for the AsciiRow messages.
						break;
						
					case 'D':
						// This is the AsciiRow message.
						
						// Read the bitmap of null fields of the row.
						// [FIXME] It is hardcoded for just one field.
						
						network_stream.Read(input_buffer, 0, 1);
												
						// [FIXME] For now, ignore the field mask. Read the first data
						// of the first row.
						network_stream.Read(input_buffer, 0, 4);
						Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
						network_stream.Read(input_buffer, 0, field_value_size - 4);
					
						// [FIXME] Assume a string for now.
						result = new String(connection.encoding.GetChars(input_buffer, 0, field_value_size - 4));
					
						// Now wait for CompletedResponse message.
						break;
					
					case 'I':
						// This is the EmptyQueryResponse.
						// [FIXME] Just ignore it this way?
						// network_stream.Read(input_buffer, 0, 1);
						GetStringFromNetStream(network_stream);
						break;
					
					case 'E':
						// This is the ErrorResponse.
						// Get the string error returned and throw a NpgsqlException.
						
						// [FIXME] The function GetStringFromNetStream should be used in NpgsqlConnection class too.
						// So, this function is a cadidate in potential to be a static method in a util class.
						
						error_message = GetStringFromNetStream(network_stream);
					
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
						
						throw new NpgsqlException("Bug! A message should be handled in ExecuteNonQuery." + (Char)message);
				
					
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
			// Check the connection state.
			CheckConnectionState();
			
			// [TODO] Finish method implementation.
			throw new NotImplementedException();
			
		}
		
		public void Dispose()
		{
			
		}
		
		///<summary>
		/// This method checks the connection state to see if the connection
		/// is set or it is open. If one of this conditions is not met, throws
		/// an InvalidOperationException
		///</summary>
		
		private void CheckConnectionState()
		{
			// Check the connection state.
			if (connection == null)
				throw new InvalidOperationException("The Connection is not set");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException("The Connection is not open");
			
		}
		
		
		///<summary>
		/// This method gets a C NULL terminated string from the network stream.
		/// It keeps reading a byte in each time until a NULL byte is returned.
		/// It returns the resultant string of bytes read.
		/// This string is sent from backend.
		/// </summary>
		
		private String GetStringFromNetStream(NetworkStream network_stream)
		{
			// [FIXME] Is 512 enough? At least it can't be more than MTU = 1500 on ethernet.
			Byte[] buffer = new Byte[512];
			Byte b;
			Int16 counter = 0;
			
			// [FIXME] Is this cast always safe?
			b = (Byte)network_stream.ReadByte();
			while(b != 0)
			{
				buffer[counter] = b;
				counter++;
				b = (Byte)network_stream.ReadByte();
			}
			
			return connection.encoding.GetString(buffer, 0, counter);
		}
	}
}
