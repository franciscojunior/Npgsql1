// in C#
using System;
using System.Data;
using Npgsql;
using Npgsql.NpgsqlTests;

public class Test_1
{
  public static void Main(String[] args)
    {

      String connstring = NPGSQL_TESTS_CONNECTIONSTRING;
      System.Console.WriteLine("String send from test is:" + connstring);

      NpgsqlConnection conn = new NpgsqlConnection(connstring);

      conn.Open();
      conn.Close();
    }
}
