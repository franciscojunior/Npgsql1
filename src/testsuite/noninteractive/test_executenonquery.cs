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
			
			// Check whether the insert statement works
			NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values ('Text from Npgsql');", conn);
			Int32 num_rows = command.ExecuteNonQuery();
			Console.WriteLine("{0} rows were added!", num_rows);
			
			// Check whether the update statement works
			command.CommandText = "update tablea set field_text='Updated Text from Npgsql' where field_text='Text from Npgsql';";
			num_rows = command.ExecuteNonQuery();
			Console.WriteLine("{0} rows were updated!", num_rows);

			// Check whether the delete statement works
			command.CommandText = "delete from tablea where field_text = 'Updated Text from Npgsql'";
			num_rows = command.ExecuteNonQuery();
			Console.WriteLine("{0} rows were deleted!", num_rows);

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
