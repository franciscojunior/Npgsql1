
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
        private static Hashtable BackendTypeMappingCache = new Hashtable();
        private static NpgsqlNativeTypeMapping NativeTypeMapping = new NpgsqlNativeTypeMapping();


        public static String GetDefaultTypeInfo(DbType dbType)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBackendTypeNameFromDbType");

            if (NativeTypeMapping.Count == 0) {
                NativeTypeMapping.AddType("text", DbType.String, typeof(String),
                null);

                NativeTypeMapping.AddType("text", DbType.StringFixedLength, typeof(String),
                null);

                NativeTypeMapping.AddType("text", DbType.AnsiString, typeof(String),
                null);

                NativeTypeMapping.AddType("text", DbType.AnsiStringFixedLength, typeof(String),
                null);

                NativeTypeMapping.AddType("bytea", DbType.Binary, typeof(byte[]),
                null);

                NativeTypeMapping.AddType("bool", DbType.Boolean, typeof(Boolean),
                null);

                NativeTypeMapping.AddType("int2", DbType.Int16, typeof(Int16),
                null);

                NativeTypeMapping.AddType("int4", DbType.Int32, typeof(Int32),
                null);

                NativeTypeMapping.AddType("int8", DbType.Int64, typeof(Int64),
                null);

                NativeTypeMapping.AddType("float4", DbType.Single, typeof(Single),
                null);

                NativeTypeMapping.AddType("float8", DbType.Double, typeof(Double),
                null);

                NativeTypeMapping.AddType("numeric", DbType.Decimal, typeof(Decimal),
                null);

                NativeTypeMapping.AddType("currency", DbType.Currency, typeof(Decimal),
                null);

                NativeTypeMapping.AddType("date", DbType.Date, typeof(DateTime),
                null);

                NativeTypeMapping.AddType("time", DbType.Time, typeof(DateTime),
                null);

                NativeTypeMapping.AddType("timestamp", DbType.DateTime, typeof(DateTime),
                null);
            }

            NpgsqlNativeTypeInfo TI = NativeTypeMapping[dbType];

            if (TI == null) {
                throw new InvalidCastException(String.Format(resman.GetString("Exception_TypeNotSupported"), dbType));
            }

            return TI.Name;
        }

        public static Object ConvertBackendBytesToSystemType(NpgsqlBackendTypeInfo TypeInfo, Byte[] data, Encoding encoding, Int32 fieldValueSize, Int32 typeModifier)
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
        public static Object ConvertBackendStringToSystemType(NpgsqlBackendTypeInfo TypeInfo, String data, Int16 typeSize, Int32 typeModifier)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ConvertBackendStringToSystemType");

            if (TypeInfo != null) {
                return TypeInfo.ConvertToNative(data, typeSize, typeModifier);
            } else {
                return data;
            }
        }

        ///<summary>
        /// This method creates (or retrieves from cache) a mapping between type and OID 
        /// of all natively supported postgresql data types.
        /// This is needed as from one version to another, this mapping can be changed and
        /// so we avoid hardcoding them.
        /// </summary>
        /// <returns>NpgsqlTypeMapping containing all known data types.  The mapping must be
        /// cloned before it is modified because it is cached; changes made by one connection may
        /// effect another connection.</returns>
        public static NpgsqlBackendTypeMapping CreateAndLoadInitialTypesMapping(NpgsqlConnector conn)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "LoadTypesMapping");

            // [TODO] Verify another way to get higher concurrency.
            lock(typeof(NpgsqlTypesHelper))
            {
                // Check the cache for an initial types map.
                NpgsqlBackendTypeMapping oidToNameMapping = (NpgsqlBackendTypeMapping) BackendTypeMappingCache[conn.ServerVersion];

                if (oidToNameMapping != null)
                {
                    return oidToNameMapping;
                }

                // Not in cache, create a new one.
                oidToNameMapping = new NpgsqlBackendTypeMapping();

                // Create a list of all natively supported postgresql data types.
                NpgsqlBackendTypeInfo[] TypeInfoList = new NpgsqlBackendTypeInfo[]
                {
                    new NpgsqlBackendTypeInfo(0, "unknown", DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "char", DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "bpchar", DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "varchar", DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "text", DbType.String, typeof(String),
                        null),

                    new NpgsqlBackendTypeInfo(0, "bytea", DbType.Binary, typeof(Byte[]),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBinary)),


                    new NpgsqlBackendTypeInfo(0, "bool", DbType.Boolean, typeof(Boolean),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToBoolean)),


                    new NpgsqlBackendTypeInfo(0, "int2", DbType.Int16, typeof(Int16),
                        null),

                    new NpgsqlBackendTypeInfo(0, "int4", DbType.Int32, typeof(Int32),
                        null),

                    new NpgsqlBackendTypeInfo(0, "int8", DbType.Int64, typeof(Int64),
                        null),

                    new NpgsqlBackendTypeInfo(0, "oid", DbType.Int32, typeof(Int32),
                        null),


                    new NpgsqlBackendTypeInfo(0, "float4", DbType.Single, typeof(Single),
                        null),

                    new NpgsqlBackendTypeInfo(0, "float8", DbType.Double, typeof(Double),
                        null),

                    new NpgsqlBackendTypeInfo(0, "numeric", DbType.Decimal, typeof(Decimal),
                        null),

                    new NpgsqlBackendTypeInfo(0, "money", DbType.Currency, typeof(Decimal),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToMoney)),


                    new NpgsqlBackendTypeInfo(0, "date", DbType.Date, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDate)),

                    new NpgsqlBackendTypeInfo(0, "time", DbType.Time, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime)),

                    new NpgsqlBackendTypeInfo(0, "timetz", DbType.Time, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToTime)),

                    new NpgsqlBackendTypeInfo(0, "timestamp", DbType.DateTime, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime)),

                    new NpgsqlBackendTypeInfo(0, "timestamptz", DbType.DateTime, typeof(DateTime),
                        new ConvertBackendToNativeHandler(BasicBackendToNativeTypeConverter.ToDateTime)),


                    new NpgsqlBackendTypeInfo(0, "point", DbType.Object, typeof(PointF),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPoint)),

                    new NpgsqlBackendTypeInfo(0, "lseg", DbType.Object, typeof(NpgsqlLSeg),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToLSeg)),

                    new NpgsqlBackendTypeInfo(0, "path", DbType.Object, typeof(NpgsqlPath),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPath)),

                    new NpgsqlBackendTypeInfo(0, "box", DbType.Object, typeof(RectangleF),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToRectangle)),

                    new NpgsqlBackendTypeInfo(0, "circle", DbType.Object, typeof(NpgsqlCircle),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToCircle)),

                    new NpgsqlBackendTypeInfo(0, "polygon", DbType.Object, typeof(NpgsqlPolygon),
                        new ConvertBackendToNativeHandler(ExtendedBackendToNativeTypeConverter.ToPolygon)),
                };

                // Attempt to map each type info in the list to an OID on the backend and
                // add each mapped type to the new type mapping object.
                LoadTypesMappings(conn, oidToNameMapping, TypeInfoList);
