// NpgsqlTypes.NpgsqlDateTime.cs
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

	public struct NpgsqlDateTime
	{
		private DateTime _value;
		private Boolean _notNull;
		
		public static readonly NpgsqlDateTime Null;
		
		public NpgsqlDateTime(DateTime value)
		{
			_value = value;
			_notNull = true;

		}
		
		internal NpgsqlDateTime(String value)
		{
			
			//Console.WriteLine("string from database: {0}, length: {1}", value, value.Length);
			Int32 year, month, day, hour, minute, second, millisecond;
			
			year = Int32.Parse(value.Substring(0, 4));
			month = Int32.Parse(value.Substring(5,2));
			day = Int32.Parse(value.Substring(8,2));
			hour = Int32.Parse(value.Substring(11,2));
			minute = Int32.Parse(value.Substring(14,2));
			second = Int32.Parse(value.Substring(17,2));
			
			if (value.Length > 19)
				millisecond = Int32.Parse(value.Substring(20,value.Length - (19 + 1)));
			else
				millisecond = 0;
			
			this._value = new DateTime(year, month, day, hour, minute, second, millisecond);
			_notNull = true;
		}
		
		public Boolean IsNull
		{
			get
			{
				return !_notNull;
			}
		}
		
		public DateTime Value 
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
		
		public String ToISOString()
		{
			if (this.IsNull)
				return "Null";
			else
				return _value.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fff");
		}
				
		public static explicit operator DateTime (NpgsqlDateTime x)
		{
			return x.Value;
				
		}
		
		public static explicit operator NpgsqlDateTime(NpgsqlString x)
		{
			return new NpgsqlDateTime(x.Value);
		}
		
		public static NpgsqlDateTime Parse(String x)
		{
			return new NpgsqlDateTime(x);
		}
	}
}
