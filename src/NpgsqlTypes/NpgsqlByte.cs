// NpgsqlTypes.NpgsqlByte.cs
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
	public struct NpgsqlByte : INullable, IComparable
	{

#region Fields

		byte _value;
		private bool notNull;

		public static readonly NpgsqlByte MaxValue = new NpgsqlByte (0xff);
		public static readonly NpgsqlByte MinValue = new NpgsqlByte (0);
		public static readonly NpgsqlByte Null;
		public static readonly NpgsqlByte Zero = new NpgsqlByte (0);

#endregion

#region Constructors

		public NpgsqlByte (byte value) 
		{
			this._value = value;
			notNull = true;
		}

#endregion

#region Properties

		public bool IsNull 
		{
			get 
			{ 
				return !notNull; 
			}
		}

		public byte Value 
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

		public static NpgsqlByte Add (NpgsqlByte x, NpgsqlByte y)
		{
			return (x + y);
		}

		public static NpgsqlByte BitwiseAnd (NpgsqlByte x, NpgsqlByte y)
		{
			return (x & y);
		}

		public static NpgsqlByte BitwiseOr (NpgsqlByte x, NpgsqlByte y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is NpgsqlByte))
				throw new ArgumentException ();
			else if (((NpgsqlByte)value).IsNull)
				return 1;
			else
				return this._value.CompareTo (((NpgsqlByte)value).Value);
		}

		public static NpgsqlByte Divide (NpgsqlByte x, NpgsqlByte y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is NpgsqlByte))
				return false;
			else if (this.IsNull && ((NpgsqlByte)value).IsNull)
				return true;
			else if (((NpgsqlByte)value).IsNull)
				return false;
			else
				return (bool) (this.Value == ((NpgsqlByte)value).Value);
		}

		public static NpgsqlBoolean Equals (NpgsqlByte x, NpgsqlByte y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			if(this.IsNull)
				return 0;
			return (int)_value;
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlByte x, NpgsqlByte y)
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlByte x, NpgsqlByte y)
		{
			return (x >= y);
		}

		public static NpgsqlBoolean LessThan (NpgsqlByte x, NpgsqlByte y)
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual (NpgsqlByte x, NpgsqlByte y)
		{
			return (x <= y);
		}

		public static NpgsqlByte Mod (NpgsqlByte x, NpgsqlByte y)
		{
			return (x % y);
		}

		public static NpgsqlByte Multiply (NpgsqlByte x, NpgsqlByte y)
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals (NpgsqlByte x, NpgsqlByte y)
		{
			return (x != y);
		}

		public static NpgsqlByte OnesComplement (NpgsqlByte x)
		{
			return ~x;
		}

		public static NpgsqlByte Parse (string s)
		{
			checked 
			{
				return new NpgsqlByte (Byte.Parse (s));
			}
		}

		public static NpgsqlByte Subtract (NpgsqlByte x, NpgsqlByte y)
		{
			return (x - y);
		}

		public NpgsqlBoolean ToNpgsqlBoolean ()
		{
			return ((NpgsqlBoolean)this);
		}
		
		public NpgsqlDecimal ToNpgsqlDecimal ()
		{
			return ((NpgsqlDecimal)this);
		}

		public NpgsqlDouble ToNpgsqlDouble ()
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

		public NpgsqlString ToNpgsqlString ()
		{
			return ((NpgsqlString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return "Null";
			else
				return _value.ToString ();
		}

		public static NpgsqlByte Xor (NpgsqlByte x, NpgsqlByte y)
		{
			return (x ^ y);
		}
#endregion

#region Operators
		public static NpgsqlByte operator + (NpgsqlByte x, NpgsqlByte y)
		{
			checked 
			{
				return new NpgsqlByte ((byte) (x.Value + y.Value));
			}
		}

		public static NpgsqlByte operator & (NpgsqlByte x, NpgsqlByte y)
		{
			return new NpgsqlByte ((byte) (x.Value & y.Value));
		}

		public static NpgsqlByte operator | (NpgsqlByte x, NpgsqlByte y)
		{
			return new NpgsqlByte ((byte) (x.Value | y.Value));
		}

		public static NpgsqlByte operator / (NpgsqlByte x, NpgsqlByte y)
		{
			checked 
			{
				return new NpgsqlByte ((byte) (x.Value / y.Value));
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlByte x, NpgsqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlByte operator ^ (NpgsqlByte x, NpgsqlByte y)
		{
			return new NpgsqlByte ((byte) (x.Value ^ y.Value));
		}

		public static NpgsqlBoolean operator > (NpgsqlByte x, NpgsqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlByte x, NpgsqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlByte x, NpgsqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (!(x.Value == y.Value));
		}

		public static NpgsqlBoolean operator < (NpgsqlByte x, NpgsqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlByte x, NpgsqlByte y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlByte operator % (NpgsqlByte x, NpgsqlByte y)
		{
			return new NpgsqlByte ((byte) (x.Value % y.Value));
		}

		public static NpgsqlByte operator * (NpgsqlByte x, NpgsqlByte y)
		{
			checked 
			{
				return new NpgsqlByte ((byte) (x.Value * y.Value));
			}
		}

		public static NpgsqlByte operator ~ (NpgsqlByte x)
		{
			return new NpgsqlByte ((byte) ~x.Value);
		}

		public static NpgsqlByte operator - (NpgsqlByte x, NpgsqlByte y)
		{
			checked 
			{
				return new NpgsqlByte ((byte) (x.Value - y.Value));
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new NpgsqlByte (x.ByteValue);
		}

		public static explicit operator byte (NpgsqlByte x)
		{
			return x.Value;
		}

		public static explicit operator NpgsqlByte (NpgsqlDecimal x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else 
					return new NpgsqlByte ((byte)x.Value);
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlDouble x)
		{
			
			checked 
			{
				if (x.IsNull)
					return Null;
				else 					
					return new NpgsqlByte ((byte)x.Value);
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlInt16 x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else 			       
					return new NpgsqlByte ((byte)x.Value);
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlInt32 x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else 			       
					return new NpgsqlByte ((byte)x.Value);
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlInt64 x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlByte ((byte)x.Value);
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlMoney x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else 					
					return new NpgsqlByte ((byte)x.Value);
			}
		}

		public static explicit operator NpgsqlByte (NpgsqlSingle x)
		{
			checked {
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlByte ((byte)x.Value);
			}
		}


		public static explicit operator NpgsqlByte (NpgsqlString x)
		{			
			checked 
			{
				return NpgsqlByte.Parse (x.Value);
			}
		}

		public static implicit operator NpgsqlByte (byte x)
		{
			return new NpgsqlByte (x);
		}
		
		#endregion
	}
}
			
