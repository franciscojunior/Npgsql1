// in C#
using System;
using System.Data;
using Npgsql;
using Npgsql.NpgsqlTests;

public class test_executescalar
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
			command.CommandText = "select count(*) from tablea";
			command.Connection = conn;
			Int32 result = (Int32) command.ExecuteScalar();
			Console.WriteLine(result.ToString());
			Console.WriteLine(result.GetType());
			
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
