// created on 4/3/2003 at 19:45

// Npgsql.NpgsqlBinaryRow.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using NpgsqlTypes;


namespace Npgsql
{
	
	/// <summary>
	/// This class represents the AsciiRow message sent from PostgreSQL
	/// server.
	/// </summary>
	/// 
	internal sealed class NpgsqlBinaryRow
	{
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlBinaryRow";
		
		private ArrayList							data;
		private Byte[]								null_map_array;
		private readonly Int16	READ_BUFFER_SIZE = 300; //[FIXME] Is this enough??
		private NpgsqlRowDescription row_desc;
		
		public NpgsqlBinaryRow(NpgsqlRowDescription rowDesc)
		{
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
						
			data = new ArrayList();
			row_desc = rowDesc;
			null_map_array = new Byte[(row_desc.NumFields + 7)/8];
						
			
		}
		
		
		public void ReadFromStream(Stream inputStream, Encoding encoding)
		{
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream");
						
			//Byte[] input_buffer = new Byte[READ_BUFFER_SIZE]; 
			Byte[] input_buffer = null;
			
			Array.Clear(null_map_array, 0, null_map_array.Length);
			
			// Read the null fields bitmap.
			inputStream.Read(null_map_array, 0, null_map_array.Length );
			
			// Get the data.
			for (Int16 field_count = 0; field_count < row_desc.NumFields; field_count++)
			{
				
				// Check if this field isn't null
				if (IsNull(field_count))
				{
					// Field is null just keep next field.
					
					//[FIXME] See this[] method.
					data.Add(null);
					continue;
				}
				
				// Read the first data of the first row.
								
				PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, 4);
								
				Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
							
				Int32 bytes_left = field_value_size; //Size of data is the value read.
				
				input_buffer = new Byte[bytes_left];
				
				
				//StringBuilder result = new StringBuilder();
				
				/*while (bytes_left > READ_BUFFER_SIZE)
				{
					// Now, read just the field value.
					PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, READ_BUFFER_SIZE);
					
					// Read the bytes as string.
					result.Append(new String(encoding.GetChars(input_buffer, 0, READ_BUFFER_SIZE)));
									
					bytes_left -= READ_BUFFER_SIZE;
				}
				*/
				// Now, read just the field value.
				PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, bytes_left);
				
				// Read the bytes as string.
				//result.Append(new String(encoding.GetChars(input_buffer, 0, bytes_left)));
				
				
				// Add them to the BinaryRow data.
				//data.Add(NpgsqlTypesHelper.ConvertStringToNpgsqlType(result.ToString(), row_desc[field_count].type_oid));
				data.Add(input_buffer);
				
			}
			
		}
		
		
		public Boolean IsNull(Int32 index)
		{
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IsNull");
			// [FIXME] Check more optimized way of doing this.
			// Should this be public or internal?
			
			// Check valid index range.
			if ((index < 0) || (index >= row_desc.NumFields))
					throw new ArgumentOutOfRangeException("index");
			
			// Check if the value (index) of the field is null 
			
			// Get the byte that holds the bit index position.
			Byte test_byte = null_map_array[index/8];
			
			// Now, check if index bit is set.
			// To this, get its position in the byte, shift to 
			// MSB and test it with the byte 10000000.
    	return (((test_byte << (index%8)) & 0x80) == 0);
		}
			
		
		public Object this[Int32 index]
		{
			get
			{
				NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, index);
				if ((index < 0) || (index >= row_desc.NumFields))
					throw new ArgumentOutOfRangeException("this[] index value");
				// [FIXME] Should return null or something else
				// more meaningful?
				
				//[FIXME] This code assumes that the data arraylist has the null and non null values
				// in order, but just the non-null values are added. 
				// It is necessary to map the index value with the elements in the array list.
				// For now, the workaround is to insert the null values in the array list. 
				// But this is a hack. :)
				
				//return (IsNull(index) ? null : data[index]);
				return data[index];
				
				
				
			}
		}
	}
	
}
