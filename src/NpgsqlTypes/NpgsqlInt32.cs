// NpgsqlTypes.NpgsqlInt32.cs
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
	public struct NpgsqlInt32 : INullable, IComparable 
	{
#region Fields

		private int _value;
		private bool notNull;

		public static readonly NpgsqlInt32 MaxValue = new NpgsqlInt32 (2147483647);
		public static readonly NpgsqlInt32 MinValue = new NpgsqlInt32 (-2147483648);
		public static readonly NpgsqlInt32 Null;
		public static readonly NpgsqlInt32 Zero = new NpgsqlInt32 (0);

#endregion

#region Constructors

		public NpgsqlInt32(int value) 
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

		public int Value 
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

		public static NpgsqlInt32 Add (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x + y);
		}

		public static NpgsqlInt32 BitwiseAnd(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x & y);
		}
		
		public static NpgsqlInt32 BitwiseOr(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x | y);
		}

		public int CompareTo(object comparer) 
		{
			if (comparer == null)
				return 1;
			else if (!(comparer is NpgsqlInt32))
				throw new ArgumentException ("Value is not a NpgsqlInt32");
			else if (((NpgsqlInt32)comparer).IsNull)
				return 1;
			else
				return this._value.CompareTo (((NpgsqlInt32)comparer).Value);
		}

		public static NpgsqlInt32 Divide(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x / y);
		}

		public override bool Equals(object _value) 
		{
			if (!(_value is NpgsqlInt32))
				return false;
			else if (this.IsNull && ((NpgsqlInt32)_value).IsNull)
				return true;
			else if (((NpgsqlInt32)_value).IsNull)
				return false;
			else
				return (bool) (this == (NpgsqlInt32)_value);
		}

		public static NpgsqlBoolean Equals(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return _value;
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x >= y);
		}
                
		public static NpgsqlBoolean LessThan(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x <= y);
		}

		public static NpgsqlInt32 Mod(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x % y);
		}

		public static NpgsqlInt32 Multiply(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x != y);
		}

		public static NpgsqlInt32 OnesComplement(NpgsqlInt32 x) 
		{
			return ~x;
		}

		public static NpgsqlInt32 Parse(string s) 
		{
			return new NpgsqlInt32 (Int32.Parse (s));
		}

		public static NpgsqlInt32 Subtract(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x - y);
		}

		public NpgsqlBoolean ToNpgsqlBoolean() 
		{
			return ((NpgsqlBoolean)this);
		}

		public NpgsqlByte ToNpgsqlByte() 
		{
			return ((NpgsqlByte)this);
		}

		/*public NpgsqlDecimal ToNpgsqlDecimal() 
		{
			return ((NpgsqlDecimal)this);
		}
*/
		public NpgsqlDouble ToNpgsqlDouble() 	
		{
			return ((NpgsqlDouble)this);
		}

		public NpgsqlInt16 ToNpgsqlInt16() 
		{
			return ((NpgsqlInt16)this);
		}

		public NpgsqlInt64 ToNpgsqlInt64() 
		{
			return ((NpgsqlInt64)this);
		}

		public NpgsqlMoney ToNpgsqlMoney() 
		{
			return ((NpgsqlMoney)this);
		}

		public NpgsqlSingle ToNpgsqlSingle() 
		{
			return ((NpgsqlSingle)this);
		}

		/*public NpgsqlString ToNpgsqlString ()
		{
			return ((NpgsqlString)this);
		}
*/
		public override string ToString() 
		{
			if (this.IsNull)
				return "Null";
			else
				return _value.ToString ();
		}

		public static NpgsqlInt32 Xor(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return (x ^ y);
		}

		#endregion

#region Operators

		public static NpgsqlInt32 operator + (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			checked 
			{
				return new NpgsqlInt32 (x.Value + y.Value);
			}
		}

		public static NpgsqlInt32 operator & (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return new NpgsqlInt32 (x.Value & y.Value);
		}

		public static NpgsqlInt32 operator | (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			checked 
			{
				return new NpgsqlInt32 (x.Value | y.Value);
			}
		}

		public static NpgsqlInt32 operator / (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			checked 
			{
				return new NpgsqlInt32 (x.Value / y.Value);
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlInt32 operator ^ (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return new NpgsqlInt32 (x.Value ^ y.Value);
		}

		public static NpgsqlBoolean operator >(NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value != y.Value);
		}
		
		public static NpgsqlBoolean operator < (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlInt32 operator % (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			return new NpgsqlInt32 (x.Value % y.Value);
		}

		public static NpgsqlInt32 operator * (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			checked 
			{
				return new NpgsqlInt32 (x.Value * y.Value);
			}
		}

		public static NpgsqlInt32 operator ~ (NpgsqlInt32 x) 
		{
			return new NpgsqlInt32 (~x.Value);
		}

		public static NpgsqlInt32 operator - (NpgsqlInt32 x, NpgsqlInt32 y) 
		{
			checked 
			{
				return new NpgsqlInt32 (x.Value - y.Value);
			}
		}

		public static NpgsqlInt32 operator - (NpgsqlInt32 x) 
		{
			return new NpgsqlInt32 (-x.Value);
		}

		public static explicit operator NpgsqlInt32 (NpgsqlBoolean x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new NpgsqlInt32 ((int)x.ByteValue);
		}

		public static explicit operator NpgsqlInt32 (NpgsqlDecimal x) 
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else 
					return new NpgsqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator NpgsqlInt32 (NpgsqlDouble x) 
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else 
					return new NpgsqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator Int32 (NpgsqlInt32 x)
		{
			return x.Value;
		}
		
		

		public static explicit operator NpgsqlInt32 (NpgsqlInt64 x) 
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else 
					return new NpgsqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator NpgsqlInt32(NpgsqlMoney x) 
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else 
					return new NpgsqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator NpgsqlInt32(NpgsqlSingle x) 
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else 
					return new NpgsqlInt32 ((int)x.Value);
			}
		}

		public static explicit operator NpgsqlInt32(NpgsqlString x) 
		{
			checked 
			{
				return NpgsqlInt32.Parse (x.Value);
			}
		}

		public static implicit operator NpgsqlInt32(int x) 
		{
			return new NpgsqlInt32 (x);
		}

		public static implicit operator NpgsqlInt32(NpgsqlByte x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new NpgsqlInt32 ((int)x.Value);
		}

		public static implicit operator NpgsqlInt32(NpgsqlInt16 x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new NpgsqlInt32 ((int)x.Value);
		}

#endregion
	}
}
