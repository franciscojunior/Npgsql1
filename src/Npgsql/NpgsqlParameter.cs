// created on 18/5/2002 at 01:25

// Npgsql.NpgsqlParameter.cs
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

namespace Npgsql
{
	///<summary>
	/// This class represents a parameter to a command that will be sent to server
	///</summary>
	public sealed class NpgsqlParameter : IDbDataParameter, IDataParameter
	{
		// Fields to implement IDbDataParameter interface.
		private byte 				precision;
		private byte 				scale;
		private Int32				size;
		
		// Fields to implement IDataParameter
		private DbType				type;
		private ParameterDirection	direction;
		private Boolean				is_nullable;
		private String				parameter_name;
		private String				source_column;
		private DataRowVersion		source_version;
		private Object				value;
		
		
		// Implementation of IDbDataParameter
		
		public byte Precision
		{
			get
			{
				return precision;
			}
			
			set
			{
				precision = value;
			}
		}
		
		public byte Scale
		{
			get
			{
				return scale;
			}
			
			set
			{
				scale = value;
			}
		}
		
		public Int32 Size
		{
			get
			{
				return size;
			}
			
			set
			{
				size = value;
			}
		}
		
		public DbType DbType
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
		
		public ParameterDirection Direction
		{
			get
			{
				return direction;
			}
			
			set
			{
				direction = value;
			}
		}
		
		public Boolean IsNullable
		{
			get
			{
				return is_nullable;
			}
			
			set
			{
				is_nullable = value;
			}
		}
		
		public String ParameterName
		{
			get
			{
				return parameter_name;
			}
			
			set
			{
				parameter_name = value;
			}
		}
		
		public String SourceColumn 
		{
			get
			{
				return source_column;
			}
			
			set
			{
				source_column = value;
			}
		}
		
		public DataRowVersion SourceVersion
		{
			get
			{
				return source_version;
			}
			
			set
			{
				source_version = value;
			}
		}
		
		public Object Value
		{
			get
			{
				return value;
			}
			
			set
			{
				this.value = value;
			}
		}
	}
}
