// NpgsqlTypes.NpgsqlInt64.cs
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
using System.Globalization;

namespace NpgsqlTypes
{
	public struct NpgsqlInt64 : INullable, IComparable
	{

#region Fields

		long _value;

		private bool notNull;
		
		public static readonly NpgsqlInt64 MaxValue = new NpgsqlInt64 (9223372036854775807);
		public static readonly NpgsqlInt64 MinValue = new NpgsqlInt64 (-9223372036854775808);

		public static readonly NpgsqlInt64 Null;
		public static readonly NpgsqlInt64 Zero = new NpgsqlInt64 (0);

#endregion

#region Constructors

		public NpgsqlInt64 (long value) 
		{
			this._value = value;
			notNull = true;
		}

#endregion

#region Properties

		public bool IsNull 
		{ 
			get { return !notNull; }
		}

		public long Value 
		{ 
			get 
			{ 
				if (this.IsNull) 
					throw new NpgsqlNullValueException ();
				else 
					return _value; 
			}
		}

#endregion

#region Methods

		public static NpgsqlInt64 Add (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x + y);
		}

		public static NpgsqlInt64 BitwiseAnd (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x & y);
		}

		public static NpgsqlInt64 BitwiseOr (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is NpgsqlInt64))
				throw new ArgumentException ();
			else if (((NpgsqlInt64)value).IsNull)
				return 1;
			else
				return this._value.CompareTo (((NpgsqlInt64)value).Value);
		}

		public static NpgsqlInt64 Divide (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is NpgsqlInt64))
				return false;
			else if (this.IsNull && ((NpgsqlInt64)value).IsNull)
				return true;
			else if (((NpgsqlInt64)value).IsNull)
				return false;
			else
				return (bool) (this.Value == ((NpgsqlInt64)value).Value);
		}

		public static NpgsqlBoolean Equals (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long val;
			if (this.IsNull)
				return 0;

			val = this._value;
			return val.GetHashCode();
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x >= y);
		}

		public static NpgsqlBoolean LessThan (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x <= y);
		}

		public static NpgsqlInt64 Mod (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x % y);
		}

		public static NpgsqlInt64 Multiply (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x != y);
		}

		public static NpgsqlInt64 OnesComplement (NpgsqlInt64 x)
		{
			if (x.IsNull)
				return Null;

			return ~x;
		}


		public static NpgsqlInt64 Parse (string s)
		{
			checked 
			{
				return new NpgsqlInt64 (Int64.Parse (s));
			}
		}

		public static NpgsqlInt64 Subtract (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x - y);
		}

		public NpgsqlBoolean ToNpgsqlBoolean ()
		{
			return ((NpgsqlBoolean)this);
		}
		
		public NpgsqlByte ToNpgsqlByte ()
		{
			return ((NpgsqlByte)this);
		}

/*		public NpgsqlDecimal ToNpgsqlDecimal ()
		{
			return ((NpgsqlDecimal)this);
		}

*/		public NpgsqlDouble ToNpgsqlDouble ()
		{
			return ((NpgsqlDouble)this);
		}

		public NpgsqlInt16 ToNpgsqlInt16 ()
		{
			return ((NpgsqlInt16)this);
		}

		public NpgsqlInt32 ToNpgsqlInt32 ()
		{
			return ((NpgsqlInt32)this);
		}

		public NpgsqlMoney ToNpgsqlMoney ()
		{
			return ((NpgsqlMoney)this);
		}

		public NpgsqlSingle ToNpgsqlSingle ()
		{
			return ((NpgsqlSingle)this);
		}

	/*	public NpgsqlString ToNpgsqlString ()
		{
			return ((NpgsqlString)this);
		}
*/
		public override string ToString ()
		{
			if (this.IsNull)
				return "Null";

			return _value.ToString ();
		}

		public static NpgsqlInt64 Xor (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return (x ^ y);
		}
#endregion
		
