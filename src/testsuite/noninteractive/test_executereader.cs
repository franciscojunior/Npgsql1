// in C#
using System;
using System.Data;
using Npgsql;


public class test_executereader
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
			command.CommandText = "select * from tablea;";
			command.Connection = conn;
			NpgsqlDataReader dr = command.ExecuteReader();
			
			Int32 j;
			
			do
			{
			  j = dr.FieldCount;
			  Console.WriteLine(j);
			  DataTable dt = dr.GetSchemaTable();
			  DataRowCollection schemarows = dt.Rows;
			  
			  Int32 i;
			  
			  for (i = 0; i < j; i++)
			  {
			    Console.Write("{0} \t", schemarows[i][0]);
			   
			  }
  			Console.WriteLine();
  			Console.WriteLine("============================================");
  			  			
			  while(dr.Read())
			  {
  			  for (i = 0; i < j; i++)
  			  {
  			    Console.Write("{0} \t", dr[i]);
  			   
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