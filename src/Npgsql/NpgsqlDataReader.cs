
// Npgsql.NpgsqlDataReader.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
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


namespace Npgsql
{
		
	public class NpgsqlDataReader : IDataReader
	{
		
		
		
	  private NpgsqlConnection 	_connection;
		private ArrayList 				_resultsets;
	  private Int32 						_rowIndex;
		private Int32							_resultsetIndex;
		private	NpgsqlResultSet		_currentResultset;
		private DataTable					_currentResultsetSchema;
		
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlDataReader";
		
	  internal NpgsqlDataReader( ArrayList resultsets, NpgsqlConnection connection)
	  {
	    _resultsets					= resultsets;
	  	_connection 				= connection;
	  	_rowIndex						= -1;
	  	_resultsetIndex			= 0;
	  	_currentResultset 	= (NpgsqlResultSet)_resultsets[_resultsetIndex];
	  	
	  }
	  	  
	  public void Dispose()
	  {
	  	
	  }
	  public Int32 Depth 
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_Depth() ", LogLevel.Debug);
	  		return 0;
	  	}
	  }
	  
	  public Boolean IsClosed
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_IsClosed()", LogLevel.Debug);
	  		return false; 
	  	}
	  }
	  
	  public Int32 RecordsAffected 
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_RecordsAffected()", LogLevel.Debug);
	  		return -1;
	  	}
	    
	  }
	  
	  public void Close()
	  {
	    
	  }
	  
	  public Boolean NextResult()
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NextResult()", LogLevel.Debug);
	    //throw new NotImplementedException();
	  	
	  	//[FIXME] Should the currentResultset not be modified
	  	// in case there aren't any more resultsets?
	  	// SqlClient modify to a invalid resultset and throws exceptions
	  	// when trying to access any data.
	  	
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
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".Read()", LogLevel.Debug);
	    _rowIndex++;
	  	return (_rowIndex < _currentResultset.Count);
	  }
	  
	  public DataTable GetSchemaTable()
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetSchemaTable()", LogLevel.Debug);
	    //throw new NotImplementedException();
	  		  	
	  	if(_currentResultsetSchema == null)
	  		_currentResultsetSchema = GetResultsetSchema();
	  	
	  	return _currentResultsetSchema;
	  	
	  }
	  
	  
	  public Int32 FieldCount
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".get_FieldCount()", LogLevel.Debug);
	  		return _currentResultset.RowDescription.NumFields;
	  	}
	    
	  }
	  
	  public String GetName(Int32 i)
	  {
	    //throw new NotImplementedException();
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetName(Int32)", LogLevel.Debug);
	  	return _currentResultset.RowDescription[i].name;
	  }
	  
	  public String GetDataTypeName(Int32 i)
	  {
	  	throw new NotImplementedException();
	  }
	  
	  public Type GetFieldType(Int32 i)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetFieldType(Int32)", LogLevel.Debug);
	    //throw new NotImplementedException();
	  	//[FIXME] hack
	  	  	
	  	return Type.GetType(PGUtil.GetSystemTypeFromDbType(_currentResultset.RowDescription[i].type_oid));
	  }
	  
	  public Object GetValue(Int32 i)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetValue(Int32)", LogLevel.Debug);
	  	if (i < 0 || _rowIndex < 0)
	  		throw new InvalidOperationException("Cannot read data.");
	  	return ((NpgsqlAsciiRow)_currentResultset[_rowIndex])[i];
	  }
	  
	  public Int32 GetValues(Object[] values)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetValues(Object[])", LogLevel.Debug);
	  	
	  	// Only the number of elements in the array are filled.
	  	// It's also possible to pass an array with more that FieldCount elements.
	  	Int32 maxColumnIndex = (values.Length < FieldCount) ? values.Length : FieldCount;
	  	
	  	for (Int32 i = 0; i < maxColumnIndex; i++)
	  		values[i] = GetValue(i);
	  	
	  	return maxColumnIndex;
	  	
	  }
	  
	  public Int32 GetOrdinal(String name)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Object this [ Int32 i ]
	  {
	  	get
	  	{
	  		NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".this[Int32]", LogLevel.Debug);
	  		return GetValue(i);
	  	}
	  }
	  
	  public Object this [ String name ]
	  {
	  	get
	  	{
		  	throw new NotImplementedException();
	  	}
	  }
	  
	  public Boolean GetBoolean(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Byte GetByte(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int64 GetBytes(Int32 i, Int64 fieldOffset, Byte[] buffer, Int32 bufferoffset, Int32 length)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Char GetChar(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int64 GetChars(Int32 i, Int64 fieldoffset, Char[] buffer, Int32 bufferoffset, Int32 length)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Guid GetGuid(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int16 GetInt16(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int32 GetInt32(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Int64 GetInt64(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Single GetFloat(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Double GetDouble(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public String GetString(Int32 i)
	  {
	  	NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetString(Int32)", LogLevel.Debug);
	    return (String) this[i];
	  }
	  
	  public Decimal GetDecimal(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public DateTime GetDateTime(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public IDataReader GetData(Int32 i)
	  {
	    throw new NotImplementedException();
	  }
	  
	  public Boolean IsDBNull(Int32 i)
	  {
	  	//throw new NotImplementedException();
	  	
	  	return ((NpgsqlAsciiRow)_currentResultset[_rowIndex]).IsNull(i);
	  }

		private DataTable GetResultsetSchema()
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".GetResultsetSchema()", LogLevel.Debug);
			// [FIXME] For now, just support fields name.
			
			NpgsqlRowDescription rd = _currentResultset.RowDescription;
			Int16 numFields = rd.NumFields;
			DataTable result = new DataTable();
			
			result.Columns.Add("ColumnName");
			DataRow row;
			
			for (Int16 i = 0; i < numFields; i++)
			{
				row = result.NewRow();
				row["ColumnName"] = rd[i].name;
				result.Rows.Add(row);
			}
			
			return result;
			
		}
	}
}
