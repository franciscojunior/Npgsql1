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

namespace Npgsql
{
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
