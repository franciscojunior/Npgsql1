// in C#
using System;
using System.Data;
using Npgsql;


public class test_preparedstatements
{
  public static void Main(String[] args)
  {

	  NpgsqlConnection conn = null;
		try
		{
			conn = new NpgsqlConnection(NpgsqlTests.getConnectionString());
			
			conn.Open();
			Console.WriteLine("Connection completed");
			
			NpgsqlCommand command = new NpgsqlCommand();
			command.CommandText = "select * from tablea where field_int4 = :a;";
			command.Connection = conn;
			command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));
			
			command.Prepare();
			command.Parameters[0].Value = 4;
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			Int32 j;
			
			do
			{
			  j = dr.FieldCount;
			  Console.WriteLine(j);
			  
			  /*DataTable dt = dr.GetSchemaTable();
			  DataRowCollection schemarows = dt.Rows;
			  
			  
			  
			  for (i = 0; i < j; i++)
			  {
			    Console.Write("{0} \t", schemarows[i][0]);
			   
			  }
			  */
			  Int32 i;
			  
  			Console.WriteLine();
  			Console.WriteLine("============================================");
  			  			
			  while(dr.Read())
			  {
  			  for (i = 0; i < j; i++)
  			  {
  			    Console.Write("{0} \t", dr[i] == null ? "null" : dr[i]);
  			   
  			  }
  			  Console.WriteLine();
  			  
  			}
			  
			} while(dr.NextResult());
						
			dr.Close();			
			
		}
		catch(NpgsqlException e)
		{
			Console.WriteLine(e.ToString());
		}
		finally
		{
			
			if (conn != null)
				conn.Close();
		}
	}
}