#region Operators
		public static NpgsqlInt64 operator + (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			checked 
			{
				return new NpgsqlInt64 (x.Value + y.Value);
			}
		}

		public static NpgsqlInt64 operator & (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return new NpgsqlInt64 (x.Value & y.Value);
		}

		public static NpgsqlInt64 operator | (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return new NpgsqlInt64 (x.Value | y.Value);
		}

		public static NpgsqlInt64 operator / (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			checked 
			{
				return new NpgsqlInt64 (x.Value / y.Value);
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlInt64 operator ^ (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return new NpgsqlInt64 (x.Value ^ y.Value);
		}

		public static NpgsqlBoolean operator > (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (!(x.Value == y.Value));
		}

		public static NpgsqlBoolean operator < (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlInt64 operator % (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			return new NpgsqlInt64(x.Value % y.Value);
		}

		public static NpgsqlInt64 operator * (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			checked 
			{
				return new NpgsqlInt64 (x.Value * y.Value);
			}
		}

		public static NpgsqlInt64 operator ~ (NpgsqlInt64 x)
		{
			if (x.IsNull)
				return NpgsqlInt64.Null;

			return new NpgsqlInt64 (~(x.Value));
		}

		public static NpgsqlInt64 operator - (NpgsqlInt64 x, NpgsqlInt64 y)
		{
			checked 
			{
				return new NpgsqlInt64 (x.Value - y.Value);
			}
		}

		public static NpgsqlInt64 operator - (NpgsqlInt64 n)
		{
			return new NpgsqlInt64 (-(n.Value));
		}

		public static explicit operator NpgsqlInt64 (NpgsqlBoolean x)
		{
			if (x.IsNull) 
				return NpgsqlInt64.Null;
			else
				return new NpgsqlInt64 ((long)x.ByteValue);
		}

		public static explicit operator NpgsqlInt64 (NpgsqlDecimal x)
		{
			checked 
			{
				if (x.IsNull) 
					return NpgsqlInt64.Null;
				else
					return new NpgsqlInt64 ((long)x.Value);
			}
		}

		public static explicit operator NpgsqlInt64 (NpgsqlDouble x)
		{
			checked
			{
				if (x.IsNull) 
					return NpgsqlInt64.Null;
				else
					return new NpgsqlInt64 ((long)x.Value);
			}
		}

		public static explicit operator long (NpgsqlInt64 x)
		{
			return x.Value;
		}

		public static explicit operator NpgsqlInt64 (NpgsqlMoney x)
		{
			checked 
			{
				if (x.IsNull) 
					return NpgsqlInt64.Null;
				else
					return new NpgsqlInt64 ((long)x.Value);
			}
		}

		public static explicit operator NpgsqlInt64 (NpgsqlSingle x)
		{
			checked 
			{
				if (x.IsNull) 
					return NpgsqlInt64.Null;
				else
					return new NpgsqlInt64 ((long)x.Value);
			}
		}

		public static explicit operator NpgsqlInt64 (NpgsqlString x)
		{
			checked 
			{
				return NpgsqlInt64.Parse (x.Value);
			}
		}

		public static implicit operator NpgsqlInt64 (long x)
		{
			return new NpgsqlInt64 (x);
		}

		public static implicit operator NpgsqlInt64 (NpgsqlByte x)
		{
			if (x.IsNull) 
				return NpgsqlInt64.Null;
			else
				return new NpgsqlInt64 ((long)x.Value);
		}

		public static implicit operator NpgsqlInt64 (NpgsqlInt16 x)
		{
			if (x.IsNull) 
				return NpgsqlInt64.Null;
			else
				return new NpgsqlInt64 ((long)x.Value);
		}

		public static implicit operator NpgsqlInt64 (NpgsqlInt32 x)
		{
			if (x.IsNull) 
				return NpgsqlInt64.Null;
			else
				return new NpgsqlInt64 ((long)x.Value);
		}

#endregion
	}
}
			
