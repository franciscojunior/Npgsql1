// NpgsqlTypes.NpgsqlDecimal.cs
// 
// Author:
//	Victor Vatamanescu (victor.vatamanescu@hqsoftware.ro)
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

namespace NpgsqlTypes
{

	public struct NpgsqlDecimal : INullable
	{
		
		// Good reference about Decimal encoding:
		// http://www2.hursley.ibm.com/decimal/dcspec.html
		
		private Decimal _value;
		
		private Boolean _isNotNull;
		
		public static readonly NpgsqlDecimal Null;
		
		public NpgsqlDecimal(Decimal value)
		{
			_value = value;
			_isNotNull = true;

		}
		
		public Boolean IsNull
		{
			get
			{
				return !_isNotNull;
			}
			
		}
		
		
		public Decimal Value 
		{ 
			get 
			{ 
				if (this.IsNull) 
					throw new NpgsqlNullValueException ();
				else 
					return _value; 
			}
		}
		
		public override String ToString()
		{
			if (this.IsNull)
				return "Null";
			else
				return _value.ToString();
		}
		
		public static explicit operator NpgsqlDecimal (NpgsqlBoolean x) 
		{
			
			if (x.IsNull)
				return Null;
			else
				if (x.IsTrue)
					return new NpgsqlDecimal(1);
				else
					return new NpgsqlDecimal(0);
			
	
		}
		
		public static explicit operator NpgsqlDecimal (NpgsqlByte x) 
		{
			
			if (x.IsNull)
				return Null;
			else
				return new NpgsqlDecimal(x.Value);
			
	
		}
		
		public static explicit operator Decimal (NpgsqlDecimal x)
		{
			return x.Value;
		}
		
		public static NpgsqlDecimal Parse(String x)
		{
			return new NpgsqlDecimal(Decimal.Parse(x));
		}
		
		
	}
}
