// in C#
using System;
using System.Data;
using Npgsql;
using Npgsql.NpgsqlTests;

public class Test_1
{
  public static void Main(String[] args)
    {

      String connstring;
      connstring = NpgsqlTests.getConnectionString();
      System.Console.WriteLine("Test Works");

      NpgsqlConnection conn = new NpgsqlConnection(connstring);

      conn.Open();
      conn.Close();
    }
}
