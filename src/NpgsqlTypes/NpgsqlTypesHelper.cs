
// NpgsqlTypes.NpgsqlTypesHelper.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
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
using System.Collections;
using System.Globalization;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using System.Resources;
using System.Drawing;

using Npgsql;

namespace NpgsqlTypes
{
    /// <summary>
    ///	This class contains helper methods for type conversion between
    /// the .Net type system and postgresql.
    /// </summary>
    internal abstract class NpgsqlTypesHelper
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlTypesHelper";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlTypesHelper));

        /// <summary>
        /// A cache of basic datatype mappings keyed by server version.  This way we don't
        /// have to load the basic type mappings for every connection.
        /// </summary>
        private static Hashtable _oidToNameMappings = new Hashtable();

        public static String GetBackendTypeNameFromDbType(DbType dbType)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromDbType");

            switch (dbType)
            {
            case DbType.Binary:
                return "bytea";
            case DbType.Boolean:
                return "bool";
            case DbType.Single:
                return "float4";
            case DbType.Double:
                return "float8";
            case DbType.Int64:
                return "int8";
            case DbType.Int32:
                return "int4";
            case DbType.Decimal:
                return "numeric";
            case DbType.Int16:
                return "int2";
            case DbType.String:
            case DbType.AnsiString:
                return "text";
            case DbType.DateTime:
                return "timestamp";
            case DbType.Date:
                return "date";
            case DbType.Time:
                return "time";
            default:
                throw new InvalidCastException(String.Format(resman.GetString("Exception_TypeNotSupported"), dbType));

            }
        }

        public static Object ConvertBackendBytesToSystemType(NpgsqlTypeInfo TypeInfo, Byte[] data, Encoding encoding, Int32 fieldValueSize, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendBytesToStytemType");

            // We are never guaranteed to know about every possible data type the server can send us.
            // When we encounter an unknown type, we punt and return the data without modification.
            if (TypeInfo == null)
                return data;

            switch (TypeInfo.DBType)
            {
            case DbType.Binary:
                return data;
            case DbType.Boolean:
                return BitConverter.ToBoolean(data, 0);
            case DbType.DateTime:
                return DateTime.MinValue.AddTicks(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0)));

            case DbType.Int16:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
            case DbType.Int32:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
            case DbType.Int64:
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, 0));
            case DbType.String:
            case DbType.AnsiString:
            case DbType.StringFixedLength:
                return encoding.GetString(data, 0, fieldValueSize);
            default:
                throw new InvalidCastException("Type not supported in binary format");
            }


        }

        private static String QuoteString(Boolean Quote, String S)
        {
            if (Quote) {
                return String.Format("'{0}'", S);
            } else {
                return S;
            }
        }

        public static String ConvertNpgsqlParameterToBackendStringValue(NpgsqlParameter parameter, Boolean QuoteStrings)
        {
            // HACK (?)
            // glenebob@nwlink.com 05/20/2004
            // bool QuoteString is a bit of a hack.
            // When using the version 3 extended query support, we do not need to do quoting of parameters.
            // The backend handles that properly.

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertNpgsqlParameterToBackendStringValue");

            if ((parameter.Value == DBNull.Value) || (parameter.Value == null))
                return "NULL";

            switch(parameter.DbType)
            {
            case DbType.Binary:
                return QuoteString(QuoteStrings, ConvertByteArrayToBytea((Byte[])parameter.Value));

            case DbType.Boolean:
                return ((bool)parameter.Value) ? "TRUE" : "FALSE";

            case DbType.Int64:
            case DbType.Int32:
            case DbType.Int16:
                return parameter.Value.ToString();

            case DbType.Single:
                // To not have a value implicitly converted to float8, we add quotes.
                return QuoteString(QuoteStrings, ((Single)parameter.Value).ToString(NumberFormatInfo.InvariantInfo));

            case DbType.Double:
                return ((Double)parameter.Value).ToString(NumberFormatInfo.InvariantInfo);

            case DbType.Date:
                return QuoteString(QuoteStrings, ((DateTime)parameter.Value).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));

            case DbType.Time:
                return QuoteString(QuoteStrings, ((DateTime)parameter.Value).ToString("HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo));

            case DbType.DateTime:
                return QuoteString(QuoteStrings, ((DateTime)parameter.Value).ToString("yyyy-MM-dd HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo));

            case DbType.Decimal:
                return ((Decimal)parameter.Value).ToString(NumberFormatInfo.InvariantInfo);

            case DbType.String:
            case DbType.AnsiString:
            case DbType.StringFixedLength:
                return QuoteString(QuoteStrings, parameter.Value.ToString().Replace("'", "''"));

            default:
                // This should not happen!
                throw new InvalidCastException(String.Format(resman.GetString("Exception_TypeNotSupported"), parameter.DbType));

            }

        }


        ///<summary>
        /// This method is responsible to convert the string received from the backend
        /// to the corresponding NpgsqlType.
        /// </summary>
        public static Object ConvertBackendStringToSystemType(NpgsqlTypeInfo TypeInfo, String data, Int16 typeSize, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

            if (TypeInfo != null) {
                return TypeInfo.ConvertToNative(data, typeSize, typeModifier);
            } else {
                return data;
            }
        }

        ///<summary>
        /// This method is responsible to send query to get the oid-to-name mapping
        /// of a few basic natively recognized data types.
        /// This is needed as from one version to another, this mapping can be changed and
        /// so we avoid hardcoding them.
        /// </summary>
        public static NpgsqlTypeMapping LoadInitialTypesMapping(NpgsqlConnector conn)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "LoadTypesMapping");

            // [TODO] Verify another way to get higher concurrency.
            lock(typeof(NpgsqlTypesHelper))
            {
                NpgsqlTypeMapping oidToNameMapping = (NpgsqlTypeMapping) _oidToNameMappings[conn.ServerVersion];

                if (oidToNameMapping != null)
                {
                    return oidToNameMapping;
                }

                oidToNameMapping = new NpgsqlTypeMapping();

                NpgsqlCommand       command = new NpgsqlCommand("select oid, typname from pg_type where typname in ('bool', 'box', 'bytea', 'char', 'circle', 'date', 'float4', 'float8', 'int2', 'int4', 'int8', 'lseg', 'numeric', 'path', 'point', 'polygon', 'text', 'time', 'timestamp', 'timestamptz', 'timetz', 'varchar');", conn);
                NpgsqlDataReader    dr = command.ExecuteReader();

                while (dr.Read())
                {
                    DbType dbtype;
                    Type   systype;
                    String typeName = (String) dr[1];
                    ConvertBackendToNativeHandler BackendConvert = null;
                    ConvertNativeToBackendHandler NativeConvert = null;

                    switch (typeName)
                    {
                    case "bool":
                        dbtype = DbType.Boolean;
                        systype = typeof(Boolean);
                        BackendConvert = new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBoolean);
                        break;
                    case "box":
                        dbtype = DbType.Object;
                        systype = typeof(RectangleF);
                        BackendConvert = new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToRectangle);
                        break;
                    case "bytea":
                        dbtype = DbType.Binary;
                        systype = typeof(System.Byte[]);
                        BackendConvert = new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBinary);
                        break;
                    case "circle":
                        dbtype = DbType.Object;
                        systype = typeof(NpgsqlCircle);
                        BackendConvert = new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToCircle);
                        break;
                    case "date":
                        dbtype = DbType.Date;
                        systype = typeof(DateTime);
                        BackendConvert = new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDate);
                        break;
                    case "float4":
                        dbtype = DbType.Single;
                        systype = typeof(Single);
                        break;
                    case "float8":
                        dbtype = DbType.Double;
                        systype = typeof(Double);
                        break;
                    case "int2":
                        dbtype = DbType.Int16;
                        systype = typeof(Int16);
                        break;
                    case "int4":
                        dbtype = DbType.Int32;
                        systype = typeof(Int32);
                        break;
                    case "int8":
                        dbtype = DbType.Int64;
                        systype = typeof(Int64);
                        break;
                    case "lseg":
                        dbtype = DbType.Object;
                        systype = typeof(NpgsqlLSeg);
                        BackendConvert = new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToLSeg);
                        break;
                    case "numeric":
                        dbtype = DbType.Decimal;
                        systype = typeof(Decimal);
                        break;
                    case "path":
                        dbtype = DbType.Object;
                        systype = typeof(NpgsqlPath);
                        BackendConvert = new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPath);
                        break;
                    case "point":
                        dbtype = DbType.Object;
                        systype = typeof(PointF);
                        BackendConvert = new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPoint);
                        break;
                    case "polygon":
                        dbtype = DbType.Object;
                        systype = typeof(NpgsqlPolygon);
                        BackendConvert = new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPolygon);
                        break;
                    case "time":
                    case "timetz":
                        dbtype = DbType.Time;
                        systype = typeof(DateTime);
                        BackendConvert = new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime);
                        break;
                    case "timestamp":
                    case "timestamptz":
                        dbtype = DbType.DateTime;
                        systype = typeof(DateTime);
                        BackendConvert = new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime);
                        break;
                    default:
                        // Unsupported types will be returned as String.
                        // Well-known string types such as char and varchar will be caught here also.
                        dbtype = DbType.String;
                        systype = typeof(String);
                        break;
                    }

                    // We don't have any types mapped yet, so all fields in this result set are represented
                    // as String.
                    // The mapping will index this type on OID and Name.
                    oidToNameMapping.AddType(Convert.ToInt32(dr[0]), (String)dr[1], dbtype, systype, BackendConvert, NativeConvert);
                }

                // Add this mapping to the per-server-version cache so we don't have to
                // do these expensive queries on every connection startup.
                _oidToNameMappings.Add(conn.ServerVersion, oidToNameMapping);

                return oidToNameMapping;
            }


        }



        internal static Byte[] ConvertByteAToByteArray(String byteA)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertByteAToByteArray");
            Int32 octalValue = 0;
            Int32 byteAPosition = 0;

            Int32 byteAStringLength = byteA.Length;

            MemoryStream ms = new MemoryStream();

            while (byteAPosition < byteAStringLength)
            {


                // The IsDigit is necessary in case we receive a \ as the octal value and not
                // as the indicator of a following octal value in decimal format.
                // i.e.: \201\301P\A
                if (byteA[byteAPosition] == '\\')

                    if (byteAPosition + 1 == byteAStringLength)
                    {
                        octalValue = '\\';
                        byteAPosition++;
                    }
                    else if (Char.IsDigit(byteA[byteAPosition + 1]))
                    {
                        octalValue = (Byte.Parse(byteA[byteAPosition + 1].ToString()) << 6);
                        octalValue |= (Byte.Parse(byteA[byteAPosition + 2].ToString()) << 3);
                        octalValue |= Byte.Parse(byteA[byteAPosition + 3].ToString());
                        byteAPosition += 4;

                    }
                    else
                    {
                        octalValue = '\\';
                        byteAPosition += 2;
                    }


                else
                {
                    octalValue = (Byte)byteA[byteAPosition];
                    byteAPosition++;
                }


                ms.WriteByte((Byte)octalValue);

            }

            return ms.ToArray();


        }

        private static String ConvertByteArrayToBytea(Byte[] byteArray)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertByteArrayToBytea");
            int len = byteArray.Length;
            char[] res = new char[len * 5];
            for (int i=0, o=0; i<len; ++i, o += 5)
            {
                byte item = byteArray[i];
                res[o] = res[o + 1] = '\\';
                res[o + 2] = (char)('0' + (7 & (item >> 6)));
                res[o + 3] = (char)('0' + (7 & (item >> 3)));
                res[o + 4] = (char)('0' + (7 & item));
            }
            return new String(res);

        }
    }

    /// <summary>
    /// Delegate called to convert the given backend data to its native representation.
    /// </summary>
    internal delegate Object ConvertBackendToNativeHandler(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier);
    /// <summary>
    /// Delegate called to convert the given native data to its backand representation.
    /// </summary>
    internal delegate String ConvertNativeToBackendHandler(NpgsqlTypeInfo TypeInfo, Object NativeData);

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a backend field representation to a native object,
    /// and to convert a native object to its backend representation.
    /// </summary>
    internal class NpgsqlTypeInfo
    {
        private event ConvertBackendToNativeHandler _ConvertBackendToNative;
        private event ConvertNativeToBackendHandler _ConvertNativeToBackend;

        private Int32            _OID;
        private String           _Name;
        private DbType           _DBType;
        private Type             _Type;

        /// <summary>
        /// Construct a new NpgsqlTypeInfo with the given attributes and conversion handlers.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="DBType">DbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public NpgsqlTypeInfo(Int32 OID, String Name, DbType DBType, Type Type,
                              ConvertBackendToNativeHandler
                              ConvertBackendToNative, ConvertNativeToBackendHandler ConvertNativeToBackend)
        {
            _OID = OID;
            _Name = Name;
            _DBType = DBType;
            _Type = Type;
            _ConvertBackendToNative = ConvertBackendToNative;
            _ConvertNativeToBackend = ConvertNativeToBackend;
        }

        /// <summary>
        /// Type OID provided by the backend server.
        /// </summary>
        public Int32 OID
        { get { return _OID; } }

        /// <summary>
        /// Type name provided by the backend server.
        /// </summary>
        public String Name
        { get { return _Name; } }

        /// <summary>
        /// DbType.
        /// </summary>
        public DbType DBType
        { get { return _DBType; } }

        /// <summary>
        /// System type to convert fields of this type to.
        /// </summary>
        public Type Type
        { get { return _Type; } }

        /// <summary>
        /// Perform a data conversion from a backend representation to 
        /// a native object.
        /// </summary>
        /// <param name="BackendData">Data sent from the backend.</param>
        /// <param name="TypeModifier">Type modifier field sent from the backend.</param>
        public Object ConvertToNative(String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            if (_ConvertBackendToNative != null) {
                return _ConvertBackendToNative(this, BackendData, TypeSize, TypeModifier);
            } else {
                try {
                    return Convert.ChangeType(BackendData, Type, CultureInfo.InvariantCulture);
                } catch {
                    return BackendData;
                }
            }
        }

        /// <summary>
        /// Perform a data conversion from a native object to
        /// a backend representation.
        /// </summary>
        /// <param name="NativeData">Native .NET object to be converted.</param>
        public String ConvertToBackend(Object NativeData)
        {
            if (_ConvertNativeToBackend != null) {
                return _ConvertNativeToBackend(this, NativeData);
            } else {
                return (String)Convert.ChangeType(NativeData, typeof(String), CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// Provide mapping between type OID, type name, and a NpgsqlTypeInfo object that represents it.
    /// </summary>
    internal class NpgsqlTypeMapping
    {
        private Hashtable       OIDIndex;
        private Hashtable       NameIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public NpgsqlTypeMapping()
        {
            OIDIndex = new Hashtable();
            NameIndex = new Hashtable();
        }

        /// <summary>
        /// Copy constuctor.
        /// </summary>
        private NpgsqlTypeMapping(NpgsqlTypeMapping Other)
        {
            OIDIndex = (Hashtable)Other.OIDIndex.Clone();
            NameIndex = (Hashtable)Other.NameIndex.Clone();
        }

        /// <summary>
        /// Add the given NpgsqlTypeInfo to this mapping.
        /// </summary>
        public void AddType(NpgsqlTypeInfo T)
        {
            if (OIDIndex.Contains(T.OID)) {
                throw new Exception("Type already mapped");
            }

            OIDIndex[T.OID] = T;
            NameIndex[T.Name] = T;
        }

        /// <summary>
        /// Add a new NpgsqlTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="DBType">DbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public void AddType(Int32 OID, String Name, DbType DBType, Type Type,
                            ConvertBackendToNativeHandler BackendConvert,
                            ConvertNativeToBackendHandler NativeConvert)
        {
            AddType(new NpgsqlTypeInfo(OID, Name, DBType, Type, BackendConvert, NativeConvert));
        }

        /// <summary>
        /// Retrieve the NpgsqlTypeInfo with the given backend type OID, or null if none found.
        /// </summary>
        public NpgsqlTypeInfo this [Int32 OID]
        {
            get
            {
                return (NpgsqlTypeInfo)OIDIndex[OID];
            }
        }

        /// <summary>
        /// Retrieve the NpgsqlTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public NpgsqlTypeInfo this [String Name]
        {
            get
            {
                return (NpgsqlTypeInfo)NameIndex[Name];
            }
        }

        /// <summary>
        /// Make a shallow copy of this type mapping.
        /// </summary>
        public NpgsqlTypeMapping Clone()
        {
            return new NpgsqlTypeMapping(this);
        }

        /// <summary>
        /// Determine if a NpgsqlTypeInfo with the given backend type OID exists in this mapping.
        /// </summary>
        public Boolean ContainsOID(Int32 OID)
        {
            return OIDIndex.ContainsKey(OID);
        }

        /// <summary>
        /// Determine if a NpgsqlTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsName(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }
    }

    /// <summary>
    /// Provide event handlers to convert all native supported data types from their backend
    /// text representation to a .NET object.
    /// </summary>
    internal abstract class BasicBackendToNativeTypeConverter
    {
        private static readonly String[] DateFormats = new String[] 
        {
          "yyyy-MM-dd",
        };

        private static readonly String[] TimeFormats = new String[]
        {
          "HH:mm:ss.ffffff",
          "HH:mm:ss.fffff",	
          "HH:mm:ss.ffff",
          "HH:mm:ss.fff",
          "HH:mm:ss.ff",
          "HH:mm:ss.f",
          "HH:mm:ss",
          "HH:mm:ss.ffffffzz",
          "HH:mm:ss.fffffzz",	
          "HH:mm:ss.ffffzz",
          "HH:mm:ss.fffzz",
          "HH:mm:ss.ffzz",
          "HH:mm:ss.fzz",
          "HH:mm:sszz"
        };

        private static readonly String[] DateTimeFormats = new String[] 
        {
          "yyyy-MM-dd HH:mm:ss.ffffff",
          "yyyy-MM-dd HH:mm:ss.fffff",	
          "yyyy-MM-dd HH:mm:ss.ffff",
          "yyyy-MM-dd HH:mm:ss.fff",
          "yyyy-MM-dd HH:mm:ss.ff",
          "yyyy-MM-dd HH:mm:ss.f",
          "yyyy-MM-dd HH:mm:ss",
          "yyyy-MM-dd HH:mm:ss.ffffffzz",
          "yyyy-MM-dd HH:mm:ss.fffffzz",	
          "yyyy-MM-dd HH:mm:ss.ffffzz",
          "yyyy-MM-dd HH:mm:ss.fffzz",
          "yyyy-MM-dd HH:mm:ss.ffzz",
          "yyyy-MM-dd HH:mm:ss.fzz",
          "yyyy-MM-dd HH:mm:sszz"
        };

        /// <summary>
        /// Binary data.
        /// </summary>
        internal static Object ToBinary(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return NpgsqlTypesHelper.ConvertByteAToByteArray(BackendData);
        }

        /// <summary>
        /// Convert a postgresql boolean to a System.Boolean.
        /// </summary>
        internal static Object ToBoolean(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return (BackendData.ToLower() == "t" ? true : false);
        }

        /// <summary>
        /// Convert a postgresql datetime to a System.DateTime.
        /// </summary>
        internal static Object ToDateTime(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // Get the date time parsed in all expected formats for timestamp.
            return DateTime.ParseExact(BackendData,
                                        DateTimeFormats,
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
        }

        /// <summary>
        /// Convert a postgresql date to a System.DateTime.
        /// </summary>
        internal static Object ToDate(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return DateTime.ParseExact(BackendData,
                                        DateFormats,
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.AllowWhiteSpaces);
        }

        /// <summary>
        /// Convert a postgresql time to a System.DateTime.
        /// </summary>
        internal static Object ToTime(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return DateTime.ParseExact(BackendData,
                                        TimeFormats,
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
        }
    }

    /// <summary>
    /// Provide event handlers to convert extended native supported data types from their backend
    /// text representation to a .NET object.
    /// </summary>
    internal abstract class ExtendedBackendToNativeTypeConverter
    {
        /// <summary>
        /// Convert a postgresql point to a System.PointF.
        /// </summary>
        internal static Object ToPoint(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new PointF(100,250);
        }

        /// <summary>
        /// Convert a postgresql point to a System.RectangleF.
        /// </summary>
        internal static Object ToRectangle(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new RectangleF(100,250,20,40);
        }

        /// <summary>
        /// LDeg.
        /// </summary>
        internal static Object ToLSeg(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlLSeg(new PointF(10,20), new PointF(30,40));
        }

        /// <summary>
        /// Path.
        /// </summary>
        internal static Object ToPath(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlPath(new PointF[] { new PointF(10,20), new PointF(30,40), new PointF(50,60) } );
        }

        /// <summary>
        /// Polygon.
        /// </summary>
        internal static Object ToPolygon(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlPolygon(new PointF[] { new PointF(10,20), new PointF(30,40), new PointF(50,60) } );
        }

        /// <summary>
        /// Circle.
        /// </summary>
        internal static Object ToCircle(NpgsqlTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlCircle(new PointF(10,20), 100);
        }
    }
}
