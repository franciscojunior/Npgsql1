
// Npgsql.NpgsqlDataReader.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
using System.Data;
using System.Collections;
using NpgsqlTypes;


namespace Npgsql
{

    public class NpgsqlDataReader : IDataReader, IEnumerable
    {



        private NpgsqlConnection 	_connection;
        private ArrayList 			_resultsets;
        private ArrayList			_responses;
        private Int32 				_rowIndex;
        private Int32				_resultsetIndex;
        private	NpgsqlResultSet		_currentResultset;
        private DataTable			_currentResultsetSchema;
        private CommandBehavior     _behavior;
        private Boolean             _isClosed;
        


        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlDataReader";

        internal NpgsqlDataReader( ArrayList resultsets, ArrayList responses, NpgsqlConnection connection, CommandBehavior behavior)
        {
            _resultsets = resultsets;
            _responses = responses;
            _connection = connection;
            _rowIndex = -1;
            _resultsetIndex = 0;
            
            if (_resultsets.Count > 0)
                _currentResultset = (NpgsqlResultSet)_resultsets[_resultsetIndex];
                
            _behavior = behavior;
            _isClosed = false;

        }

        private Boolean CanRead()
        {
            //NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CanRead");
            /*if (_currentResultset == null)
            	return false;*/
            return ((_currentResultset != null) && 
                    (_currentResultset.Count > 0) && 
                    (_rowIndex < _currentResultset.Count));

        }