/*
                // Add all the DBType default handlers
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.AnsiString, "text");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.AnsiStringFixedLength, "text");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.String, "text");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.StringFixedLength, "text");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Binary, "bytea");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Boolean, "bool");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.SByte, "int2");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Currency, "money");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Date, "date");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Time, "time");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.DateTime, "timestamp");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Decimal, "numeric");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Single, "float4");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Double, "float8");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Int16, "int2");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Int32, "int4");
                oidToNameMapping.SetDefaultDBTypeHandler(DbType.Int64, "int8");

                // Add all the Type default handlers
                // (TODO - implement more of these!!!)
                oidToNameMapping.SetDefaultTypeHandler(typeof(String), "text");
*/
                // Add this mapping to the per-server-version cache so we don't have to
                // do these expensive queries on every connection startup.
                BackendTypeMappingCache.Add(conn.ServerVersion, oidToNameMapping);

                return oidToNameMapping;
            }


        }

        /// <summary>
        /// Attempt to map types by issuing a query against pg_type.
        /// This function takes a list of NpgsqlTypeInfo and attempts to resolve the OID field
        /// of each by querying pg_type.  If the mapping is found, the type info object is
        /// updated and added to the provided NpgsqlTypeMapping object.
        /// </summary>
        /// <param name="conn">NpgsqlConnector to send query through.</param>
        /// <param name="TypeMappings">Mapping object to add types too.</param>
        /// <param name="TypeInfoList">List of types that need to have OID's mapped.</param>
        public static void LoadTypesMappings(NpgsqlConnector conn, NpgsqlBackendTypeMapping TypeMappings, IList TypeInfoList)
        {
            StringBuilder       InList = new StringBuilder();
            Hashtable           NameIndex = new Hashtable();

            foreach (NpgsqlBackendTypeInfo TypeInfo in TypeInfoList) {
                NameIndex.Add(TypeInfo.Name, TypeInfo);
                InList.AppendFormat("{0}'{1}'", ((InList.Length > 0) ? ", " : ""), TypeInfo.Name);
            }

            if (InList.Length == 0) {
                return;
            }

            NpgsqlCommand       command = new NpgsqlCommand("select oid, typname from pg_type where typname in (" + InList.ToString() + ")", conn);
            NpgsqlDataReader    dr = command.ExecuteReader();

            while (dr.Read()) {
                NpgsqlBackendTypeInfo TypeInfo = (NpgsqlBackendTypeInfo)NameIndex[dr[1].ToString()];

                TypeInfo.OID = Convert.ToInt32(dr[0]);

                TypeMappings.AddType(TypeInfo);
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
    internal delegate Object ConvertBackendToNativeHandler(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier);
    /// <summary>
    /// Delegate called to convert the given native data to its backand representation.
    /// </summary>
    internal delegate String ConvertNativeToBackendHandler(NpgsqlNativeTypeInfo TypeInfo, Object NativeData, Boolean SuppressQuoting);

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a backend field representation to a native object.
    /// </summary>
    internal class NpgsqlBackendTypeInfo
    {
        private event ConvertBackendToNativeHandler _ConvertBackendToNative;

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
        public NpgsqlBackendTypeInfo(Int32 OID, String Name, DbType DBType, Type Type,
                              ConvertBackendToNativeHandler ConvertBackendToNative)
        {
            _OID = OID;
            _Name = Name;
            _DBType = DBType;
            _Type = Type;
            _ConvertBackendToNative = ConvertBackendToNative;
        }

        /// <summary>
        /// Type OID provided by the backend server.
        /// </summary>
        public Int32 OID
        {
          get { return _OID; }
          set { _OID = value; }
        }

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
    }

    /// <summary>
    /// Represents a backend data type.
    /// This class can be called upon to convert a native object to its backend field representation,
    /// </summary>
    internal class NpgsqlNativeTypeInfo
    {
        private event ConvertNativeToBackendHandler _ConvertNativeToBackend;

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
        public NpgsqlNativeTypeInfo(String Name, DbType DBType, Type Type,
                              ConvertNativeToBackendHandler ConvertNativeToBackend)
        {
            _Name = Name;
            _DBType = DBType;
            _Type = Type;
            _ConvertNativeToBackend = ConvertNativeToBackend;
        }

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
        /// Perform a data conversion from a native object to
        /// a backend representation.
        /// </summary>
        /// <param name="NativeData">Native .NET object to be converted.</param>
        /// <param name="SuppressQuoting">Never add quotes (only applies to certain types).</param>
        public String ConvertToBackend(Object NativeData, Boolean SuppressQuoting)
        {
            if (_ConvertNativeToBackend != null) {
                return _ConvertNativeToBackend(this, NativeData, SuppressQuoting);
            } else {
                return (String)Convert.ChangeType(NativeData, typeof(String), CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// Provide mapping between type OID, type name, and a NpgsqlBackendTypeInfo object that represents it.
    /// </summary>
    internal class NpgsqlBackendTypeMapping
    {
        private Hashtable       OIDIndex;
        private Hashtable       NameIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public NpgsqlBackendTypeMapping()
        {
            OIDIndex = new Hashtable();
            NameIndex = new Hashtable();
        }

        /// <summary>
        /// Copy constuctor.
        /// </summary>
        private NpgsqlBackendTypeMapping(NpgsqlBackendTypeMapping Other)
        {
            OIDIndex = (Hashtable)Other.OIDIndex.Clone();
            NameIndex = (Hashtable)Other.NameIndex.Clone();
        }

        /// <summary>
        /// Add the given NpgsqlBackendTypeInfo to this mapping.
        /// </summary>
        public void AddType(NpgsqlBackendTypeInfo T)
        {
            if (OIDIndex.Contains(T.OID)) {
                throw new Exception("Type already mapped");
            }

            OIDIndex[T.OID] = T;
            NameIndex[T.Name] = T;
        }

        /// <summary>
        /// Add a new NpgsqlBackendTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="DBType">DbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        public void AddType(Int32 OID, String Name, DbType DBType, Type Type,
                            ConvertBackendToNativeHandler BackendConvert)
        {
            AddType(new NpgsqlBackendTypeInfo(OID, Name, DBType, Type, BackendConvert));
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        { get { return NameIndex.Count; } }

        /// <summary>
        /// Retrieve the NpgsqlBackendTypeInfo with the given backend type OID, or null if none found.
        /// </summary>
        public NpgsqlBackendTypeInfo this [Int32 OID]
        {
            get
            {
                return (NpgsqlBackendTypeInfo)OIDIndex[OID];
            }
        }

        /// <summary>
        /// Retrieve the NpgsqlBackendTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public NpgsqlBackendTypeInfo this [String Name]
        {
            get
            {
                return (NpgsqlBackendTypeInfo)NameIndex[Name];
            }
        }

        /// <summary>
        /// Make a shallow copy of this type mapping.
        /// </summary>
        public NpgsqlBackendTypeMapping Clone()
        {
            return new NpgsqlBackendTypeMapping(this);
        }

        /// <summary>
        /// Determine if a NpgsqlBackendTypeInfo with the given backend type OID exists in this mapping.
        /// </summary>
        public Boolean ContainsOID(Int32 OID)
        {
            return OIDIndex.ContainsKey(OID);
        }

        /// <summary>
        /// Determine if a NpgsqlBackendTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsName(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }
    }



    /// <summary>
    /// Provide mapping between type Type, DbType and a NpgsqlNativeTypeInfo object that represents it.
    /// </summary>
    internal class NpgsqlNativeTypeMapping
    {
        private Hashtable       NameIndex;
        private Hashtable       DBTypeIndex;

        /// <summary>
        /// Construct an empty mapping.
        /// </summary>
        public NpgsqlNativeTypeMapping()
        {
            NameIndex = new Hashtable();
            DBTypeIndex = new Hashtable();
        }

        /// <summary>
        /// Add the given NpgsqlNativeTypeInfo to this mapping.
        /// </summary>
        public void AddType(NpgsqlNativeTypeInfo T)
        {
            if (DBTypeIndex.Contains(T.DBType)) {
                throw new Exception("Type already mapped");
            }

            NameIndex[T.Name] = T;
            DBTypeIndex[T.DBType] = T;
        }

        /// <summary>
        /// Add a new NpgsqlNativeTypeInfo with the given attributes and conversion handlers to this mapping.
        /// </summary>
        /// <param name="OID">Type OID provided by the backend server.</param>
        /// <param name="Name">Type name provided by the backend server.</param>
        /// <param name="DBType">DbType</param>
        /// <param name="Type">System type to convert fields of this type to.</param>
        /// <param name="ConvertBackendToNative">Data conversion handler.</param>
        /// <param name="ConvertNativeToBackend">Data conversion handler.</param>
        public void AddType(String Name, DbType DBType, Type Type,
                            ConvertNativeToBackendHandler NativeConvert)
        {
            AddType(new NpgsqlNativeTypeInfo(Name, DBType, Type, NativeConvert));
        }

        /// <summary>
        /// Get the number of type infos held.
        /// </summary>
        public Int32 Count
        { get { return NameIndex.Count; } }

        /// <summary>
        /// Retrieve the NpgsqlNativeTypeInfo with the given backend type name, or null if none found.
        /// </summary>
        public NpgsqlNativeTypeInfo this [String Name]
        {
            get
            {
                return (NpgsqlNativeTypeInfo)NameIndex[Name];
            }
        }

        /// <summary>
        /// Retrieve the NpgsqlNativeTypeInfo with the given DbType, or null if none found.
        /// </summary>
        public NpgsqlNativeTypeInfo this [DbType DBType]
        {
            get
            {
                return (NpgsqlNativeTypeInfo)DBTypeIndex[DBType];
            }
        }

        /// <summary>
        /// Determine if a NpgsqlNativeTypeInfo with the given backend type OID exists in this mapping.
        /// </summary>
        public Boolean ContainsOID(String Name)
        {
            return NameIndex.ContainsKey(Name);
        }

        /// <summary>
        /// Determine if a NpgsqlNativeTypeInfo with the given backend type name exists in this mapping.
        /// </summary>
        public Boolean ContainsDBType(DbType DBType)
        {
            return DBTypeIndex.ContainsKey(DBType);
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
        internal static Object ToBinary(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return NpgsqlTypesHelper.ConvertByteAToByteArray(BackendData);
        }

        /// <summary>
        /// Convert a postgresql boolean to a System.Boolean.
        /// </summary>
        internal static Object ToBoolean(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return (BackendData.ToLower() == "t" ? true : false);
        }

        /// <summary>
        /// Convert a postgresql datetime to a System.DateTime.
        /// </summary>
        internal static Object ToDateTime(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
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
        internal static Object ToDate(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return DateTime.ParseExact(BackendData,
                                        DateFormats,
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.AllowWhiteSpaces);
        }

        /// <summary>
        /// Convert a postgresql time to a System.DateTime.
        /// </summary>
        internal static Object ToTime(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            return DateTime.ParseExact(BackendData,
                                        TimeFormats,
                                        DateTimeFormatInfo.InvariantInfo,
                                        DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
        }

        /// <summary>
        /// Convert a postgresql money to a System.Decimal.
        /// </summary>
        internal static Object ToMoney(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // It's a number with a $ on the beginning...
            return Convert.ToDecimal(BackendData.Substring(1, BackendData.Length - 1), CultureInfo.InvariantCulture);
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
        internal static Object ToPoint(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new PointF(100,250);
        }

        /// <summary>
        /// Convert a postgresql point to a System.RectangleF.
        /// </summary>
        internal static Object ToRectangle(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new RectangleF(100,250,20,40);
        }

        /// <summary>
        /// LDeg.
        /// </summary>
        internal static Object ToLSeg(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlLSeg(new PointF(10,20), new PointF(30,40));
        }

        /// <summary>
        /// Path.
        /// </summary>
        internal static Object ToPath(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlPath(new PointF[] { new PointF(10,20), new PointF(30,40), new PointF(50,60) } );
        }

        /// <summary>
        /// Polygon.
        /// </summary>
        internal static Object ToPolygon(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlPolygon(new PointF[] { new PointF(10,20), new PointF(30,40), new PointF(50,60) } );
        }

        /// <summary>
        /// Circle.
        /// </summary>
        internal static Object ToCircle(NpgsqlBackendTypeInfo TypeInfo, String BackendData, Int16 TypeSize, Int32 TypeModifier)
        {
            // FIXME - uh actually parse the data
            return new NpgsqlCircle(new PointF(10,20), 100);
        }
    }
}
