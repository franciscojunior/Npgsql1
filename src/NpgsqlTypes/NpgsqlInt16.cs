// NpgsqlTypes.NpgsqlInt16.cs
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
	public struct NpgsqlInt16 : INullable, IComparable
	{
#region Fields

		private short _value;
		private bool notNull;

		public static readonly NpgsqlInt16 MaxValue = new NpgsqlInt16 (32767);
		public static readonly NpgsqlInt16 MinValue = new NpgsqlInt16 (-32768);
		public static readonly NpgsqlInt16 Null;
		public static readonly NpgsqlInt16 Zero = new NpgsqlInt16 (0);

#endregion

#region Constructors

		public NpgsqlInt16 (short value) 
		{
			this._value = value;
			notNull = true;;
		}

#endregion

#region Properties

		public bool IsNull 
		{ 
			get { return !notNull; }
		}

		public short Value 
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

		public static NpgsqlInt16 Add (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x + y);
		}

		public static NpgsqlInt16 BitwiseAnd (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x & y);
		}

		public static NpgsqlInt16 BitwiseOr (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is NpgsqlInt16))
				throw new ArgumentException ();
			else if (((NpgsqlInt16)value).IsNull)
				return 1;
			else
				return this._value.CompareTo (((NpgsqlInt16)value).Value);
		}

		public static NpgsqlInt16 Divide (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is NpgsqlInt16))
				return false;
			else if (this.IsNull && ((NpgsqlInt16)value).IsNull)
				return true;
			else if (((NpgsqlInt16)value).IsNull)
				return false;
			else
				return (bool)(this._value == ((NpgsqlInt16)value)._value);
		}

		public static NpgsqlBoolean Equals (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			if (this.IsNull)
				return 0;
			return (int)_value;
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x >= y);
		}

		public static NpgsqlBoolean LessThan (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x <= y);
		}

		public static NpgsqlInt16 Mod (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x % y);
		}

		public static NpgsqlInt16 Multiply (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x != y);
		}

		public static NpgsqlInt16 OnesComplement (NpgsqlInt16 x)
		{
			if (x.IsNull)
				return Null;

			return ~x;
		}

		public static NpgsqlInt16 Parse (string s)
		{
			checked 
			{
				return new NpgsqlInt16 (Int16.Parse (s));
			}
		}

		public static NpgsqlInt16 Subtract (NpgsqlInt16 x, NpgsqlInt16 y)
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

	/*	public NpgsqlDecimal ToNpgsqlDecimal ()
		{
			return ((NpgsqlDecimal)this);
		}
*/
		public NpgsqlDouble ToNpgsqlDouble ()
		{
			return ((NpgsqlDouble)this);
		}

		public NpgsqlInt32 ToNpgsqlInt32 ()
		{
			return ((NpgsqlInt32)this);
		}

		public NpgsqlInt64 ToNpgsqlInt64 ()
		{
			return ((NpgsqlInt64)this);
		}

		public NpgsqlMoney ToNpgsqlMoney ()
		{
			return ((NpgsqlMoney)this);
		}

		public NpgsqlSingle ToNpgsqlSingle ()
		{
			return ((NpgsqlSingle)this);
		}

		/*public NpgsqlString ToNpgsqlString ()
		{
			return ((NpgsqlString)this);
		}
*/
		public override string ToString ()
		{
			if (this.IsNull)
				return "Null";
			else
				return _value.ToString ();
		}

		public static NpgsqlInt16 Xor (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return (x ^ y);
		}
#endregion

#region Operators
		public static NpgsqlInt16 operator + (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			checked 
			{
				return new NpgsqlInt16 ((short) (x.Value + y.Value));
			}
		}

		public static NpgsqlInt16 operator & (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return new NpgsqlInt16 ((short) (x.Value & y.Value));
		}

		public static NpgsqlInt16 operator | (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return new NpgsqlInt16 ((short) ((byte) x.Value | (byte) y.Value));
		}

		public static NpgsqlInt16 operator / (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			checked 
			{
				return new NpgsqlInt16 ((short) (x.Value / y.Value));
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlInt16 operator ^ (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return new NpgsqlInt16 ((short) (x.Value ^ y.Value));
		}

		public static NpgsqlBoolean operator > (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else 
				return new NpgsqlBoolean (!(x.Value == y.Value));
		}

		public static NpgsqlBoolean operator < (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlInt16 operator % (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			return new NpgsqlInt16 ((short) (x.Value % y.Value));
		}

		public static NpgsqlInt16 operator * (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			checked 
			{
				return new NpgsqlInt16 ((short) (x.Value * y.Value));
			}
		}

		public static NpgsqlInt16 operator ~ (NpgsqlInt16 x)
		{
			if (x.IsNull)
				return Null;
			
			return new NpgsqlInt16 ((short) (~x.Value));
		}

		public static NpgsqlInt16 operator - (NpgsqlInt16 x, NpgsqlInt16 y)
		{
			checked 
			{
				return new NpgsqlInt16 ((short) (x.Value - y.Value));
			}
		}

		public static NpgsqlInt16 operator - (NpgsqlInt16 n)
		{
			checked 
			{
				return new NpgsqlInt16 ((short) (-n.Value));
			}
		}

		public static explicit operator NpgsqlInt16 (NpgsqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new NpgsqlInt16 ((short)x.ByteValue);
		}

		public static explicit operator NpgsqlInt16 (NpgsqlDecimal x)
		{		
			checked 
			{
				if (x.IsNull)
					return Null;
				else 
					return new NpgsqlInt16 ((short)x.Value);
			}
		}

		public static explicit operator NpgsqlInt16 (NpgsqlDouble x)
		{
			checked {
				if (x.IsNull)
					return Null;
				else 
					return new NpgsqlInt16 (checked ((short)x.Value));
			}
		}

		public static explicit operator short (NpgsqlInt16 x)
		{
			return x.Value; 
		}

		public static explicit operator NpgsqlInt16 (NpgsqlInt32 x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else 
					return new NpgsqlInt16 ((short)x.Value);
			}
		}

		public static explicit operator NpgsqlInt16 (NpgsqlInt64 x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlInt16 ((short)x.Value);
			}
		}

		public static explicit operator NpgsqlInt16 (NpgsqlMoney x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else 
					return new NpgsqlInt16 ((short)x.Value);
			}			
		}


		public static explicit operator NpgsqlInt16 (NpgsqlSingle x)
		{
			
			checked 
			{
				if (x.IsNull)
					return Null;
				else 
					return new NpgsqlInt16 ((short)x.Value);
			}
		}

		/*public static explicit operator NpgsqlInt16 (NpgsqlString x)
		{	
			if (x.IsNull)
				return Null;

			return NpgsqlInt16.Parse (x.Value);
		}
*/
		public static implicit operator NpgsqlInt16 (short x)
		{
			return new NpgsqlInt16 (x);
		}

		public static implicit operator NpgsqlInt16 (NpgsqlByte x)
		{
			return new NpgsqlInt16 ((short)x.Value);
		}

#endregion
	}
}
			