        private void CheckCanRead()
        {
            if (!CanRead())
                throw new InvalidOperationException("Cannot read data");
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        /// <summary>
        /// Releases the resources used by the <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>.
        /// </summary>
        protected void Dispose (bool disposing)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose");
            if (disposing)
            {
                this.Close();
            }
        }
        public Int32 Depth
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Depth");
                return 0;
            }
        }

        public Boolean IsClosed
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsClosed");
                return _isClosed;
            }
        }

        public Int32 RecordsAffected
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "RecordsAffected");

            
                if (CanRead())
                    return -1;

                String[] _returnStringTokens = ((String)_responses[_resultsetIndex]).Split(null);	// whitespace separator.

                return Int32.Parse(_returnStringTokens[_returnStringTokens.Length - 1]);
            }

        }

        public void Close()
        {
           if ((_behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
            {
                _connection.Close();
                _isClosed = true;
            }

        }

        public Boolean NextResult()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "NextResult");
            
            if((_resultsetIndex + 1) < _resultsets.Count)
            {
                _resultsetIndex++;
                _rowIndex = -1;
                _currentResultset = (NpgsqlResultSet)_resultsets[_resultsetIndex];
                return true;
            }
            else
                return false;

        }

        public Boolean Read()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Read");

            _rowIndex++;
            
            if (!CanRead())
                return false;
            else
                return true;

        }

        public DataTable GetSchemaTable()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetSchemaTable");

            if(_currentResultsetSchema == null)
                _currentResultsetSchema = GetResultsetSchema();

            return _currentResultsetSchema;

        }


        public Int32 FieldCount
        {
            get
            {

                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "FieldCount");
                
                if (_currentResultset == null) //Executed a non return rows query.
                    return -1;
                else
                    return _currentResultset.RowDescription.NumFields;


            }

        }

        public String GetName(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetName");

            if (_currentResultset == null)
                return String.Empty;
            else
                return _currentResultset.RowDescription[i].name;
        }

        public String GetDataTypeName(Int32 i)
        {
            // FIXME: have a type name instead of the oid
            if (_currentResultset == null)
                return String.Empty;
            else
                return (_currentResultset.RowDescription[i].type_oid).ToString();
        }

        public Type GetFieldType(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFieldType");


            if (_currentResultset == null)
                return null;
            else
                return NpgsqlTypesHelper.GetSystemTypeFromTypeOid(_connection.OidToNameMapping, _currentResultset.RowDescription[i].type_oid);
        }

        public Object GetValue(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetValue");

            CheckCanRead();

            if (i < 0)
                throw new InvalidOperationException("Cannot read data. Column less than 0 specified.");
            if (_rowIndex < 0)
                throw new InvalidOperationException("Cannot read data. DataReader not initialized. Maybe you forgot to call Read()?");
            return ((NpgsqlAsciiRow)_currentResultset[_rowIndex])[i];


        }


        public Int32 GetValues(Object[] values)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetValues");

            CheckCanRead();

            // Only the number of elements in the array are filled.
            // It's also possible to pass an array with more that FieldCount elements.
            Int32 maxColumnIndex = (values.Length < FieldCount) ? values.Length : FieldCount;

            for (Int32 i = 0; i < maxColumnIndex; i++)
                values[i] = GetValue(i);

            return maxColumnIndex;

        }

        public Int32 GetOrdinal(String name)
        {
            CheckCanRead();
            return _currentResultset.RowDescription.FieldIndex(name);
        }

        public Object this [ Int32 i ]
        {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, i);
                return GetValue(i);
            }
        }

        public Object this [ String name ]
        {
            get
            {
                //throw new NotImplementedException();
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, name);
                return GetValue(_currentResultset.RowDescription.FieldIndex(name));
            }
        }

        public Boolean GetBoolean(Int32 i)
        {
            // Should this be done using the GetValue directly and not by converting to String
            // and parsing from there?
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetBoolean");


            return (Boolean) GetValue(i);

        }

        public Byte GetByte(Int32 i)
        {
            throw new NotImplementedException();
        }

        public Int64 GetBytes(Int32 i, Int64 fieldOffset, Byte[] buffer, Int32 bufferoffset, Int32 length)
        {

            Byte[] result;

            result = (Byte[]) GetValue(i);

            // [TODO] Implement blob support.
            if (buffer != null)
            {
                result.CopyTo(buffer, 0);
            }

            return result.Length;

        }

        public Char GetChar(Int32 i)
        {
            throw new NotImplementedException();
        }

        public Int64 GetChars(Int32 i, Int64 fieldoffset, Char[] buffer, Int32 bufferoffset, Int32 length)
        {
            String		str;

            str = GetString(i);
            if (buffer == null)
                return str.Length;

            str.ToCharArray(bufferoffset, length).CopyTo(buffer, 0);
            return buffer.GetLength(0);
        }

        public Guid GetGuid(Int32 i)
        {
            throw new NotImplementedException();
        }

        public Int16 GetInt16(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt16");

            return (Int16) GetValue(i);

        }


        public Int32 GetInt32(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt32");

            return (Int32) GetValue(i);

        }


        public Int64 GetInt64(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetInt64");

            return (Int64) GetValue(i);
        }

        public Single GetFloat(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetFloat");
            
            return (Single) GetValue(i);
        }

        public Double GetDouble(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDouble");
            
            return (Double) GetValue(i);
        }

        public String GetString(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetString");

            return (String) GetValue(i);
        }

        public Decimal GetDecimal(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDecimal");

            return (Decimal) GetValue(i);
        }

        public DateTime GetDateTime(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetDateTime");

            return (DateTime) GetValue(i);
        }

        public IDataReader GetData(Int32 i)
        {
            throw new NotImplementedException();
        }

        public Boolean IsDBNull(Int32 i)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IsDBNull");

            CheckCanRead();

            return ((NpgsqlAsciiRow)_currentResultset[_rowIndex]).IsNull(i);
        }

        private DataTable GetResultsetSchema()
        {

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetResultsetSchema");
            DataTable result = null;

            NpgsqlRowDescription rd = _currentResultset.RowDescription;
            Int16 numFields = rd.NumFields;
            if(numFields > 0)
            {
                result = new DataTable("SchemaTable");

                result.Columns.Add ("ColumnName", typeof (string));
                result.Columns.Add ("ColumnOrdinal", typeof (int));
                result.Columns.Add ("ColumnSize", typeof (int));
                result.Columns.Add ("NumericPrecision", typeof (int));
                result.Columns.Add ("NumericScale", typeof (int));
                result.Columns.Add ("IsUnique", typeof (bool));
                result.Columns.Add ("IsKey", typeof (bool));
                DataColumn dc = result.Columns["IsKey"];
                dc.AllowDBNull = true; // IsKey can have a DBNull
                result.Columns.Add ("BaseCatalogName", typeof (string));
                result.Columns.Add ("BaseColumnName", typeof (string));
                result.Columns.Add ("BaseSchemaName", typeof (string));
                result.Columns.Add ("BaseTableName", typeof (string));
                result.Columns.Add ("DataType", typeof(Type));
                result.Columns.Add ("AllowDBNull", typeof (bool));
                result.Columns.Add ("ProviderType", typeof (int));
                result.Columns.Add ("IsAliased", typeof (bool));
                result.Columns.Add ("IsExpression", typeof (bool));
                result.Columns.Add ("IsIdentity", typeof (bool));
                result.Columns.Add ("IsAutoIncrement", typeof (bool));
                result.Columns.Add ("IsRowVersion", typeof (bool));
                result.Columns.Add ("IsHidden", typeof (bool));
                result.Columns.Add ("IsLong", typeof (bool));
                result.Columns.Add ("IsReadOnly", typeof (bool));

                DataRow row;

                for (Int16 i = 0; i < numFields; i++)
                {
                    row = result.NewRow();

                    row["ColumnName"] = GetName(i);
                    row["ColumnOrdinal"] = i + 1;
                    row["ColumnSize"] = (int) rd[i].type_size;
                    row["NumericPrecision"] = 0;
                    row["NumericScale"] = 0;
                    row["IsUnique"] = false;
                    row["IsKey"] = DBNull.Value;
                    row["BaseCatalogName"] = "";
                    row["BaseColumnName"] = GetName(i);
                    row["BaseSchemaName"] = "";
                    row["BaseTableName"] = "";
                    row["DataType"] = GetFieldType(i);
                    row["AllowDBNull"] = false;
                    row["ProviderType"] = (int) rd[i].type_oid;
                    row["IsAliased"] = false;
                    row["IsExpression"] = false;
                    row["IsIdentity"] = false;
                    row["IsAutoIncrement"] = false;
                    row["IsRowVersion"] = false;
                    row["IsHidden"] = false;
                    row["IsLong"] = false;
                    row["IsReadOnly"] = false;

                    result.Rows.Add(row);
                }
            }

            return result;

        }



        IEnumerator IEnumerable.GetEnumerator ()
        {
            return new System.Data.Common.DbEnumerator (this);
        }
    }
}
