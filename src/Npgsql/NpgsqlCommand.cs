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
using System.Net.Sockets;

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
			parameters = null;
			timeout = 30;
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
		
		public IDataParameterCollection Parameters
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
				
			// [TODO] Finish method implementation.
			throw new NotImplementedException();
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
			
			throw new NotImplementedException();	
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
		
		
	}
}
