
/*
  This is a testfile for the DataSet support in Npgsql with the NpgsqlDataAdapter object.
  
*/

// in C#
using System;
using System.Data;
using Npgsql;


public class test_executereader
{
  public static void Main(String[] args)
  {
	  NpgsqlConnection conn = null;
	NpgsqlEventLog.Level = LogLevel.Debug;
		  //NpgsqlEventLog.LogName = "testsuite.log";
		  NpgsqlEventLog.EchoMessages = true;
		try
		{
			conn = new NpgsqlConnection(NpgsqlTests.getConnectionString());
			conn.Open();
			Console.WriteLine("Connection completed");
			
			NpgsqlCommand command = new NpgsqlCommand();
			command.CommandText = "select * from tablea";
			
			command.Connection = conn;
			
			NpgsqlDataAdapter da = new NpgsqlDataAdapter();
			da.SelectCommand = command;
			
			DataSet ds = new DataSet();
			
			da.Fill(ds);
			
			ds.WriteXml(Console.Out);
			
			
			
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