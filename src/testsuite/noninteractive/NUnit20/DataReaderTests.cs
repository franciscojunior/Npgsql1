// created on 27/12/2002 at 17:05
// 
// Author:
// 	Francisco Figueiredo Jr. <fxjrlists@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Web.UI.WebControls;
using Npgsql;

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
			//NpgsqlEventLog.Level = LogLevel.None;
			NpgsqlEventLog.Level = LogLevel.Debug;
			NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
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
		public void GetDouble()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tabled where field_serial = 2;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			//Double result = Double.Parse(dr.GetInt32(2).ToString());
		  Double result = dr.GetDouble(2);
			
			Assertion.AssertEquals(.123456789012345D, result);
			
		}
		
		
		[Test]
		public void GetFloat()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tabled where field_serial = 1;", _conn);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			//Single result = Single.Parse(dr.GetInt32(2).ToString());
		  Single result = dr.GetFloat(1);
			
			Assertion.AssertEquals(.123456F, result);
			
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
		public void GetStringWithParameter()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_text = :value;", _conn);
			
			String test = "Random text";
			NpgsqlParameter param = new NpgsqlParameter();
			param.ParameterName = "value";
			param.DbType = DbType.String;
			//param.NpgsqlDbType = NpgsqlDbType.Text;
			param.Size = test.Length;
			param.Value = test;
			command.Parameters.Add(param);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			String result = dr.GetString(1);
			
			Assertion.AssertEquals(test, result);
			
		}
		
		[Test]
		public void GetStringWithQuoteWithParameter()
		{
			_conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_text = :value;", _conn);
			
			String test = "Text with ' single quote";
			NpgsqlParameter param = new NpgsqlParameter();
			param.ParameterName = "value";
			param.DbType = DbType.String;
			//param.NpgsqlDbType = NpgsqlDbType.Text;
			param.Size = test.Length;
			param.Value = test;
			command.Parameters.Add(param);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			String result = dr.GetString(1);
			
			Assertion.AssertEquals(test, result);
			
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
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetValueFromEmptyResultset()
		{
		  _conn.Open();
			NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_text = :value;", _conn);
			
			String test = "Text single quote";
			NpgsqlParameter param = new NpgsqlParameter();
			param.ParameterName = "value";
			param.DbType = DbType.String;
			//param.NpgsqlDbType = NpgsqlDbType.Text;
			param.Size = test.Length;
			param.Value = test;
			command.Parameters.Add(param);
			
			NpgsqlDataReader dr = command.ExecuteReader();
			
			dr.Read();
			
			
			// This line should throw the invalid operation exception as the datareader will
			// have an empty resultset.
			Console.WriteLine(dr.IsDBNull(1));
		  
			
		}
		
		
		[Test]
		public void TestOverlappedParameterNames()
		{
		  _conn.Open();
		 
		  NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = :a or field_serial = :aa", _conn);
		  command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32, 4, "a"));
		  command.Parameters.Add(new NpgsqlParameter("aa", DbType.Int32, 4, "aa"));
		  
		  command.Parameters[0].Value = 2;
		  command.Parameters[1].Value = 3;
		  
		  NpgsqlDataReader dr = command.ExecuteReader();
		  
		}
		
		[Test]
		[ExpectedException(typeof(NpgsqlException))]
		public void TestNonExistentParameterName()
		{
		  _conn.Open();
		  
		  NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_serial = :a or field_serial = :aa", _conn);
		  command.Parameters.Add(new NpgsqlParameter(":b", DbType.Int32, 4, "b"));
		  command.Parameters.Add(new NpgsqlParameter(":aa", DbType.Int32, 4, "aa"));
		  
		  command.Parameters[0].Value = 2;
		  command.Parameters[1].Value = 3;
		  
		  NpgsqlDataReader dr = command.ExecuteReader();
		  
		  
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
