// NpgsqlTypes.NpgsqlDouble.cs
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
	public struct NpgsqlDouble : INullable, IComparable
	{
#region Fields
		Double value;

		private bool notNull;

		public static readonly NpgsqlDouble MaxValue = new NpgsqlDouble (1.7976931348623157E+308);
		public static readonly NpgsqlDouble MinValue = new NpgsqlDouble (-1.7976931348623157E+308);
		public static readonly NpgsqlDouble Null;
		public static readonly NpgsqlDouble Zero = new NpgsqlDouble (0);

#endregion

#region Constructors

		public NpgsqlDouble (double value) 
		{
			if(System.Double.IsInfinity(value) || System.Double.IsNaN(value))
				throw new OverflowException();
			this.value = value;
			notNull = true;
		}


#endregion

#region Properties

		public bool IsNull 
		{ 
			get { return !notNull; }
		}

		public double Value 
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

		public static NpgsqlDouble Add (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is NpgsqlDouble))
				throw new ArgumentException ();
			else if (((NpgsqlDouble)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((NpgsqlDouble)value).Value);
		}

		public static NpgsqlDouble Divide (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is NpgsqlDouble))
				return false;
			if (this.IsNull && ((NpgsqlDouble)value).IsNull)
				return true;
			else if (((NpgsqlDouble)value).IsNull)
				return false;
			else
				return (bool) (this == (NpgsqlDouble)value);
		}

		public static NpgsqlBoolean Equals (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long)value;
			return (int)(LongValue ^ (LongValue >> 32));
                        
		}

		public static NpgsqlBoolean GreaterThan (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x > y);
		}

		public static NpgsqlBoolean GreaterThanOrEqual (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x >= y);
		}

		public static NpgsqlBoolean LessThan (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x < y);
		}

		public static NpgsqlBoolean LessThanOrEqual (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x <= y);
		}

		public static NpgsqlDouble Multiply (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x * y);
		}

		public static NpgsqlBoolean NotEquals (NpgsqlDouble x, NpgsqlDouble y)
		{
			return (x != y);
		}

		public static NpgsqlDouble Parse (string s)
		{
			return new NpgsqlDouble (Double.Parse (s));
		}

		public static NpgsqlDouble Subtract (NpgsqlDouble x, NpgsqlDouble y)
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
		}*/

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

		/*public NpgsqlString ToNpgsqlString ()
		{
			return ((NpgsqlString)this);
		}*/

		public override string ToString ()
		{
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}
#endregion		
		
#region Operators		
		public static NpgsqlDouble operator + (NpgsqlDouble x, NpgsqlDouble y)
		{
			checked 
			{
				return new NpgsqlDouble (x.Value + y.Value);
			}
		}

		public static NpgsqlDouble operator / (NpgsqlDouble x, NpgsqlDouble y)
		{
			checked 
			{
				return new NpgsqlDouble (x.Value / y.Value);
			}
		}

		public static NpgsqlBoolean operator == (NpgsqlDouble x, NpgsqlDouble y)
		{
			if (x.IsNull || y.IsNull)         
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlBoolean operator > (NpgsqlDouble x, NpgsqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value > y.Value);
		}

		public static NpgsqlBoolean operator >= (NpgsqlDouble x, NpgsqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value >= y.Value);
		}

		public static NpgsqlBoolean operator != (NpgsqlDouble x, NpgsqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (!(x.Value == y.Value));
		}

		public static NpgsqlBoolean operator < (NpgsqlDouble x, NpgsqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value < y.Value);
		}

		public static NpgsqlBoolean operator <= (NpgsqlDouble x, NpgsqlDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value <= y.Value);
		}

		public static NpgsqlDouble operator * (NpgsqlDouble x, NpgsqlDouble y)
		{
			checked 
			{
				return new NpgsqlDouble (x.Value * y.Value);
			}
		}

		public static NpgsqlDouble operator - (NpgsqlDouble x, NpgsqlDouble y)
		{
			checked 
			{
				return new NpgsqlDouble (x.Value - y.Value);
			}
		}

		public static NpgsqlDouble operator - (NpgsqlDouble n)
		{                        
			return new NpgsqlDouble (-(n.Value));
		}

		public static explicit operator NpgsqlDouble (NpgsqlBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.ByteValue);
		}

		public static explicit operator double (NpgsqlDouble x)
		{
			return x.Value;
		}

		public static explicit operator NpgsqlDouble (NpgsqlString x)
		{
			checked 
			{
				return NpgsqlDouble.Parse (x.Value);
			}
		}

		public static implicit operator NpgsqlDouble (double x)
		{
			return new NpgsqlDouble (x);
		}

		public static implicit operator NpgsqlDouble (NpgsqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.Value);
		}

		/*public static implicit operator NpgsqlDouble (NpgsqlDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble (x.ToDouble ());
		}*/

		public static implicit operator NpgsqlDouble (NpgsqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.Value);
		}

		public static implicit operator NpgsqlDouble (NpgsqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.Value);
		}

		public static implicit operator NpgsqlDouble (NpgsqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.Value);
		}

		public static implicit operator NpgsqlDouble (NpgsqlMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.Value);
		}

		public static implicit operator NpgsqlDouble (NpgsqlSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new NpgsqlDouble ((double)x.Value);
		}
#endregion
	}
}
