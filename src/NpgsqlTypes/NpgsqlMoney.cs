// NpgsqlTypes.NpgsqlMoney.cs
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
	public struct NpgsqlMoney : INullable, IComparable
	{
		
#region Fields

		private decimal _value;	
		private bool notNull;

		public static readonly NpgsqlMoney MaxValue = new NpgsqlMoney (922337203685477.5807);
		public static readonly NpgsqlMoney MinValue = new NpgsqlMoney (-922337203685477.5808);
		public static readonly NpgsqlMoney Null;
		public static readonly NpgsqlMoney Zero = new NpgsqlMoney (0);

#endregion

#region Constructors

		public NpgsqlMoney (decimal value) 
		{
			this._value = value;
			notNull = true;
		}

		public NpgsqlMoney (double value) 
		{
			this._value = (decimal)value;
			notNull = true;
		}

		public NpgsqlMoney (int value) 
		{
			this._value = (decimal)value;
			notNull = true;
		}

		public NpgsqlMoney (long value) 
		{
			this._value = (decimal)value;
			notNull = true;
		}

#endregion

#region Properties

		public bool IsNull 
		{ 
			get { return !notNull; }
		}

		public decimal Value 
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

		public static NpgsqlMoney Add (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is NpgsqlMoney))
				throw new ArgumentException ("Value is not a System.Data.NpgsqlTypes.NpgsqlMoney");
			else if (((NpgsqlMoney)value).IsNull)
				return 1;
			else
				return this._value.CompareTo (((NpgsqlMoney)_value).Value);
		}

		public static NpgsqlMoney Divide (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is NpgsqlMoney))
				return false;
			if (this.IsNull && ((NpgsqlMoney)value).IsNull)
				return true;
			else if (((NpgsqlMoney)value).IsNull)
				return false;
			else
				return (bool) (this == (NpgsqlMoney)value);
		}

		public static NpgsqlBoolean Equals (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)_value;
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x >= y);
		}

		public static NpgsqlBoolean LessThan (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x <= y);
		}

		public static NpgsqlMoney Multiply (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x != y);
		}

		public static NpgsqlMoney Parse (string s)
		{
			decimal d = Decimal.Parse (s);

			if (d > NpgsqlMoney.MaxValue.Value || d < NpgsqlMoney.MinValue.Value) 
				throw new OverflowException ("");
			
			return new NpgsqlMoney (d);
		}

		public static NpgsqlMoney Subtract (NpgsqlMoney x, NpgsqlMoney y)
		{
			return (x - y);
		}

		public decimal ToDecimal ()
		{
			return _value;
		}

		public double ToDouble ()
		{
			return (double)_value;
		}

		public int ToInt32 ()
		{
			return (int)_value;
		}

		public long ToInt64 ()
		{
			return (long)_value;
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
				return String.Empty;
			else
				return _value.ToString ();
		}

		public static NpgsqlMoney operator + (NpgsqlMoney x, NpgsqlMoney y)
		{
			checked 
			{
				return new NpgsqlMoney (x.Value + y.Value);
			}
		}

		public static NpgsqlMoney operator / (NpgsqlMoney x, NpgsqlMoney y)
		{
			checked 
			{
				return new NpgsqlMoney (x.Value / y.Value);
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlMoney x, NpgsqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlBoolean operator > (NpgsqlMoney x, NpgsqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlMoney x, NpgsqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlMoney x, NpgsqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (!(x.Value == y.Value));
		}

		public static NpgsqlBoolean operator < (NpgsqlMoney x, NpgsqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlMoney x, NpgsqlMoney y)
		{
			if (x.IsNull || y.IsNull) return NpgsqlBoolean.Null;
			return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlMoney operator * (NpgsqlMoney x, NpgsqlMoney y)
		{
			checked 
			{
				return new NpgsqlMoney (x.Value * y.Value);
			}
		}

		public static NpgsqlMoney operator - (NpgsqlMoney x, NpgsqlMoney y)
		{
			checked 
			{
				return new NpgsqlMoney (x.Value - y.Value);
			}
		}

		public static NpgsqlMoney operator - (NpgsqlMoney n)
		{
			return new NpgsqlMoney (-(n.Value));
		}

		public static explicit operator NpgsqlMoney (NpgsqlBoolean x)
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else
					return new NpgsqlMoney ((decimal)x.ByteValue);
			}
		}

		public static explicit operator NpgsqlMoney (NpgsqlDecimal x)
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else
					return new NpgsqlMoney (x.Value);
			}
		}

		public static explicit operator NpgsqlMoney (NpgsqlDouble x)
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else
					return new NpgsqlMoney ((decimal)x.Value);
			}
		}

		public static explicit operator decimal (NpgsqlMoney x)
		{
			return x.Value;
		}

		public static explicit operator NpgsqlMoney (NpgsqlSingle x)
		{
			checked 
			{
				if (x.IsNull) 
					return Null;
				else
					return new NpgsqlMoney ((decimal)x.Value);
			}
		}

		public static explicit operator NpgsqlMoney (NpgsqlString x)
		{
			checked 
			{
				return NpgsqlMoney.Parse (x.Value);
			}
		}

		public static implicit operator NpgsqlMoney (decimal x)
		{
			return new NpgsqlMoney (x);
		}

		public static implicit operator NpgsqlMoney (NpgsqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlMoney ((decimal)x.Value);
		}

		public static implicit operator NpgsqlMoney (NpgsqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlMoney ((decimal)x.Value);
		}

		public static implicit operator NpgsqlMoney (NpgsqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlMoney ((decimal)x.Value);
		}

		public static implicit operator NpgsqlMoney (NpgsqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlMoney ((decimal)x.Value);
		}

		#endregion
	}
}
			
