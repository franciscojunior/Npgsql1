
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
using Npgsql;



/// <summary>
///	This class contains helper methods for type conversion between
/// the .Net type system and postgresql.	
/// </summary>
namespace NpgsqlTypes
{
	
	/*internal struct NpgsqlTypeMapping
	{
	  public String        _backendTypeName;
	  public Type          _frameworkType;
	  public Int32         _typeOid;
	  public NpgsqlDbType  _npgsqlDbType;
	  
	  public NpgsqlTypeMapping(String backendTypeName, Type frameworkType, Int32 typeOid, NpgsqlDbType npgsqlDbType)
	  {
	    _backendTypeName = backendTypeName;
	    _frameworkType = frameworkType;
	    _typeOid = typeOid;
	    _npgsqlDbType = npgsqlDbType;
	    
	  }
	}*/
	
	
	internal class NpgsqlTypesHelper
	{
		
		private static Hashtable _oidToNameMappings = new Hashtable();
				
		
		
		public static String GetBackendTypeNameFromNpgsqlDbType(NpgsqlDbType npgsqlDbType)
		{
			switch (npgsqlDbType)
			{
				case NpgsqlDbType.Boolean:
					return "bool";
				case NpgsqlDbType.Bigint:
					return "int8";
				case NpgsqlDbType.Integer:
					return "int4";
				case NpgsqlDbType.Numeric:
					return "numeric";
				case NpgsqlDbType.Smallint:
					return "int2";
				case NpgsqlDbType.Text:
					return "text";
				case NpgsqlDbType.Timestamp:
					return "timestamp";
				default:
					throw new NpgsqlException(String.Format("Internal error. This type {0} shouldn't be allowed.", npgsqlDbType));
				
			}
		}
		
		
		public static String ConvertNpgsqlParameterToBackendStringValue(NpgsqlParameter parameter)
		{
			
			switch(parameter.NpgsqlDbType)
			{
				case NpgsqlDbType.Boolean:
				case NpgsqlDbType.Bigint:
				case NpgsqlDbType.Integer:
									
				case NpgsqlDbType.Smallint:
							
					return parameter.Value.ToString();
				
				case NpgsqlDbType.Numeric:
					return ((Decimal)parameter.Value).ToString(NumberFormatInfo.InvariantInfo);
				
				case NpgsqlDbType.Text:
				{
					NpgsqlString value = (NpgsqlString) parameter.Value;
					if (value.IsNull)
						return "Null";
					else
						// Escape all single quotes in the string.
						return "'" + parameter.Value.ToString().Replace("'", "\'") + "'";
				}
				
				case NpgsqlDbType.Timestamp:
				{
					NpgsqlDateTime value = (NpgsqlDateTime) parameter.Value;
					if (value.IsNull)
						return "Null";
					else
					return "'" + ((NpgsqlDateTime)parameter.Value).ToISOString() + "'";
				}
				default:
					// This should not happen!
					throw new NpgsqlException(String.Format("Internal error. This type {0} shouldn't be allowed.", parameter.NpgsqlDbType));
					
				
			}
			
		}
		
		
		
		/// <summary>
		/// This method is responsible to convert a given NpgsqlType to its corresponding system type.
		/// 
		/// </summary>
		public static Object ConvertNpgsqlTypeToSystemType(Hashtable oidToNameMapping, Object data, Int32 typeOid)
		{
			
			//[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
			// when connecting because we don't have yet loaded typeMapping. The switch below
			// crashes with NullPointerReference when it can't find the typeOid.
			
			if (oidToNameMapping.Count == 0)
				return (String) (NpgsqlString)data;					
			
			switch ((NpgsqlDbType)oidToNameMapping[typeOid])
			{
				case NpgsqlDbType.Boolean:
					return (Boolean)(NpgsqlBoolean)data;
				case NpgsqlDbType.Smallint:
					return (Int16)(NpgsqlInt16)data;
				case NpgsqlDbType.Integer:
					return (Int32)(NpgsqlInt32)data;
				
				case NpgsqlDbType.Bigint:
					return (Int64)(NpgsqlInt64)data;
				
				case NpgsqlDbType.Numeric:
					return (Decimal)(NpgsqlDecimal)data;
				case NpgsqlDbType.Timestamp:
					return (DateTime)(NpgsqlDateTime)data;
				case NpgsqlDbType.Text:
					return (String)(NpgsqlString)data;
				default:
						throw new NpgsqlException(String.Format("Internal error. This type {0} shouldn't be allowed.", oidToNameMapping[typeOid]));
				
				
					
			}
			
		}
		
		///<summary>
		/// This method is responsible to convert the string received from the backend
		/// to the corresponding NpgsqlType.
		/// </summary>
		/// 
		public static Object ConvertBackendStringToNpgsqlType(Hashtable oidToNameMapping, String data, Int32 typeOid, Int32 typeModifier)
		{
			//[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
			// when connecting because we don't have yet loaded typeMapping. The switch below
			// crashes with NullPointerReference when it can't find the typeOid.
			
			if (oidToNameMapping.Count == 0)
				return new NpgsqlString(data);
			
			switch ((NpgsqlDbType)oidToNameMapping[typeOid])
			{
				case NpgsqlDbType.Boolean:
					return NpgsqlBoolean.Parse(data);
				case NpgsqlDbType.Smallint:
					return NpgsqlInt16.Parse(data);
				case NpgsqlDbType.Integer:
					return NpgsqlInt32.Parse(data);
				
				case NpgsqlDbType.Bigint:
					return NpgsqlInt64.Parse(data);
				
				case NpgsqlDbType.Numeric:
					// Got this manipulation of typemodifier from jdbc driver - file AbstractJdbc1ResultSetMetaData.java.html method getColumnDisplaySize
					{ 
						typeModifier -= 4;
						//Console.WriteLine("Numeric from server: {0} digitos.digitos {1}.{2}", data, (typeModifier >> 16) & 0xffff, typeModifier & 0xffff);
						return new NpgsqlDecimal(Decimal.Parse(data, NumberFormatInfo.InvariantInfo));
					}
				
				case NpgsqlDbType.Timestamp:
					return NpgsqlDateTime.Parse(data);
				case NpgsqlDbType.Text:
					return new NpgsqlString(data);
				default:
					throw new NpgsqlException(String.Format("Internal error. This type {0} shouldn't be allowed.", oidToNameMapping[typeOid]));
				
			
			}
		}
		
