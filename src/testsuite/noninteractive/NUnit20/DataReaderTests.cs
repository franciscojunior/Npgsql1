// created on 27/12/2002 at 17:05
using System;
using System.Data;
using System.Web.UI.WebControls;
using Npgsql;

using NpgsqlTypes;

using NUnit.Framework;
using NUnit.Core;

namespace NpgsqlTests
{
	
	[TestFixture]
	public class DataReaderTests
	{
		
		private NpgsqlConnection 	_conn = null;
		private String 						_connString = "Server=localhost;User ID=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests";
		
		[SetUp]
		protected void SetUp()
		{
			NpgsqlEventLog.Level = LogLevel.None;
			//NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
			_conn = new NpgsqlConnection(_connString);
		}
		
		[TearDown]
		protected void TearDown()
		{
			if (_conn.State != ConnectionState.Closed)
				_conn.Close();
		}
		
		[Test]
		public void GetBoolean()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 4;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			Boolean result = dr.GetBoolean(4);
			Assertion.AssertEquals(true, result);
			
		}
		
		[Test]
		public void GetNpgsqlBoolean()
		{
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 4;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			NpgsqlBoolean result = dr.GetNpgsqlBoolean(4);
			Assertion.AssertEquals(true, (Boolean)result);
			
		}
		
		[Test]
		public void GetChars()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			Char[] result = new Char[6];
			
			
			Int64 a = dr.GetChars(1, 0, result, 0, 6);
			
			Assertion.AssertEquals("Random", new String(result));
			/*ConsoleWriter cw = new ConsoleWriter(Console.Out);
			
			cw.WriteLine(result);*/
			
			
		}
		
		[Test]
		public void GetInt32()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 2;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			
			Int32 result = dr.GetInt32(2);
			
			//ConsoleWriter cw = new ConsoleWriter(Console.Out);
			
			//cw.WriteLine(result.GetType().Name);
			Assertion.AssertEquals(4, result);
			
		}
		
		[Test]
		public void GetNpgsqlInt32()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 2;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			
			NpgsqlInt32 result = dr.GetNpgsqlInt32(2);
			
			//ConsoleWriter cw = new ConsoleWriter(Console.Out);
			
			//cw.WriteLine(result.GetType().Name);
			Assertion.AssertEquals(4, (Int32)result);
			
		}
		
		[Test]
		public void GetInt16()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			Int16 result = dr.GetInt16(1);
			
			Assertion.AssertEquals(2, result);
			
		}
		
		[Test]
		public void GetNpgsqlInt16()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			NpgsqlInt16 result = dr.GetNpgsqlInt16(1);
			
			Assertion.AssertEquals(new NpgsqlInt16(2), result);
			
		}
		
		[Test]
		public void GetDecimal()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 3;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			Decimal result = dr.GetDecimal(3);
			
						
			Assertion.AssertEquals(4.23M, result);
			
		}
	
	
		[Test]
		public void GetNpgsqlDecimal()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 3;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			NpgsqlDecimal result = dr.GetNpgsqlDecimal(3);
			
						
			Assertion.AssertEquals(4.23M, (Decimal)result);
			
		}
		
		[Test]
		public void GetDouble()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 2;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			Double result = Double.Parse(dr.GetInt32(2).ToString());
			
			Assertion.AssertEquals(4, result);
			
		}
		
		
		[Test]
		public void GetFloat()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 2;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			Single result = Single.Parse(dr.GetInt32(2).ToString());
			
			Assertion.AssertEquals(4, result);
			
		}
		
		
		[Test]
		public void GetString()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			String result = dr.GetString(1);
			
			Assertion.AssertEquals("Random text", result);
			
		}
		
		
		[Test]
		public void GetNpgsqlString()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			NpgsqlString result = dr.GetNpgsqlString(1);
			
			Assertion.AssertEquals("Random text", (String)result);
			
		}
		
		[Test]
		public void GetValueByName()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			String result = (String) dr["field_text"];
			
			Assertion.AssertEquals("Random text", result);
			
		}
		
		[Test]
		public void UseDataAdapter()
		{
			
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea", _conn);
			
			NpgsqlDataAdapter da = new NpgsqlDataAdapter();
			
			da.SelectCommand = command;
			
			DataSet ds = new DataSet();
			
			da.Fill(ds);
			
			//ds.WriteXml("TestUseDataAdapter.xml");
						
			
		}
		
		[Test]
		public void UseDataAdapterNpgsqlConnectionConstructor()
		{
			
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea", _conn);
			
			command.Connection = _conn;
			
			NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
			
			DataSet ds = new DataSet();
			
			da.Fill(ds);
			
			//ds.WriteXml("TestUseDataAdapterNpgsqlConnectionConstructor.xml");
						
			
		}
		
		[Test]
		public void UseDataAdapterStringNpgsqlConnectionConstructor()
		{
			
			_conn.Open();
			
						
			NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from tablea", _conn);
			
			DataSet ds = new DataSet();
			
			da.Fill(ds);
			
			//ds.WriteXml("TestUseDataAdapterStringNpgsqlConnectionConstructor.xml");
						
			
		}
		
		
		[Test]
		public void UseDataAdapterStringStringConstructor()
		{
			
			_conn.Open();
			
						
			NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from tablea", _connString);
			
			DataSet ds = new DataSet();
			
			da.Fill(ds);
			
			ds.WriteXml("TestUseDataAdapterStringStringConstructor.xml");
						
			
		}
		
		[Test]
		public void UseDataAdapterStringStringConstructor2()
		{
			
			_conn.Open();
			
						
			NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from tableb", _connString);
			
			DataSet ds = new DataSet();
			
			da.Fill(ds);
			
			ds.WriteXml("TestUseDataAdapterStringStringConstructor2.xml");
						
			
		}
		
		[Test]
		public void DataGridWebControlSupport()
		{
			
			_conn.Open();
			
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			DataGrid dg = new DataGrid();
			
			dg.DataSource = dr;
			dg.DataBind();
			
			
		}
		
		
	
	}
}