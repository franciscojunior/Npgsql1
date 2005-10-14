// created on 30/11/2002 at 22:35
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
using Npgsql;
using NUnit.Framework;
using NUnit.Core;
using System.Data;
using System.Globalization;
using NpgsqlTypes;


namespace NpgsqlTests
{

    public enum EnumTest : short
    {
        Value1 = 0,
        Value2 = 1
    };

    [TestFixture]
    public class CommandTests
    {
        private NpgsqlConnection 	_conn = null;
        private String 						_connString = "Server=localhost;User ID=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests;maxpoolsize=2;";

        [SetUp]
        protected void SetUp()
        {
            //NpgsqlEventLog.Level = LogLevel.None;
            //NpgsqlEventLog.Level = LogLevel.Debug;
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
        public void ParametersGetName()
        {
            NpgsqlCommand command = new NpgsqlCommand();

            // Add parameters.
            command.Parameters.Add(new NpgsqlParameter(":Parameter1", DbType.Boolean));
            command.Parameters.Add(new NpgsqlParameter(":Parameter2", DbType.Int32));
            command.Parameters.Add(new NpgsqlParameter(":Parameter3", DbType.DateTime));
            command.Parameters.Add(new NpgsqlParameter("Parameter4", DbType.DateTime));

            IDbDataParameter idbPrmtr = command.Parameters["Parameter1"];
            command.Parameters[0].Value = 1;

            // Get by indexers.

            Assert.AreEqual(":Parameter1", command.Parameters[":Parameter1"].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[":Parameter2"].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[":Parameter3"].ParameterName);
            Assert.AreEqual(":Parameter4", command.Parameters["Parameter4"].ParameterName);

            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[2].ParameterName);
            Assert.AreEqual("Parameter4", command.Parameters[3].ParameterName);



        }

