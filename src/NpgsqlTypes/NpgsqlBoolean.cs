// NpgsqlTypes.NpgsqlBoolean.cs
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
	public struct NpgsqlBoolean : INullable, IComparable 
	{


		byte _value;
		
		
		private bool notNull;

		public static readonly NpgsqlBoolean False = new NpgsqlBoolean (false);
		public static readonly NpgsqlBoolean Null;
		public static readonly NpgsqlBoolean One = new NpgsqlBoolean (1);
		public static readonly NpgsqlBoolean True = new NpgsqlBoolean (true);
		public static readonly NpgsqlBoolean Zero = new NpgsqlBoolean (0);

		public NpgsqlBoolean (bool value) 
		{
			this._value = (byte) (value == true ? 1 : 0);
			notNull = true;
		}

		internal NpgsqlBoolean(string value)
		{
			if (value.Length == 1)
				this._value = (byte) (value.Equals("t") ? 1:0);
			else
				this._value = (byte) (Boolean.Parse(value) == true ? 1 : 0);
			notNull = true;
		}

		public NpgsqlBoolean (int value) 
		{
			this._value = (byte) (value == 0 ? 0 : 1);
			notNull = true;
		}


		public byte ByteValue 
		{
			get 
			{
				if (this.IsNull)
					throw new NpgsqlNullValueException();
				else
					return _value;
			}
		}

		public bool IsFalse 
		{
			get 
			{ 
				if (this.IsNull) 
					return false;
				else 
					return (_value == 0);
			}
		}

		public bool IsNull 
		{
			get 
			{ 
				return !notNull;
			}
		}

		public bool IsTrue 
		{
			get 
			{ 
				if (this.IsNull) 
					return false;
				else 	
					return (_value != 0);
			}
		}

		public bool Value 
		{
			get 
			{ 
				if (this.IsNull)
					throw new NpgsqlNullValueException();
				else
					return this.IsTrue;
			}
		}



		public static NpgsqlBoolean And (NpgsqlBoolean x, NpgsqlBoolean y) 
		{
			return (x & y);
		}

		public int CompareTo (object value) 
		{
			NpgsqlBoolean _bool;
			if ((value is NpgsqlBoolean) != true)
			{
				_bool = (NpgsqlBoolean) value;

				if (this.IsNull)
				{
					if(!_bool.IsNull)
					{
						return -1;
					}
					return 0;
				}

				if (_bool.IsNull)
				{
					return 1;
				}
			
				if(this.ByteValue < _bool.ByteValue)
				{
					return -1;
				}

				if(this.ByteValue > _bool.ByteValue)
				{
					return 1;
				}

				return 0;
			}
			throw new ArgumentException();
		}

		public override bool Equals(object value) 
		{
			if (!(value is NpgsqlBoolean))
				return false;

			if (this.IsNull && ((NpgsqlBoolean)value).IsNull)
				return true;
			else if (((NpgsqlBoolean)value).IsNull)
				return false;
			else
				return (bool) (this == (NpgsqlBoolean)value);
		}

		public static NpgsqlBoolean Equals(NpgsqlBoolean x, NpgsqlBoolean y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			bool _bool;

			if(!(this.IsNull))
			{
				_bool = this.Value;
				return _bool.GetHashCode();
			}
			return 0;
		}

		public static NpgsqlBoolean NotEquals(NpgsqlBoolean x, NpgsqlBoolean y) 
		{
			return (x != y);
		}

		public static NpgsqlBoolean OnesComplement(NpgsqlBoolean x) 
		{
			return ~x;
		}

		public static NpgsqlBoolean Or(NpgsqlBoolean x, NpgsqlBoolean y) 
		{
			return (x | y);
		}

		public static NpgsqlBoolean Parse(string s) 
		{
			return new NpgsqlBoolean (s);
		}

		public NpgsqlByte ToNpgsqlByte() 
		{
			return new NpgsqlByte (_value);
		}

		public NpgsqlDecimal ToNpgsqlDecimal() 
		{
			return ((NpgsqlDecimal)this);
		}

		public NpgsqlDouble ToNpgsqlDouble() 
		{
			return ((NpgsqlDouble)this);
		}

		public NpgsqlInt16 ToNpgsqlInt16() 
		{
			return ((NpgsqlInt16)this);
		}

		public NpgsqlInt32 ToNpgsqlInt32() 
		{
			return ((NpgsqlInt32)this);
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

		public NpgsqlString ToNpgsqlString() 
		{
			if (this.IsNull)
				return new NpgsqlString ("Null");
			if (this.IsTrue)
				return new NpgsqlString ("True");
			else
				return new NpgsqlString ("False");
		}

		public override string ToString() 
		{
			if (this.IsNull)
				return "Null";
			if (this.IsTrue)
				return "True";
			else
				return "False";
		}




		public static NpgsqlBoolean Xor(NpgsqlBoolean x, NpgsqlBoolean y) 
		{
			return (x ^ y);
		}

		public static NpgsqlBoolean operator & (NpgsqlBoolean x, NpgsqlBoolean y)
		{
			return new NpgsqlBoolean (x.Value & y.Value);
		}

		public static NpgsqlBoolean operator | (NpgsqlBoolean x, NpgsqlBoolean y)
		{
			return new NpgsqlBoolean (x.Value | y.Value);

		}

		public static NpgsqlBoolean operator == (NpgsqlBoolean x, NpgsqlBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value == y.Value);
		}

		public static NpgsqlBoolean operator ^ (NpgsqlBoolean x, NpgsqlBoolean y) 
		{
			return new NpgsqlBoolean (x.Value ^ y.Value);
		}

		public static bool operator false (NpgsqlBoolean x) 
		{
			return x.IsFalse;
		}

		public static NpgsqlBoolean operator != (NpgsqlBoolean x, NpgsqlBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (x.Value != y.Value);
		}

		public static NpgsqlBoolean operator ! (NpgsqlBoolean x) 
		{
			if (x.IsNull)
				return NpgsqlBoolean.Null;
			else
				return new NpgsqlBoolean (!x.Value);
		}

		public static NpgsqlBoolean operator ~ (NpgsqlBoolean x) 
		{
			NpgsqlBoolean b;
			if (x.IsTrue)
				b = new NpgsqlBoolean(false);
			else
				b = new NpgsqlBoolean(true);

			return b;
		}

		public static bool operator true (NpgsqlBoolean x) 
		{
			return x.IsTrue;
		}

		public static explicit operator bool (NpgsqlBoolean x) 
		{
			return x.Value;
		}



		public static explicit operator NpgsqlBoolean (NpgsqlByte x) 
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlBoolean ((int)x.Value);
			}
		}


		public static explicit operator NpgsqlBoolean (NpgsqlDecimal x) 
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlBoolean ((int)x.Value);
			}
		}
		

		public static explicit operator NpgsqlBoolean (NpgsqlDouble x) 
		{
			
			checked {
			if (x.IsNull)
				return Null;
			else
				return new NpgsqlBoolean ((int)x.Value);
			}
		}

		public static explicit operator NpgsqlBoolean (NpgsqlInt16 x) 
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlBoolean ((int)x.Value);
			}
		}

		public static explicit operator NpgsqlBoolean (NpgsqlInt32 x) 
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlBoolean (x.Value);
			}
		}


		public static explicit operator NpgsqlBoolean (NpgsqlInt64 x) 
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlBoolean ((int)x.Value);
			}
		}


		public static explicit operator NpgsqlBoolean (NpgsqlMoney x) 
		{
			checked 
			{
				if (x.IsNull)
					return Null;
				else
					return new NpgsqlBoolean ((int)x.Value);
			}
		}


		public static explicit operator NpgsqlBoolean (NpgsqlSingle x) 
		{
			
			checked {
			if (x.IsNull)
				return Null;
			else
				return new NpgsqlBoolean ((int)x.Value);
			}
		}


		public static explicit operator NpgsqlBoolean (NpgsqlString x) 
		{
			checked 
			{
				return NpgsqlBoolean.Parse (x.Value);
			}
		}

		public static implicit operator NpgsqlBoolean (bool x) 
		{
			return new NpgsqlBoolean (x);
		}

	}
}
