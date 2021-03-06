//
// Author:
//  Francisco Figueiredo Jr. <fxjrlists@yahoo.com>
//
//  Copyright (C) 2002-2005 The Npgsql Development Team
//  npgsql-general@gborg.postgresql.org
//  http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using System.Configuration;
using Npgsql;

using NpgsqlTypes;

using NUnit.Framework;
using NUnit.Core;

namespace NpgsqlTests
{

    public abstract class BaseClassTests
    {
        
        // Connection tests will use.
        protected NpgsqlConnection _conn = null;
        
        // Transaction to rollback tests modifications.
        protected NpgsqlTransaction _t = null;
        
        // Commit transaction when test finish?   
        private Boolean commitTransaction = false;
        
        // Connection string
        
        protected String _connString = ConfigurationSettings.AppSettings["ConnectionString"];
        
        
        protected Boolean CommitTransaction
        {
            set
            {
                commitTransaction = value;
            }
            
            get
            {
                return commitTransaction;
            }
        }
        
        
        [SetUp]
        protected virtual void SetUp()
        {
            //NpgsqlEventLog.Level = LogLevel.None;
            //NpgsqlEventLog.Level = LogLevel.Debug;
            //NpgsqlEventLog.LogName = "NpgsqlTests.LogFile";
            _conn = new NpgsqlConnection(_connString);
            _conn.Open();
            _t = _conn.BeginTransaction();
            
            CommitTransaction = false;
        }

        [TearDown]
        protected virtual void TearDown()
        {
            if (_t != null && _t.Connection != null)
                if (CommitTransaction)
                    _t.Commit();
                else
                    _t.Rollback();
                
            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }
        
    }
}