        [Test]
        public void EmptyQuery()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand(";", _conn);
            command.ExecuteNonQuery();

        }
        
        
        [Test]
        public void NoNameParameterAdd()
        {
            NpgsqlCommand command = new NpgsqlCommand();

            command.Parameters.Add(new NpgsqlParameter());
            command.Parameters.Add(new NpgsqlParameter());
            
            
            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);

        }
        
        
        

        [Test]
        public void FunctionCallFromSelect()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select * from funcB()", _conn);

            NpgsqlDataReader reader = command.ExecuteReader();

            Assertion.AssertNotNull(reader);
            //reader.FieldCount

        }

        [Test]
        public void ExecuteScalar()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select count(*) from tablea", _conn);

            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);
            //reader.FieldCount

        }
        
        [Test]
        public void TransactionSetOk()
        {
            _conn.Open();
            
            NpgsqlTransaction t = _conn.BeginTransaction();

            NpgsqlCommand command = new NpgsqlCommand("select count(*) from tablea", _conn);
            
            command.Transaction = t;
            
            Object result = command.ExecuteScalar();
            
            t.Commit();

            Assert.AreEqual(5, result);
            //reader.FieldCount

        }
        
        
        [Test]
        public void InsertStringWithBackslashes()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:p0)", _conn);
            
            command.Parameters.Add(new NpgsqlParameter("p0", NpgsqlDbType.Text));
            
            command.Parameters["p0"].Value = @"\test";

            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            NpgsqlCommand command2 = new NpgsqlCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
            
            
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(@"\test", result);
            
            
            
            //reader.FieldCount

        }
        
               
        
        [Test]
        public void UseStringParameterWithNoNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:p0)", _conn);
            
            command.Parameters.Add(new NpgsqlParameter("p0", "test"));
            
            
            Assert.AreEqual(command.Parameters[0].NpgsqlDbType, NpgsqlDbType.Text);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.String);
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            NpgsqlCommand command2 = new NpgsqlCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
            
            
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual("test", result);
            
            
            
            //reader.FieldCount

        }
        
        
        [Test]
        public void UseIntegerParameterWithNoNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_int4) values (:p0)", _conn);
            
            command.Parameters.Add(new NpgsqlParameter("p0", 5));
            
            Assert.AreEqual(command.Parameters[0].NpgsqlDbType, NpgsqlDbType.Integer);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int32);
            
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            NpgsqlCommand command2 = new NpgsqlCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
            
            
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(5, result);
            
            
            
            //reader.FieldCount

        }
        
        
        //[Test]
        public void UseSmallintParameterWithNoNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_int4) values (:p0)", _conn);
            
            command.Parameters.Add(new NpgsqlParameter("p0", (Int16)5));
            
            Assert.AreEqual(command.Parameters[0].NpgsqlDbType, NpgsqlDbType.Smallint);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int16);
            
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            NpgsqlCommand command2 = new NpgsqlCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
            
            
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(5, result);
            
            
            
            //reader.FieldCount

        }
        
        
        

        [Test]
        public void FunctionCallReturnSingleValue()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC();", _conn);
            command.CommandType = CommandType.StoredProcedure;

            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);
            //reader.FieldCount

        }
        
        
        [Test]
        public void RollbackWithNoTransaction()
        {
            _conn.Open();

            NpgsqlTransaction t = _conn.BeginTransaction();
            
            t.Rollback();
            t.Rollback();
            //reader.FieldCount

        }


        [Test]
        public void FunctionCallReturnSingleValueWithPrepare()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC()", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Prepare();
            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);
            //reader.FieldCount

        }

        [Test]
        public void FunctionCallWithParametersReturnSingleValue()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));

            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);


        }

        [Test]
        public void FunctionCallWithParametersReturnSingleValueNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Integer));

            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);

        }




        [Test]
        public void FunctionCallWithParametersPrepareReturnSingleValue()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));

            Assert.AreEqual(1, command.Parameters.Count);
            command.Prepare();


            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);


        }

        [Test]
        public void FunctionCallWithParametersPrepareReturnSingleValueNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Integer));

            Assert.AreEqual(1, command.Parameters.Count);
            command.Prepare();


            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);


        }



        [Test]
        public void FunctionCallWithParametersPrepareReturnSingleValueNpgsqlDbType2()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC(@a)", _conn);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Integer));

            Assert.AreEqual(1, command.Parameters.Count);
            //command.Prepare();


            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);


        }


        [Test]
        public void FunctionCallReturnResultSet()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcB()", _conn);
            command.CommandType = CommandType.StoredProcedure;

            NpgsqlDataReader dr = command.ExecuteReader();




        }


        [Test]
        public void CursorStatement()
        {

            _conn.Open();

            Int32 i = 0;

            NpgsqlTransaction t = _conn.BeginTransaction();

            NpgsqlCommand command = new NpgsqlCommand("declare te cursor for select * from tablea;", _conn);

            command.ExecuteNonQuery();

            command.CommandText = "fetch forward 3 in te;";

            NpgsqlDataReader dr = command.ExecuteReader();


            while (dr.Read())
            {
                i++;
            }

            Assert.AreEqual(3, i);


            i = 0;

            command.CommandText = "fetch backward 1 in te;";

            NpgsqlDataReader dr2 = command.ExecuteReader();

            while (dr2.Read())
            {
                i++;
            }

            Assert.AreEqual(1, i);

            command.CommandText = "close te;";

            command.ExecuteNonQuery();

            t.Commit();



        }

        [Test]
        public void PreparedStatementNoParameters()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select * from tablea;", _conn);

            command.Prepare();

            
            NpgsqlDataReader dr = command.ExecuteReader();


        }
        
        
        [Test]
        public void PreparedStatementInsert()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:p0);", _conn);
            command.Parameters.Add(new NpgsqlParameter("p0", NpgsqlDbType.Text));
            command.Parameters["p0"].Value = "test";
            

            command.Prepare();

            
            NpgsqlDataReader dr = command.ExecuteReader();
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteNonQuery();
            


        }
        
        [Test]
        public void RTFStatementInsert()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:p0);", _conn);
            command.Parameters.Add(new NpgsqlParameter("p0", NpgsqlDbType.Text));
            command.Parameters["p0"].Value = @"{\rtf1\ansi\ansicpg1252\uc1 \deff0\deflang1033\deflangfe1033{";
                       
            
            NpgsqlDataReader dr = command.ExecuteReader();
            
            
            String result = (String)new NpgsqlCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteScalar();
            
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(@"{\rtf1\ansi\ansicpg1252\uc1 \deff0\deflang1033\deflangfe1033{", result);


        }
        
        
        
        [Test]
        public void PreparedStatementInsertNullValue()
        {


            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_int4) values (:p0);", _conn);
            command.Parameters.Add(new NpgsqlParameter("p0", NpgsqlDbType.Integer));
            command.Parameters["p0"].Value = DBNull.Value;
            

            command.Prepare();

            
            NpgsqlDataReader dr = command.ExecuteReader();
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteNonQuery();
            


        }
        
        

        [Test]
        public void PreparedStatementWithParameters()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));
            command.Parameters.Add(new NpgsqlParameter("b", DbType.Int64));

            Assert.AreEqual(2, command.Parameters.Count);

            Assert.AreEqual(DbType.Int32, command.Parameters[0].DbType);

            command.Prepare();

            command.Parameters[0].Value = 3;
            command.Parameters[1].Value = 5;

            NpgsqlDataReader dr = command.ExecuteReader();




        }

        [Test]
        public void PreparedStatementWithParametersNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Integer));
            command.Parameters.Add(new NpgsqlParameter("b", NpgsqlDbType.Bigint));

            Assert.AreEqual(2, command.Parameters.Count);

            Assert.AreEqual(DbType.Int32, command.Parameters[0].DbType);

            command.Prepare();

            command.Parameters[0].Value = 3;
            command.Parameters[1].Value = 5;

            NpgsqlDataReader dr = command.ExecuteReader();




        }
        
        [Test]
        public void FunctionCallWithImplicitParameters()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC", _conn);
            command.CommandType = CommandType.StoredProcedure;


            NpgsqlParameter p = new NpgsqlParameter("@a", NpgsqlDbType.Integer);
            p.Direction = ParameterDirection.InputOutput;
            p.Value = 4;
            
            command.Parameters.Add(p);
            

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }
        
        
        [Test]
        public void PreparedFunctionCallWithImplicitParameters()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC", _conn);
            command.CommandType = CommandType.StoredProcedure;


            NpgsqlParameter p = new NpgsqlParameter("a", NpgsqlDbType.Integer);
            p.Direction = ParameterDirection.InputOutput;
            p.Value = 4;
            
            command.Parameters.Add(p);
            
            command.Prepare();

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }
        
        
        
        [Test]
        public void FunctionCallWithImplicitParametersWithNoParameters()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC", _conn);
            command.CommandType = CommandType.StoredProcedure;

            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);
            //reader.FieldCount

        }
        
        [Test]
        public void FunctionCallOutputParameter()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("funcC()", _conn);
            command.CommandType = CommandType.StoredProcedure;
            
            NpgsqlParameter p = new NpgsqlParameter("a", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(5, command.Parameters["a"].Value);
        }
        
        [Test]
        public void FunctionCallOutputParameter2()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("funcC", _conn);
            command.CommandType = CommandType.StoredProcedure;
            
            NpgsqlParameter p = new NpgsqlParameter("@a", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(5, command.Parameters["@a"].Value);
        }
        
        [Test]
        public void OutputParameterWithoutName()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("funcC", _conn);
            command.CommandType = CommandType.StoredProcedure;
            
            NpgsqlParameter p = command.CreateParameter();
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(5, command.Parameters[0].Value);
        }
        
        [Test]
        public void FunctionReturnVoid()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("test(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;
            
            NpgsqlParameter p = new NpgsqlParameter("a", DbType.Int32);
            
            p.Value = 3;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            
        }
        
        [Test]
        public void StatementOutputParameters()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("select 4, 5;", _conn);
                        
            NpgsqlParameter p = new NpgsqlParameter("a", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            p = new NpgsqlParameter("b", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            
            p = new NpgsqlParameter("c", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters["a"].Value);
            Assert.AreEqual(5, command.Parameters["b"].Value);
            Assert.AreEqual(-1, command.Parameters["c"].Value);
        }
        
        [Test]
        public void FunctionCallInputOutputParameter()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;


            NpgsqlParameter p = new NpgsqlParameter("a", NpgsqlDbType.Integer);
            p.Direction = ParameterDirection.InputOutput;
            p.Value = 4;
            
            command.Parameters.Add(p);
            

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }
        
        
        [Test]
        public void StatementMappedOutputParameters()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("select 3, 4 as param1, 5 as param2, 6;", _conn);
                        
            NpgsqlParameter p = new NpgsqlParameter("param2", NpgsqlDbType.Integer);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            p = new NpgsqlParameter("param1", NpgsqlDbType.Integer);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            p = new NpgsqlParameter("p", NpgsqlDbType.Integer);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            
            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters["param1"].Value);
            Assert.AreEqual(5, command.Parameters["param2"].Value);
            Assert.AreEqual(-1, command.Parameters["p"].Value);
            
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ListenNotifySupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("listen notifytest;", _conn);
            command.ExecuteNonQuery();

            _conn.Notification += new NotificationEventHandler(NotificationSupportHelper);


            command = new NpgsqlCommand("notify notifytest;", _conn);
            command.ExecuteNonQuery();



        }

        private void NotificationSupportHelper(Object sender, NpgsqlNotificationEventArgs args)
        {
            throw new InvalidOperationException();
        }
		
		[Test]
        public void ByteSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Byte));

            command.Parameters[0].Value = 2;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.Parameters.Clear();
            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();
        }
        
        
		[Test]
        public void ByteaSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select field_bytea from tablef where field_serial = 1", _conn);


            Byte[] result = (Byte[]) command.ExecuteScalar();
            

            Assert.AreEqual(2, result.Length);

        }
        
        [Test]
        public void ByteaInsertSupport()
        {
            _conn.Open();


            Byte[] toStore = { 1 };

			NpgsqlCommand cmd = new NpgsqlCommand("insert into tablef(field_bytea) values (:val)", _conn);
			cmd.Parameters.Add(new NpgsqlParameter("val", DbType.Binary));
			cmd.Parameters[0].Value = toStore;
			cmd.ExecuteNonQuery();

			cmd = new NpgsqlCommand("select field_bytea from tablef where field_serial = (select max(field_serial) from tablef)", _conn);
			
			Byte[] result = (Byte[])cmd.ExecuteScalar();
			
			
			new NpgsqlCommand("delete from tablef where field_serial = (select max(field_serial) from tablef)", _conn).ExecuteNonQuery();
			
			
            Assert.AreEqual(1, result.Length);

        }
        
        
        
		[Test]
        public void ByteaParameterSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select field_bytea from tablef where field_bytea = :bytesData", _conn);
            
            Byte[] bytes = new Byte[]{45,44};
            
            command.Parameters.Add(":bytesData", NpgsqlTypes.NpgsqlDbType.Bytea);
			command.Parameters[":bytesData"].Value = bytes;


            Object result = command.ExecuteNonQuery();
            

            Assert.AreEqual(-1, result);

        }
        
        
        
        
		[Test]
        public void EnumSupport()
        {
        
            
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Smallint));

            command.Parameters[0].Value = EnumTest.Value1;
            

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.Parameters.Clear();
            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();
        }

        [Test]
        public void DateTimeSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select field_timestamp from tableb where field_serial = 2;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));

            DateTimeFormatInfo culture = new DateTimeFormatInfo();
            culture.TimeSeparator = ":";
            DateTime dt = System.DateTime.Parse("2004-06-04 09:48:00", culture);

            command.CommandText = "insert into tableb(field_timestamp) values (:a);delete from tableb where field_serial > 4;";
            command.Parameters.Add(new NpgsqlParameter("a", DbType.DateTime));
            command.Parameters[0].Value = dt;

            command.ExecuteScalar();

        }


        [Test]
        public void DateTimeSupportNpgsqlDbType()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select field_timestamp from tableb where field_serial = 2;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));

            DateTimeFormatInfo culture = new DateTimeFormatInfo();
            culture.TimeSeparator = ":";
            DateTime dt = System.DateTime.Parse("2004-06-04 09:48:00", culture);

            command.CommandText = "insert into tableb(field_timestamp) values (:a);delete from tableb where field_serial > 4;";
            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Timestamp));
            command.Parameters[0].Value = dt;

            command.ExecuteScalar();

        }

        [Test]
        public void DateSupport()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_date from tablec where field_serial = 1;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-03-04", d.ToString("yyyy-MM-dd"));

        }

        [Test]
        public void TimeSupport()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_time from tablec where field_serial = 2;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("10:03:45.345", d.ToString("HH:mm:ss.fff"));

        }
        
        [Test]
        public void DateTimeSupportTimezone()
        {
            _conn.Open();
            
            
            NpgsqlCommand command = new NpgsqlCommand("set time zone 5;select field_timestamp_with_timezone from tableg where field_serial = 1;", _conn);
            
            DateTime d = (DateTime)command.ExecuteScalar();
            
            
            Assert.AreEqual("2002-02-02 16:00:23Z", d.ToString("u"));
            
                        
        }

        [Test]
        public void NumericSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_numeric) values (:a)", _conn);
            command.Parameters.Add(new NpgsqlParameter("a", DbType.Decimal));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);


            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4000000M, result);




        }

        [Test]
        public void NumericSupportNpgsqlDbType()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_numeric) values (:a)", _conn);
            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Numeric));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);


            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4000000M, result);




        }


        [Test]
        public void InsertSingleValue()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new NpgsqlParameter(":a", DbType.Single));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4F, result);

        }


        [Test]
        public void InsertSingleValueNpgsqlDbType()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new NpgsqlParameter(":a", NpgsqlDbType.Real));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4F, result);

        }
        
        
        [Test]
        public void DoubleValueSupportWithExtendedQuery()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select count(*) from tabled where field_float8 = :a", _conn);
            command.Parameters.Add(new NpgsqlParameter(":a", NpgsqlDbType.Double));

            command.Parameters[0].Value = 0.123456789012345D;
            
            command.Prepare();

            Object rows = command.ExecuteScalar();

            
            Assert.AreEqual(1, rows);

        }

        [Test]
        public void InsertDoubleValue()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tabled(field_float8) values (:a)", _conn);
            command.Parameters.Add(new NpgsqlParameter(":a", DbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            command.CommandText = "select * from tabled where field_float8 = :a";


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


			Assert.AreEqual(1, rowsAdded);
            Assert.AreEqual(7.4D, result);

        }


        [Test]
        public void InsertDoubleValueNpgsqlDbType()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tabled(field_float8) values (:a)", _conn);
            command.Parameters.Add(new NpgsqlParameter(":a", NpgsqlDbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            command.CommandText = "select * from tabled where field_float8 = :a";


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();

			Assert.AreEqual(1, rowsAdded);
            Assert.AreEqual(7.4D, result);

        }


        [Test]
        public void NegativeNumericSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 4", _conn);


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            Assert.AreEqual(-4.3000000M, result);

        }


        [Test]
        public void PrecisionScaleNumericSupport()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("select * from tableb where field_serial = 4", _conn);


            NpgsqlDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            Assert.AreEqual(-4.3000000M, (Decimal)result);
            //Assert.AreEqual(11, result.Precision);
            //Assert.AreEqual(7, result.Scale);

        }

        [Test]
        public void InsertNullString()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.String));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            Int64 result = (Int64)command.ExecuteScalar();

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }

        [Test]
        public void InsertNullStringNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Text));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            Int64 result = (Int64)command.ExecuteScalar();

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }



        [Test]
        public void InsertNullDateTime()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_timestamp) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.DateTime));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }


        [Test]
        public void InsertNullDateTimeNpgsqlDbType()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_timestamp) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Timestamp));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }



        [Test]
        public void InsertNullInt16()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int16));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);


        }


        [Test]
        public void InsertNullInt16NpgsqlDbType()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Smallint));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);


        }


        [Test]
        public void InsertNullInt32()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_int4) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_int4 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(5, result);

        }


        [Test]
        public void InsertNullNumeric()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tableb(field_numeric) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Decimal));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_numeric is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(3, result);

        }

        [Test]
        public void InsertNullBoolean()
        {
            _conn.Open();


            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_bool) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Boolean));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_bool is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(5, result);

        }

        [Test]
        public void AnsiStringSupport()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.AnsiString));

            command.Parameters[0].Value = "TesteAnsiString";

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = String.Format("select count(*) from tablea where field_text = '{0}'", command.Parameters[0].Value);
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(1, result);

        }


        [Test]
        public void MultipleQueriesFirstResultsetEmpty()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("insert into tablea(field_text) values ('a'); select count(*) from tablea;", _conn);

            Object result = command.ExecuteScalar();


            command.CommandText = "delete from tablea where field_serial > 5";
            command.ExecuteNonQuery();

            command.CommandText = "select * from tablea where field_serial = 0";
            command.ExecuteScalar();


            Assert.AreEqual(6, result);


        }

        [Test]
        [ExpectedException(typeof(NpgsqlException))]
        public void ConnectionStringWithInvalidParameters()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=npgsql_tests;Password=j");

            NpgsqlCommand command = new NpgsqlCommand("select * from tablea", conn);

            command.Connection.Open();
            command.ExecuteReader();
            command.Connection.Close();


        }

        [Test]
        [ExpectedException(typeof(NpgsqlException))]
        public void InvalidConnectionString()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=npgsql_tests");

            NpgsqlCommand command = new NpgsqlCommand("select * from tablea", conn);

            command.Connection.Open();
            command.ExecuteReader();
            command.Connection.Close();


        }


        [Test]
        public void AmbiguousFunctionParameterType()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=npgsql_tests;Password=npgsql_tests");


            NpgsqlCommand command = new NpgsqlCommand("ambiguousParameterType(:a, :b, :c, :d, :e, :f)", conn);
            command.CommandType = CommandType.StoredProcedure;
            NpgsqlParameter p = new NpgsqlParameter("a", DbType.Int16);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new NpgsqlParameter("b", DbType.Int32);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new NpgsqlParameter("c", DbType.Int64);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new NpgsqlParameter("d", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new NpgsqlParameter("e", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new NpgsqlParameter("f", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);


            command.Connection.Open();
            command.ExecuteScalar();
            command.Connection.Close();


        }
        
        [Test]
        public void AmbiguousFunctionParameterTypePrepared()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=npgsql_tests;Password=npgsql_tests");


            NpgsqlCommand command = new NpgsqlCommand("ambiguousParameterType(:a, :b, :c, :d, :e, :f)", conn);
            command.CommandType = CommandType.StoredProcedure;
            NpgsqlParameter p = new NpgsqlParameter("a", DbType.Int16);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new NpgsqlParameter("b", DbType.Int32);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new NpgsqlParameter("c", DbType.Int64);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new NpgsqlParameter("d", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new NpgsqlParameter("e", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new NpgsqlParameter("f", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);


            command.Connection.Open();
            command.Prepare();
            command.ExecuteScalar();
            command.Connection.Close();


        }


        [Test]
        public void TestParameterReplace()
        {
            _conn.Open();

            String sql = @"select * from tablea where
field_serial = :a
                         ";


            NpgsqlCommand command = new NpgsqlCommand(sql, _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));

            command.Parameters[0].Value = 2;

            Int32 rowsAdded = command.ExecuteNonQuery();

        }
        
        [Test]
        public void TestParameterReplace2()
        {
            _conn.Open();

            String sql = @"select * from tablea where
                         field_serial = :a+1
                         ";


            NpgsqlCommand command = new NpgsqlCommand(sql, _conn);

            command.Parameters.Add(new NpgsqlParameter("a", DbType.Int32));

            command.Parameters[0].Value = 1;

            Int32 rowsAdded = command.ExecuteNonQuery();

        }
        
        [Test]
        public void TestParameterNameInParameterValue()
        {
            _conn.Open();

            String sql = "insert into tablea(field_text, field_int4) values ( :a, :b );" ;

            String aValue = "test :b";

            NpgsqlCommand command = new NpgsqlCommand(sql, _conn);

            command.Parameters.Add(new NpgsqlParameter(":a", NpgsqlDbType.Text));

            command.Parameters[":a"].Value = aValue;
            
            command.Parameters.Add(new NpgsqlParameter(":b", NpgsqlDbType.Integer));

            command.Parameters[":b"].Value = 1;

            Int32 rowsAdded = command.ExecuteNonQuery();
            
            
            NpgsqlCommand command2 = new NpgsqlCommand("select field_text, field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            
            NpgsqlDataReader dr = command2.ExecuteReader();
            
            dr.Read();
            
            String a = dr.GetString(0);;
            Int32 b = dr.GetInt32(1);
            
            dr.Close();
            
            
            new NpgsqlCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(aValue, a);
            Assert.AreEqual(1, b);
            
            
            
            

        }

        [Test]
        public void TestPointSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_point from tablee where field_serial = 1", _conn);

            NpgsqlPoint p = (NpgsqlPoint) command.ExecuteScalar();

            Assert.AreEqual(4, p.X);
            Assert.AreEqual(3, p.Y);
        }


        [Test]
        public void TestBoxSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_box from tablee where field_serial = 2", _conn);

            NpgsqlBox box = (NpgsqlBox) command.ExecuteScalar();

            Assert.AreEqual(5, box.UpperRight.X);
            Assert.AreEqual(4, box.UpperRight.Y);
            Assert.AreEqual(4, box.LowerLeft.X);
            Assert.AreEqual(3, box.LowerLeft.Y);


        }

        [Test]
        public void TestLSegSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_lseg from tablee where field_serial = 3", _conn);

            NpgsqlLSeg lseg = (NpgsqlLSeg) command.ExecuteScalar();

            Assert.AreEqual(4, lseg.Start.X);
            Assert.AreEqual(3, lseg.Start.Y);
            Assert.AreEqual(5, lseg.End.X);
            Assert.AreEqual(4, lseg.End.Y);


        }

        [Test]
        public void TestClosedPathSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_path from tablee where field_serial = 4", _conn);

            NpgsqlPath path = (NpgsqlPath) command.ExecuteScalar();

            Assert.AreEqual(false, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);


        }

        [Test]
        public void TestOpenPathSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_path from tablee where field_serial = 5", _conn);

            NpgsqlPath path = (NpgsqlPath) command.ExecuteScalar();

            Assert.AreEqual(true, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);


        }



        [Test]
        public void TestPolygonSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_polygon from tablee where field_serial = 6", _conn);

            NpgsqlPolygon polygon = (NpgsqlPolygon) command.ExecuteScalar();

            Assert.AreEqual(2, polygon.Count);
            Assert.AreEqual(4, polygon[0].X);
            Assert.AreEqual(3, polygon[0].Y);
            Assert.AreEqual(5, polygon[1].X);
            Assert.AreEqual(4, polygon[1].Y);


        }


        [Test]
        public void TestCircleSupport()
        {

            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select field_circle from tablee where field_serial = 7", _conn);

            NpgsqlCircle circle = (NpgsqlCircle) command.ExecuteScalar();

            Assert.AreEqual(4, circle.Center.X);
            Assert.AreEqual(3, circle.Center.Y);
            Assert.AreEqual(5, circle.Radius);



        }
        
        [Test]
        public void SetParameterValueNull()
        {
            _conn.Open();


            NpgsqlCommand cmd = new NpgsqlCommand("insert into tablef(field_bytea) values (:val)", _conn);
			NpgsqlParameter param = cmd.CreateParameter();
			param.ParameterName="val";
            param.NpgsqlDbType = NpgsqlDbType.Bytea;
			param.Value = DBNull.Value;
			
			cmd.Parameters.Add(param);
			
			cmd.ExecuteNonQuery();

			cmd = new NpgsqlCommand("select field_bytea from tablef where field_serial = (select max(field_serial) from tablef)", _conn);
			
			Object result = cmd.ExecuteScalar();
			
			
			new NpgsqlCommand("delete from tablef where field_serial = (select max(field_serial) from tablef)", _conn).ExecuteNonQuery();
			
			
            Assert.AreEqual(DBNull.Value, result);

        }
        
        
        [Test]
        public void TestCharParameterLength()
        {
            _conn.Open();
    
            String sql = "insert into tableh(field_char5) values ( :a );" ;
    
            String aValue = "atest";
    
            NpgsqlCommand command = new NpgsqlCommand(sql, _conn);
    
            command.Parameters.Add(new NpgsqlParameter(":a", NpgsqlDbType.Char));
    
            command.Parameters[":a"].Value = aValue;
            command.Parameters[":a"].Size = 5;
            
            Int32 rowsAdded = command.ExecuteNonQuery();
            
            
            NpgsqlCommand command2 = new NpgsqlCommand("select field_char5 from tableh where field_serial = (select max(field_serial) from tableh)", _conn);
            
            NpgsqlDataReader dr = command2.ExecuteReader();
            
            dr.Read();
            
            String a = dr.GetString(0);;
                        
            dr.Close();
            
            
            new NpgsqlCommand("delete from tableh where field_serial = (select max(field_serial) from tableh)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(aValue, a);
            
    
        }
        
        [Test]
        public void ParameterHandlingOnQueryWithParameterPrefix()
        {
            _conn.Open();

            NpgsqlCommand command = new NpgsqlCommand("select to_char(field_time, 'HH24:MI') from tablec where field_serial = :a;", _conn);
            
            NpgsqlParameter p = new NpgsqlParameter("a", NpgsqlDbType.Integer);
            p.Value = 2;
            
            command.Parameters.Add(p);

            String d = (String)command.ExecuteScalar();


            Assert.AreEqual("10:03", d);

        }
        
        [Test]
        public void MultipleRefCursorSupport()
        {
            _conn.Open();
            
            NpgsqlTransaction t = _conn.BeginTransaction();
            
            NpgsqlCommand command = new NpgsqlCommand("testmultcurfunc", _conn);
            command.CommandType = CommandType.StoredProcedure;
            
            NpgsqlDataReader dr = command.ExecuteReader();
            
            dr.Read();
            
            Int32 one = dr.GetInt32(0);
            
            dr.NextResult();
            
            dr.Read();
            
            Int32 two = dr.GetInt32(0);
            
            dr.Close();
            
            t.Commit();
            
            Assert.AreEqual(1, one);
            Assert.AreEqual(2, two);
            
            
        }
        
        [Test]
        public void ReturnRecordSupport()
        {
            _conn.Open();
            
            NpgsqlCommand command = new NpgsqlCommand("testreturnrecord", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new NpgsqlParameter(":a", NpgsqlDbType.Integer));
            command.Parameters[0].Direction = ParameterDirection.Output;

            command.Parameters.Add(new NpgsqlParameter(":b", NpgsqlDbType.Integer));
            command.Parameters[1].Direction = ParameterDirection.Output;

            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters[0].Value);
            Assert.AreEqual(5, command.Parameters[1].Value);
            


        }
        

        
        
    }

}

