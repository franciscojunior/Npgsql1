
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
					throw new NpgsqlException(String.Format("The NpgsqlDbType {0} isn't supported yet", npgsqlDbType));
				
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
					// Escape all single quotes in the string.
					return "'" + parameter.Value.ToString().Replace("'", "\'") + "'";
				
				case NpgsqlDbType.Timestamp:
					return "'" + ((DateTime)parameter.Value).ToString("u") + "'";
				
				default:
					// This should not happen!
					throw new NpgsqlException (String.Format("Parameter type {0} not supported", parameter.NpgsqlDbType));
					
				
			}
			
		}
		
		
		
		/// <summary>
		/// This method is responsible to convert a given NpgsqlType to its corresponding system type.
		/// 
		/// </summary>
		public static Object ConvertNpgsqlTypeToSystemType(Hashtable oidToNameMapping, Object data, Int32 typeOid)
		{
								
			switch ((String)oidToNameMapping[typeOid])
			{
				case "bool":
					return (Boolean)(NpgsqlBoolean)data;
				case "int2":
					return (Int16)(NpgsqlInt16)data;
				case "int4":
				case "oid":
				  return (Int32)(NpgsqlInt32)data;
				
				case "int8":
					return (Int64)(NpgsqlInt64)data;
				
				case "numeric":
					return (Decimal)(NpgsqlDecimal)data;
				case "timestamp":
					return (DateTime)(NpgsqlDateTime)data;
				case "text":
				default:
					return (String)(NpgsqlString)data;
				
				
					
			}
			
		}
		
		///<summary>
		/// This method is responsible to convert the string received from the backend
		/// to the corresponding NpgsqlType.
		/// </summary>
		/// 
		public static Object ConvertBackendStringToNpgsqlType(Hashtable oidToNameMapping, String data, Int32 typeOid, Int32 typeModifier)
		{
			
			switch ((String)oidToNameMapping[typeOid])
			{
				case "bool":
					return NpgsqlBoolean.Parse(data);
				case "int2":
					return NpgsqlInt16.Parse(data);
				case "int4":
				case "oid":
				  return NpgsqlInt32.Parse(data);
				
				case "int8":
					return NpgsqlInt64.Parse(data);
				
				case "numeric":
					// Got this manipulation of typemodifier from jdbc driver - file AbstractJdbc1ResultSetMetaData.java.html method getColumnDisplaySize
					{ typeModifier -= 4;
						//Console.WriteLine("Numeric from server: {0} digitos.digitos {1}.{2}", data, (typeModifier >> 16) & 0xffff, typeModifier & 0xffff);
						return new NpgsqlDecimal(Decimal.Parse(data, NumberFormatInfo.InvariantInfo));
					}
				
				case "timestamp":
					return NpgsqlDateTime.Parse(data);
				case "text":
				default:
					return new NpgsqlString(data);
				
			
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
    	
    	switch ((String)oidToNameMapping[typeOid])
			{
				case "bool":
					return "NpgsqlTypes.NpgsqlBoolean";
				case "int2":
					return "NpgsqlTypes.NpgsqlInt16";
				case "int4":
				case "oid":
				  return "NpgsqlTypes.NpgsqlInt32";
				case "int8":
					return "NpgsqlTypes.NpgsqlInt64";
				case "numeric":
					return "NpgsqlTypes.NpgsqlDecimal";
				case "timestamp":
					return "NpgsqlTypes.NpgsqlDateTime";
				case "text":
				default:
					return "NpgsqlTypes.NpgsqlString";
			
			}
    	
    }
    
    ///<summary>
		/// This method gets a type oid and return the equivalent
		/// Npgsql type name.
		/// </summary>
		/// 
		
		public static String GetNpgsqlTypeNameFromTypeOid(Hashtable oidToNameMapping, Int32 typeOid)
    {
    	// This method gets a db type identifier and return the equivalent
    	// system type name.
    	
    	switch ((String)oidToNameMapping[typeOid])
			{
				case "bool":
					return "System.Boolean";
				case "int2":
					return "System.Int16";
				case "int4":
				case "oid":
				  return "System.Int32";
				case "int8":
					return "System.Int64";
				case "numeric":
					return "System.Decimal";
				case "timestamp":
					return "System.DateTime";
				case "text":
				default:
					return "System.String";
			
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
				oidToNameMapping.Clear();
				
				while (dr.Read())
				{
					// Add the key as a Int32 value so the switch in ConvertStringToNpgsqlType can use it
					// in the search. If don't, the key is added as string and the switch doesn't work.
					oidToNameMapping.Add(Int32.Parse((String)dr[0]), dr[1]);
				}
				
				_oidToNameMappings.Add(conn.ServerVersion, oidToNameMapping);
				return oidToNameMapping;
			}
			
			
		}
		
	}
	
}
