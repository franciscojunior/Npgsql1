// created on 18/5/2002 at 00:59

// Npgsql.NpgsqlParameterCollection.cs
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
using System.Collections;


namespace Npgsql
{
	
	// Use ArrayList as base class so that we can get a lot of required methods implemented.
	
	// [TODO] Implement more Add methods that construct the Parameter object.
	// [TODO] Remove dependency on ArrayList. Implement the interfaces by hand.
	
	public class NpgsqlParameterCollection : ArrayList, IDataParameterCollection
	{
		
		public override Int32 Add(Object parameter)
		{
			// Call the add version that receives a NpgsqlParameter as parameter
			try
			{
				return Add((NpgsqlParameter) parameter);
			}
			catch(InvalidCastException e)
			{
				throw new NpgsqlException("Only NpgsqlParameter objects can be added to collection.", e);
			}
		}
		
		public Int32 Add(NpgsqlParameter parameter)
		{
			// Check if the parameter has at least a name.
			if (parameter.ParameterName != null)
				// Add the parameter
				return base.Add(parameter);
			else
				throw new NpgsqlException("A parameter must have a name when added to collection");
			
		}
		
		public Boolean Contains(String parameterName)
		{
			// Check if parameterName is in the collection.
			return (IndexOf(parameterName) != -1);
		}
				
		public Int32 IndexOf(String parameterName)
		{
			// Iterate values to see what is the index of parameter.
			Int32 index = 0;
			
			foreach(NpgsqlParameter parameter in this)
			{
				if (parameter.ParameterName == parameterName)
					return index;
				index++;
					
			}
			return -1;
		}
		
		public void RemoveAt(String parameterName)
		{
			base.RemoveAt(IndexOf(parameterName));
		}
		
		public Object this[String parameterName]
		{
			get
			{
				return base[IndexOf(parameterName)];
			}
			set
			{
				base[IndexOf(parameterName)] = value;
			}
		}
	}
}
