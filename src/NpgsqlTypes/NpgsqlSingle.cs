// NpgsqlTypes.NpgsqlSingle.cs
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
	public struct NpgsqlSingle : INullable, IComparable
	{
#region Fields

		float value;
		private bool notNull;

		public static readonly NpgsqlSingle MaxValue = new NpgsqlSingle (3.40282346638528859E+38f);
		public static readonly NpgsqlSingle MinValue = new NpgsqlSingle (-3.40282346638528859E+38f);
		public static readonly NpgsqlSingle Null;
		public static readonly NpgsqlSingle Zero = new NpgsqlSingle (0);

#endregion

#region Constructors

		public NpgsqlSingle (double value) 
		{
			this.value = (float)value;
			notNull = true;
		}

		public NpgsqlSingle (float value) 
		{
			this.value = value;
			notNull = true;
		}

		public NpgsqlSingle (string _value)
		{
			this.value = float.Parse(_value);
			notNull = true;
		}

                #endregion

#region Properties

		public bool IsNull 
		{ 
			get { return !notNull; }
		}

		public float Value 
		{ 
			get 
			{ 
				if (this.IsNull) 
					throw new NpgsqlNullValueException ();
				else 
					return value; 
			}
		}

                #endregion

#region Methods

		public static NpgsqlSingle Add (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is NpgsqlSingle))
				throw new ArgumentException ("Value is not a System.Data.NpgsqlTypes.NpgsqlSingle");
			else if (((NpgsqlSingle)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((NpgsqlSingle)value).Value);
		}

		public static NpgsqlSingle Divide (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is NpgsqlSingle))
				return false;
			else if (this.IsNull && ((NpgsqlSingle)value).IsNull)
				return true;
			else if (((NpgsqlSingle)value).IsNull)
				return false;
			else
				return (bool) (this == (NpgsqlSingle)value);
		}

		public static NpgsqlBoolean Equals (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long) value;
			return (int)(LongValue ^ (LongValue >> 32));
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x >= y);
		}

		public static NpgsqlBoolean LessThan (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x <= y);
		}

		public static NpgsqlSingle Multiply (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals (NpgsqlSingle x, NpgsqlSingle y)
		{
			return (x != y);
		}

		public static NpgsqlSingle Parse (string s)
		{
			return new NpgsqlSingle (Single.Parse (s));
		}

		public static NpgsqlSingle Subtract (NpgsqlSingle x, NpgsqlSingle y)
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

		/*public NpgsqlDecimal ToNpgsqlDecimal ()
		{
			return ((NpgsqlDecimal)this);
		}
*/
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


	/*	public NpgsqlString ToNpgsqlString ()
		{
			return ((NpgsqlString)this);
		}

	*/	public override string ToString ()
		{
			return value.ToString ();
		}
#endregion

#region Operators
		public static NpgsqlSingle operator + (NpgsqlSingle x, NpgsqlSingle y)
		{
			checked 
			{
				return new NpgsqlSingle ((float)(x.Value + y.Value));
			}
		}

		public static NpgsqlSingle operator / (NpgsqlSingle x, NpgsqlSingle y)
		{
			checked 
			{
				return new NpgsqlSingle (x.Value / y.Value);
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlSingle x, NpgsqlSingle y)
		{
			if (x.IsNull || y .IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlBoolean operator > (NpgsqlSingle x, NpgsqlSingle y)
		{
			if (x.IsNull || y .IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlSingle x, NpgsqlSingle y)
		{
			if (x.IsNull || y .IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlSingle x, NpgsqlSingle y)
		{
			if (x.IsNull || y .IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (!(x.Value == y.Value));
		}

		public static NpgsqlBoolean operator < (NpgsqlSingle x, NpgsqlSingle y)
		{
			if (x.IsNull || y .IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlSingle x, NpgsqlSingle y)
		{
			if (x.IsNull || y .IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlSingle operator * (NpgsqlSingle x, NpgsqlSingle y)
		{
			checked 
			{
				return new NpgsqlSingle (x.Value * y.Value);
			}
		}

		public static NpgsqlSingle operator - (NpgsqlSingle x, NpgsqlSingle y)
		{
			checked 
			{
				return new NpgsqlSingle (x.Value - y.Value);
			}
		}

		public static NpgsqlSingle operator - (NpgsqlSingle n)
		{
			return new NpgsqlSingle (-(n.Value));
		}

		public static explicit operator NpgsqlSingle (NpgsqlBoolean x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
                                
				return new NpgsqlSingle((float)x.ByteValue);
			}
		}

		public static explicit operator NpgsqlSingle (NpgsqlDouble x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
                                
				return new NpgsqlSingle((float)x.Value);
			}
		}

		public static explicit operator float (NpgsqlSingle x)
		{
			return x.Value;
		}

		/*public static explicit operator NpgsqlSingle (NpgsqlString x)
		{
			checked 
			{
				if (x.IsNull)
					return Null;
                                
				return NpgsqlSingle.Parse (x.Value);
			}
		}
*/
		public static implicit operator NpgsqlSingle (float x)
		{
			return new NpgsqlSingle (x);
		}

		public static implicit operator NpgsqlSingle (NpgsqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else 
				return new NpgsqlSingle((float)x.Value);
		}

		public static implicit operator NpgsqlSingle (NpgsqlDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlSingle((float)x.Value);
		}

		public static implicit operator NpgsqlSingle (NpgsqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlSingle((float)x.Value);
		}

		public static implicit operator NpgsqlSingle (NpgsqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlSingle((float)x.Value);
		}

		public static implicit operator NpgsqlSingle (NpgsqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlSingle((float)x.Value);
		}

		public static implicit operator NpgsqlSingle (NpgsqlMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlSingle((float)x.Value);
		}

#endregion
	}
}
