// created on 1/6/2002 at 22:27

// Npgsql.PGUtil.cs
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
using System.IO;
using System.Text;

namespace Npgsql
{
	///<summary>
	/// This class provides many util methods to handle 
	/// reading and writing of PostgreSQL protocol messages.
	/// </summary>
	/// [FIXME] Does this name fully represent the class responsability?
	/// Should it be abstract or with a private constructor to prevent
	/// creating instances?
	
	// 
	internal sealed class PGUtil
	{
		
				
		///<summary>
		/// This method gets a C NULL terminated string from the network stream.
		/// It keeps reading a byte in each time until a NULL byte is returned.
		/// It returns the resultant string of bytes read.
		/// This string is sent from backend.
		/// </summary>
		
		public static String ReadString(Stream network_stream, Encoding encoding)
		{
			// [FIXME] Is 512 enough?
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
			
			return encoding.GetString(buffer, 0, counter);
		}
		
		///<summary>
		/// This method writes a C NULL terminated string to the network stream.
		/// It appends a NULL terminator to the end of the String.
		/// </summary>
		
		public static void WriteString(String the_string, Stream network_stream, Encoding encoding)
		{
			network_stream.Write(encoding.GetBytes(the_string + '\x00') , 0, the_string.Length + 1);
		}
		
	}
}