		///<summary>
		/// This method gets a type oid and return the equivalent
		/// Npgsql type name.
		/// </summary>
		/// 
		
		public static String GetNpgsqlTypeNameFromTypeOidold(Hashtable oidToNameMapping, Int32 typeOid)
    {
    	// This method gets a db type identifier and return the equivalent
    	// system type name.
    	
    	
    	switch ((NpgsqlDbType)oidToNameMapping[typeOid])
			{
				case NpgsqlDbType.Boolean:
					return "NpgsqlTypes.NpgsqlBoolean";
				case NpgsqlDbType.Smallint:
					return "NpgsqlTypes.NpgsqlInt16";
				case NpgsqlDbType.Integer:
					return "NpgsqlTypes.NpgsqlInt32";
				case NpgsqlDbType.Bigint:
					return "NpgsqlTypes.NpgsqlInt64";
				case NpgsqlDbType.Numeric:
					return "NpgsqlTypes.NpgsqlDecimal";
				case NpgsqlDbType.Timestamp:
					return "NpgsqlTypes.NpgsqlDateTime";
				case NpgsqlDbType.Text:
				default:
					return "NpgsqlTypes.NpgsqlString";
			
			}
    	
    }
    
    ///<summary>
		/// This method gets a type oid and return the equivalent
		/// Npgsql type name.
		/// </summary>
		/// 
		
		public static String GetSystemTypeNameFromTypeOid(Hashtable oidToNameMapping, Int32 typeOid)
    {
    	// This method gets a db type identifier and return the equivalent
    	// system type name.
    	
    	//[TODO] Find a way to eliminate this checking. It is just used at bootstrap time
			// when connecting because we don't have yet loaded typeMapping. The switch below
			// crashes with NullPointerReference when it can't find the typeOid.
    	if (oidToNameMapping.Count == 0)
				return "System.String";
    	
    	switch ((NpgsqlDbType)oidToNameMapping[typeOid])
			{
				case NpgsqlDbType.Boolean:
					return "System.Boolean";
				case NpgsqlDbType.Smallint:
					return "System.Int16";
				case NpgsqlDbType.Integer:
					return "System.Int32";
				case NpgsqlDbType.Bigint:
					return "System.Int64";
				case NpgsqlDbType.Numeric:
					return "System.Decimal";
				case NpgsqlDbType.Timestamp:
					return "System.DateTime";
				case NpgsqlDbType.Text:
					return "System.String";
				default:
					throw new NpgsqlException(String.Format("Internal error. This type {0} shouldn't be allowed.", oidToNameMapping[typeOid]));
			
			}
    	
    }
    
		
		///<summary>
		/// This method is responsible to send query to get the oid-to-name mapping.
		/// This is needed as from one version to another, this mapping can be changed and
		/// so we avoid hardcoding them.
		/// </summary>
		public static Hashtable LoadTypesMapping(NpgsqlConnection conn)
		{
			// [TODO] Verify another way to get higher concurrency.
			lock(typeof(NpgsqlTypesHelper))
			{
				Hashtable oidToNameMapping = (Hashtable) _oidToNameMappings[conn.ServerVersion];
												
				if (oidToNameMapping != null)
				{
					//conn.OidToNameMapping = oidToNameMapping;
					return oidToNameMapping;
				}
				
				oidToNameMapping = new Hashtable();
				//conn.OidToNameMapping = oidToNameMapping;
				
				// Bootstrap value as the datareader below will use ConvertStringToNpgsqlType above.
				//oidToNameMapping.Add(26, "oid");
								
				NpgsqlCommand command = new NpgsqlCommand("select oid, typname from pg_type", conn);
				
				NpgsqlDataReader dr = command.ExecuteReader();
				
				// Data was read. Clear the mapping from previous bootstrap value so we don't get
				// exceptions trying to add duplicate key.
				// oidToNameMapping.Clear();
				
				while (dr.Read())
				{
					// Add the key as a Int32 value so the switch in ConvertStringToNpgsqlType can use it
					// in the search. If don't, the key is added as string and the switch doesn't work.
					
					NpgsqlDbType type;
					String typeName = (String) dr[1];
										
					switch (typeName)
					{
						case "bool":
							type = NpgsqlDbType.Boolean;
							break;
						case "int2":
							type = NpgsqlDbType.Smallint;
							break;
						case "int4":
							type = NpgsqlDbType.Integer;
							break;
						case "int8":
							type = NpgsqlDbType.Bigint;
							break;
						case "numeric":
							type = NpgsqlDbType.Numeric;
							break;
						case "timestamp":
							type = NpgsqlDbType.Timestamp;
							break;
						default:
							type = NpgsqlDbType.Text; // Default npgsqltype of the oid. Unsupported types will be returned as NpgsqlString.
							break;
					}
										
					
					oidToNameMapping.Add(Int32.Parse((String)dr[0]), type);
				}
				
				_oidToNameMappings.Add(conn.ServerVersion, oidToNameMapping);
				return oidToNameMapping;
			}
			
			
		}
		
	}
	
}