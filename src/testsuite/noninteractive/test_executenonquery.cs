// in C#
using System;
using System.Data;
using Npgsql;
using Npgsql.NpgsqlTests;

public class test_executenonquery
{
  public static void Main(String[] args)
  {
    NpgsqlConnection conn = null;
    
    try
		{
			conn = new NpgsqlConnection(NpgsqlTests.getConnectionString());
			conn.Open();
			Console.WriteLine("Connection completed");
			
			NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values ('Text from Npgsql');", conn);
			Int32 num_rows = command.ExecuteNonQuery();
			Console.WriteLine("{0} rows were added!", num_rows);
			
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
