//
// Npsql.cs
// author: Hiroshi Saito
//
using System;
using System.Data;
using Npgsql;

class Npsql {
	static string version = "0.1";
	public static void supsql(NpgsqlCommand command)
	{
		if(String.Compare(command.CommandText,0,"\\l",0,2) ==0)
			command.CommandText = "SELECT d.datname as \"Name\",u.usename as \"Owner\",pg_catalog.pg_encoding_to_char(d.encoding) as \"Encoding\" FROM pg_catalog.pg_database d LEFT JOIN pg_catalog.pg_user u ON d.datdba = u.usesysid ORDER BY 1;";
		else
		if(String.Compare(command.CommandText,0,"\\d",0,2) ==0)
			command.CommandText = "SELECT n.nspname as \"Schema\",c.relname as \"Name\",CASE c.relkind WHEN 'r' THEN 'table' WHEN 'v' THEN 'view' WHEN 'i' THEN 'index' WHEN 'S' THEN 'sequence' WHEN 's' THEN 'special' END as \"Type\",u.usename as \"Owner\" FROM pg_catalog.pg_class c LEFT JOIN pg_catalog.pg_user u ON u.usesysid = c.relowner LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace WHERE c.relkind IN ('r','v','S','') AND n.nspname NOT IN ('pg_catalog', 'pg_toast') AND pg_catalog.pg_table_is_visible(c.oid) ORDER BY 1,2;";

	}
	public static void welcommsg()
	{
		Console.WriteLine("Welcome to Npsql {0} , the PostgreSQL mini terminal.",version);
		Console.WriteLine("Type:  \\l list all databases");
		Console.WriteLine("       \\d describe table, index, sequence, or view");
		Console.WriteLine("       \\q to quit");

	}
	public static void helpmsg()
	{
		Console.WriteLine("usage:");
		Console.WriteLine("  Npsql HOSTNAME [[DBNAME] USERNAME]\n");
		Console.WriteLine("  DBNAME    specify database name to connect to (default: template1)");
		Console.WriteLine("  USERNAME  user name to connect to (default: postgres)");
		Console.WriteLine("");
	}
        public static int Main(string[] args)
        {
                String url = "localhost";
		String dbn = "template1";
		String usr = "postgres";
		if (args.Length <= 0)
		{
			helpmsg();
			return 0;
		}
                url = args[0];
		if (args.Length == 2)
		{
                	usr = args[1];
		}
		else
		if (args.Length == 3)
		{
                	dbn = args[1];
                	usr = args[2];
		}

		NpgsqlConnection cnDB;

                // Connect to database
                Console.WriteLine("Connecting to ... " + url);
		String constr = "DATABASE=" + dbn + ";SERVER=" + url + ";PORT=5432;UID=" + usr + ";PWD=;";
                cnDB = new NpgsqlConnection(constr);

		try 
		{
			cnDB.Open();
		} 
		catch (NpgsqlException ex)
		{
			Console.WriteLine(ex.ToString());
			return 1;
		}

		// Get the PostgreSQL version number as proof
		try
		{
			NpgsqlCommand cmdVer = new NpgsqlCommand("SELECT version()", cnDB);
			Object ObjVer = cmdVer.ExecuteScalar();
			Console.WriteLine(ObjVer.ToString());
		}
		catch(NpgsqlException ex)
		{
			Console.WriteLine(ex.ToString());
			return 1;
		}

		welcommsg();

		//
		try
		{
		  do{
			NpgsqlCommand command = new NpgsqlCommand();
			Console.Write("\nNpsql>");
			command.CommandText = Console.ReadLine();
			if(String.Compare(command.CommandText,0,"\\q",0,2) ==0)
				break;
			// command helper
			supsql(command);
			command.Connection = cnDB;
			NpgsqlDataReader dr;
			try
			{
				dr = command.ExecuteReader();
			}
			catch(NpgsqlException ex)
			{
				Console.WriteLine(ex.ToString());
				continue;
			}
			do
			{
				Int32 j,i;
				j = dr.FieldCount;
				DataTable dt = dr.GetSchemaTable();
				DataRowCollection schemarows = dt.Rows;
				for (i = 0; i < j; i++)
				{
					Console.Write("{0} \t", schemarows[i][0]);
				}
				Console.WriteLine("\n============================================");
				while(dr.Read())
				{
					for (i = 0; i < j; i++)
						Console.Write("{0} \t", dr[i]);
					Console.WriteLine();
				}
                       } while(dr.NextResult());
                    } while(true);
		}
		catch(NpgsqlException ex)
		{
			Console.WriteLine(ex.ToString());
		}
		if (cnDB != null)
		{
			if (cnDB.State != ConnectionState.Closed)
			{
				try
				{
					cnDB.Close();
				}
				catch (NpgsqlException ex)
				{
				}
			}
		} 
		return 1;
	}
}
