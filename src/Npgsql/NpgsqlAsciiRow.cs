// created on 13/6/2002 at 21:06

// Npgsql.NpgsqlAsciiRow.cs
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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;

namespace Npgsql
{
	
	/// <summary>
	/// This class represents the AsciiRow message sent from PostgreSQL
	/// server.
	/// </summary>
	/// 
	internal sealed class NpgsqlAsciiRow
	{
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlAsciiRow";
		
		private NpgsqlRowDescription	desc;
		private ArrayList							data;
		private Byte[]								null_map_array;
		
		
		public NpgsqlAsciiRow(NpgsqlRowDescription rowDescription)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NpgsqlAsciiRow()", LogLevel.Debug);
			
			desc = rowDescription;
			data = new ArrayList();
			null_map_array = null;
		}
		
		
		public void ReadFromStream(Stream inputStream, Encoding encoding)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ReadFromStream()", LogLevel.Debug);
			
			Byte[] input_buffer = new Byte[300]; //[FIXME] Is this enough??
			Int16 num_fields = desc.NumFields;
			null_map_array = new Byte[(num_fields + 7)/8];
					
			// Read the null fields bitmap.
			inputStream.Read(null_map_array, 0, (num_fields + 7)/8 );
			
			// Get the data.
			for (Int32 field_count = 0; field_count < num_fields; field_count++)
			{
				// Check if this field isn't null
				if (IsNull(field_count))
					// Field is null just keep next field.
					continue;
							
				// Read the first data of the first row.
				// Read the size of the field + sizeof(Int32)
				inputStream.Read(input_buffer, 0, 4);
				Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
				
				// Now, read just the field value.
				inputStream.Read(input_buffer, 0, field_value_size - 4);
		
				// Read the bytes as string.
				String result = new String(encoding.GetChars(input_buffer, 0, field_value_size - 4));
				
				// Add them to the AsciiRow data.
				data.Add(result);
				
			}
		}
		
		
		public Boolean IsNull(Int32 index)
		{
			// [FIXME] Check more optimized way of doing this.
			// Should this be public or private?
			
			// Check if the value (index) of the field is null 
			
			// Get the byte that holds the bit index position.
			Byte test_byte = null_map_array[index/8];
			
			// Now, check if index bit is set.
			// To this, get its position in the byte, shift to 
			// MSB and test it with the byte 10000000.
    	return (((test_byte << (index%8)) & 0x80) == 0);
		}
			
		
		public NpgsqlRowDescription RowDescription
		{
			get
			{
				return desc;
			}
		}
		
		public Object this[Int32 index]
		{
			get
			{
				// [FIXME] Should return null or something else
				// more meaningful?
				return (IsNull(index) ? null : data[index]);
			}
		}
	}
	
}
