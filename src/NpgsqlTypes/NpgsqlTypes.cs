// NpgsqlTypes.NpgsqlTypesHelper.cs
//
// Author:
//	Glen Parker <glenebob@nwlink.com>
//
//	Copyright (C) 2004 The Npgsql Development Team
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

// This file provides implementations of PostgreSQL specific data types that cannot
// be mapped to standard .NET classes.

using System;
using System.Collections;
using System.Globalization;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using System.Resources;
using System.Drawing;

namespace NpgsqlTypes
{

	/// <summary>
	/// Represents a PostgreSQL Point type
	/// </summary>
	
	public struct NpgsqlPoint
	{
		private Single _X;
		private Single _Y;
		
		public NpgsqlPoint(Single X, Single Y)
		{
			_X = X;
			_Y = Y;
		}
		
		public Single X
		{
			get
			{
				return _X;
			}
			
			set
			{
				_X = value;
			}
		}
		
		
		public Single Y
		{
			get
			{
				return _Y;
			}
			
			set
			{
				_Y = value;
			}
		}
	}
	
	public struct NpgsqlBox
	{
		private NpgsqlPoint _LeftTop;
		private NpgsqlPoint _RightBottom;
		
		public NpgsqlBox(NpgsqlPoint LeftTop, NpgsqlPoint RightBottom)
		{
			_LeftTop = LeftTop;
			_RightBottom = RightBottom;
		}
		
		public NpgsqlBox(Single X, Single Y, Single Width, Single Height)
		{
			_LeftTop = new NpgsqlPoint(X, Y);
			_RightBottom = new NpgsqlPoint(X + Width, Y + Height); 
		}

		public NpgsqlPoint LeftTop
		{
			get
			{
				return _LeftTop;
			}
			set
			{
				_LeftTop = value;
			}
		}

		public NpgsqlPoint RightBottom
		{
			get
			{
				return _RightBottom;
			}
			set
			{
				_RightBottom = value;
			}
		} 
		
	}
	
	
    /// <summary>
    /// Represents a PostgreSQL Line Segment type.
    /// </summary>
    public struct NpgsqlLSeg
    {
        public PointF     Start;
        public PointF     End;

        public NpgsqlLSeg(PointF Start, PointF End)
        {
            this.Start = Start;
            this.End = End;
        }

        public override String ToString()
        {
            return string.Format("({0}, {1})", Start, End);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Path type.
    /// </summary>
    public struct NpgsqlPath
    {
        internal PointF[]     Points;

        public NpgsqlPath(PointF[] Points)
        {
            this.Points = Points;
        }

        public Int32 Count
        { get { return Points.Length; } }

        public PointF this [Int32 Index]
        { get { return Points[Index]; } }
    }

    /// <summary>
    /// Represents a PostgreSQL Polygon type.
    /// </summary>
    public struct NpgsqlPolygon
    {
        internal PointF[]     Points;

        public NpgsqlPolygon(PointF[] Points)
        {
            this.Points = Points;
        }

        public Int32 Count
        { get { return Points.Length; } }

        public PointF this [Int32 Index]
        { get { return Points[Index]; } }
    }

    /// <summary>
    /// Represents a PostgreSQL Circle type.
    /// </summary>
    public struct NpgsqlCircle
    {
        public PointF        Center;
        public Double        Radius;

        public NpgsqlCircle(PointF Center, Double Radius)
        {
            this.Center = Center;
            this.Radius = Radius;
        }

        public override String ToString()
        {
            return string.Format("({0}), {1}", Center, Radius);
        }
    }
}
