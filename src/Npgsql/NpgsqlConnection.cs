// created on 10/5/2002 at 23:01
// Npgsql.NpgsqlConnection.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Tls;
using NpgsqlTypes;
using Npgsql.Design;


namespace Npgsql
{
    /// <summary>
    /// Represents the method that handles the <see cref="Npgsql.NpgsqlConnection.Notification">Notification</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="Npgsql.NpgsqlNotificationEventArgs">NpgsqlNotificationEventArgs</see> that contains the event data.</param>
    public delegate void NotificationEventHandler(Object sender, NpgsqlNotificationEventArgs e);

    /// <summary>
    /// This class represents a connection to a
    /// PostgreSQL server.
    /// </summary>
    [System.Drawing.ToolboxBitmapAttribute(typeof(NpgsqlConnection))]
    public sealed class NpgsqlConnection : Component, IDbConnection, ICloneable
    {
        // Logging related values
        private readonly String CLASSNAME = "NpgsqlConnection";

        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        public event NotificationEventHandler   Notification;

        private NpgsqlState			                state;

        private ConnectionState                 connection_state;
//        private String                          connection_string;
        private ListDictionary                  connection_string_values;
        // some of the following constants are needed
        // for designtime support so I made them 'internal'
        // as I didn't want to add another interface for internal access
        // --brar
        // In the connection string
        internal readonly Char CONN_DELIM	= ';';  // Delimeter
        internal readonly Char CONN_ASSIGN	= '=';
        internal readonly String CONN_SERVER 	= "SERVER";
        internal readonly String CONN_PORT 	= "PORT";
        internal readonly String CONN_PROTOCOL 	= "PROTOCOL";
        internal readonly String CONN_DATABASE	= "DATABASE";
        internal readonly String CONN_USERID 	= "USER ID";
        internal readonly String CONN_PASSWORD	= "PASSWORD";
        internal readonly String CONN_SSL_ENABLED	= "SSL";
        internal readonly String CONN_ENCODING = "ENCODING";
        internal readonly String CONN_TIMEOUT = "TIMEOUT";

        // These are for ODBC connection string compatibility
        internal readonly String ODBC_USERID 	= "UID";
        internal readonly String ODBC_PASSWORD = "PWD";

        // These are for the connection pool
        internal readonly String MIN_POOL_SIZE = "MINPOOLSIZE";
        internal readonly String MAX_POOL_SIZE = "MAXPOOLSIZE";

        // Connection string defaults
        internal readonly Int32 DEF_PORT = 5432;
        internal readonly String DEF_ENCODING = "SQL_ASCII";
        internal readonly Int32 DEF_MIN_POOL_SIZE = 1;
        internal readonly Int32 DEF_MAX_POOL_SIZE = 20;
        internal readonly Int32 DEF_TIMEOUT = 15;


        // Values for possible CancelRequest messages.
        private NpgsqlBackEndKeyData            backend_keydata;

        // Flag for transaction status.
        private Boolean                         _inTransaction = false;

        // Mediator which will hold data generated from backend.
        private NpgsqlMediator                  _mediator;
        // Connector being used for the active connection.
        private Connector                       _connector;

        private Boolean                         _supportsPrepare = false;

        private Hashtable                       _oidToNameMapping;

        private System.Resources.ResourceManager resman;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> class.
        /// </summary>
        public NpgsqlConnection() : this(String.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> class
        /// and sets the <see cref="Npgsql.NpgsqlConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        /// <param name="ConnectionString">The connection used to open the PostgreSQL database.</param>
        public NpgsqlConnection(String ConnectionString)
        {
            resman = new System.Resources.ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, ConnectionString);

            connection_state = ConnectionState.Closed;
            state = NpgsqlClosedState.Instance;

            _mediator = new NpgsqlMediator();
            _connector = null;
            _oidToNameMapping = new Hashtable();

            connection_string_values = ParseConnectionString(ConnectionString);
        }

        /// <summary>
        /// Gets or sets the string used to connect to a PostgreSQL database.
        /// Valid values are:
        /// Server:        Address/Name of Postgresql Server;
        /// Port:          Port to connect to;
        /// Protocol:      Protocol version to use, instead of automatic; Integer 2 or 3;
        /// Database:      Database name. Defaults to user name if not specified;
        /// User:          User name;
        /// Password:      Password for clear text authentication;
        /// MinPoolSize:   Min size of connection pool;
        /// MaxPoolSize:   Max size of connection pool;
        /// Encoding:      Encoding to be used;
        /// Timeout:       Time to wait for connection open in seconds.
        /// </summary>
        /// <value>The connection string that includes the server name,
        /// the database name, and other parameters needed to establish
        /// the initial connection. The default value is an empty string.
        /// </value>
        [RefreshProperties(RefreshProperties.All), DefaultValue(""), RecommendedAsConfigurable(true)]
        [NpgsqlSysDescription("Description_ConnectionString", typeof(NpgsqlConnection)), Category("Data")]
        [Editor(typeof(ConnectionStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public String ConnectionString {
            get
            {
                StringBuilder      S = new StringBuilder();

                foreach (DictionaryEntry DE in connection_string_values) {
                    S.AppendFormat("{0}={1};", DE.Key, DE.Value);
                }

                return S.ToString();
//                return connection_string;
            }
            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "ConnectionString", value);
                connection_string_values = ParseConnectionString(value);
            }
        }

        /// <summary>
        /// Gets the ListDictionary containing the parsed connection string values.
        /// </summary>
        internal ListDictionary ConnectionStringValues {
            get
            {
                return connection_string_values;
            }
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection
        /// before terminating the attempt and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</value>
        [NpgsqlSysDescription("Description_ConnectionTimeout", typeof(NpgsqlConnection))]
        public Int32 ConnectionTimeout {
            get
            {
                return this.ConnectStringValueToInt32(CONN_TIMEOUT, DEF_TIMEOUT);
            }
        }

        ///<summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <value>The name of the current database or the name of the database to be
        /// used after a connection is opened. The default value is an empty string.</value>
        [NpgsqlSysDescription("Description_Database", typeof(NpgsqlConnection))]
        public String Database {
            get
            {
                return DatabaseName;
            }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>A bitwise combination of the <see cref="System.Data.ConnectionState">ConnectionState</see> values. The default is <b>Closed</b>.</value>
        [Browsable(false)]
        public ConnectionState State {
            get
            {
                return connection_state;
            }
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>An <see cref="System.Data.IDbTransaction">IDbTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently there's no support for nested transactions.
        /// </remarks>
        IDbTransaction IDbConnection.BeginTransaction()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbConnection.BeginTransaction");

            return BeginTransaction();
        }

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="level">The <see cref="System.Data.IsolationLevel">isolation level</see> under which the transaction should run.</param>
        /// <returns>An <see cref="System.Data.IDbTransaction">IDbTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently the IsolationLevel ReadCommitted and Serializable are supported by the PostgreSQL backend.
        /// There's no support for nested transactions.
        /// </remarks>
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbConnection.BeginTransaction", level);

            return BeginTransaction(level);
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently there's no support for nested transactions.
        /// </remarks>
        public NpgsqlTransaction BeginTransaction()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "BeginTransaction");
            return this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="level">The <see cref="System.Data.IsolationLevel">isolation level</see> under which the transaction should run.</param>
        /// <returns>A <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently the IsolationLevel ReadCommitted and Serializable are supported by the PostgreSQL backend.
        /// There's no support for nested transactions.
        /// </remarks>
        public NpgsqlTransaction BeginTransaction(IsolationLevel level)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "BeginTransaction", level);

            if (_inTransaction)
                throw new InvalidOperationException(resman.GetString("Exception_NoNestedTransactions"));


            return new NpgsqlTransaction(this, level);
        }

        /// <summary>
        /// This method changes the current database by disconnecting from the actual
        /// database and connecting to the specified.
        /// </summary>
        /// <param name="dbName">The name of the database to use in place of the current database.</param>
        public void ChangeDatabase(String dbName)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ChangeDatabase", dbName);

            if (dbName == null)
                throw new ArgumentNullException("dbName");

            if (dbName == String.Empty)
                throw new ArgumentOutOfRangeException(String.Format(resman.GetString("Exception_InvalidDbName"), dbName), "dbName");

            if(this.connection_state != ConnectionState.Open)
                throw new InvalidOperationException(resman.GetString("Exception_ChangeDatabaseOnOpenConn"));

            String oldDatabaseName = ConnectStringValueToString(CONN_DATABASE);
            Close();

            connection_string_values[CONN_DATABASE] = dbName;            

            Open();
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the
        /// <see cref="Npgsql.NpgsqlConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        public void Open()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Open");

            // Check if the connection is already open.
            if (connection_state == ConnectionState.Open) {
                throw new InvalidOperationException(resman.GetString("Exception_ConnOpen"));
            }

            bool                 ForcedProtocolVersion = false;
            ProtocolVersion      PV;

            // Check if there is any missing argument.
            if (connection_string_values[CONN_SERVER] == null)
                throw new ArgumentException(resman.GetString("Exception_MissingConnStrArg"), CONN_SERVER);
            if (connection_string_values[CONN_USERID] == null)
                throw new ArgumentException(resman.GetString("Exception_MissingConnStrArg"), CONN_USERID);

            // If ConnectionString specifies a protocol version, we will 
            // not try to fall back to version 2 on failure.
            if (connection_string_values.Contains(CONN_PROTOCOL)) {
                PV = ConnectStringValueToProtocolVersion(CONN_PROTOCOL);
                ForcedProtocolVersion = true;
            } else {
                PV = ProtocolVersion.Version3;
            }

            lock(ConnectorPool.ConnectorPoolMgr)
            {
                _connector = ConnectorPool.ConnectorPoolMgr.
                    RequestConnector (ConnectionString, MaxPoolSize, ConnectionTimeout, false);
            }

            if (_connector == null) {
                throw new Exception("Internal: Unable to obtain a connector from the connection pool.");
            }

            if (! _connector.IsInitialized)
            {
                _connector.Encoding = Encoding.Default;
                _connector.BackendProtocolVersion = PV;

                try {
                    // Reset state to initialize new connector in pool.
                    CurrentState = NpgsqlClosedState.Instance;

                    CurrentState.Open(this);

                    // Change the state of connection to open.
                    connection_state = ConnectionState.Open;

                    // Check if there were any errors.
                    if (_mediator.Errors.Count > 0 && ! ForcedProtocolVersion)
                    {
                        // Check if there is an error of protocol not supported...
                        // As the message can be localized, just check the initial unlocalized part of the
                        // message. If it is an error other than protocol error, when connecting using
                        // version 2.0 we shall catch the error again.
                        if (((NpgsqlError)_mediator.Errors[0]).Message.StartsWith("FATAL"))
                        {
                            // Try using the 2.0 protocol.
                            _mediator.ResetResponses();
                            CurrentState = NpgsqlClosedState.Instance;
                            _connector.BackendProtocolVersion = ProtocolVersion.Version2;
                            CurrentState.Open(this);
                        }
                    }

                    // Check for errors and do the Right Thing.
                    CheckErrors();

                    String       ServerVersionString = String.Empty;

                    backend_keydata = _mediator.BackendKeyData;

                    // First try to determine backend server version using the newest method.
                    try {
                        ServerVersionString = ((NpgsqlParameterStatus)_mediator.Parameters["__npgsql_server_version"]).ParameterValue;
                    } catch {}

                    // Fall back to the old way, SELECT VERSION().
                    // This should not happen for protocol version 3+.
                    if (ServerVersionString.Length == 0)
                    {
                        NpgsqlCommand command = new NpgsqlCommand("select version();set DATESTYLE TO ISO;", this);
                        ServerVersionString = ExtractServerVersion( (String)command.ExecuteScalar() );
                    }

                    // Cook version string so we can use it for enabling/disabling things based on
                    // backend version.
                    _connector.ServerVersion = ParseServerVersion(ServerVersionString);

                    // Adjust client encoding.

                    //NpgsqlCommand commandEncoding1 = new NpgsqlCommand("show client_encoding", this);
                    //String clientEncoding1 = (String)commandEncoding1.ExecuteScalar();

                    if (ConnectStringValueToString(CONN_ENCODING, DEF_ENCODING).ToUpper() == "UNICODE")
                    {
                        _connector.Encoding = Encoding.UTF8;
                        NpgsqlCommand commandEncoding = new NpgsqlCommand("SET CLIENT_ENCODING TO UNICODE", this);
                        commandEncoding.ExecuteNonQuery();
                    }
                }
                catch {
                    Close();
                    throw;
                }
            }

            // Connector was obtained from pool.
            // Do a mini initialization in the state machine.

            _connector.IsInitialized = true;
            connection_state = ConnectionState.Open;

            CurrentState = NpgsqlReadyState.Instance;

            ProcessServerVersion();
            _oidToNameMapping = NpgsqlTypesHelper.LoadTypesMapping(this);
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        public void Close()
        {
            Dispose(true);
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand">IDbCommand</see>
        /// object associated with the <see cref="System.Data.IDbConnection">IDbConnection</see>.
        /// </summary>
        /// <returns>A <see cref="System.Data.IDbCommand">IDbCommand</see> object.</returns>
        IDbCommand IDbConnection.CreateCommand()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbConnection.CreateCommand");
            return (NpgsqlCommand) CreateCommand();
        }

        /// <summary>
        /// Creates and returns a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>
        /// object associated with the <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> object.</returns>
        public NpgsqlCommand CreateCommand()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateCommand");
            return new NpgsqlCommand("", this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Only if explicitly calling Close or dispose we still have access to
                // managed resources.
                NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose", disposing);

                try
                {
                    if ((connection_state == ConnectionState.Open))
                    {
                        CurrentState.Close(this);
                    }

                }
                catch {}

                lock(ConnectorPool.ConnectorPoolMgr)
                {
                    ConnectorPool.ConnectorPoolMgr.ReleaseConnector(_connector);
                }

                connection_state = ConnectionState.Closed;
            }
            base.Dispose (disposing);
        }


        /// <summary>
        /// Create a new (unconnected) connection based on this one.
        /// </summary>
        /// <returns>A new NpgsqlConnction object.</returns>
        public Object Clone()
        {
            return new NpgsqlConnection(ConnectionString);
        }


        //         
        // Private util methods
        //

        /// <summary>
        /// This method parses a connection string.
        /// It translates it to a list of key-value pairs.
        /// </summary>
        private ListDictionary ParseConnectionString(String CS)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ParseConnectionString");

            ListDictionary new_values = new ListDictionary(CaseInsensitiveComparer.Default);
            String[] pairs;
            String[] keyvalue;

            // Get the key-value pairs delimited by CONN_DELIM
            pairs = CS.Split(new Char[] {CONN_DELIM});

            // Now, for each pair, get its key-value.
            foreach(String sraw in pairs)
            {
                String s = sraw.Trim();
                String Key = "", Value = "";

                // This happens when there are trailing/empty CONN_DELIMs
                // Just ignore them.
                if (s == "") {
                    continue;
                }

                // Split this chunk on the first CONN_ASSIGN only.
                keyvalue = s.Split(new Char[] {CONN_ASSIGN}, 2);

                // Keys always get trimmed and uppercased.
                Key = keyvalue[0].Trim().ToUpper();

                // Make sure the key is even there...
                if (Key.Length == 0) {
                    throw new ArgumentException(resman.GetString("Exception_WrongKeyVal"), "<BLANK>");
                }

                // We don't expect keys this long, and it might be about to be put
                // in an error message, so makes sure it is a sane length.
                if (Key.Length > 20) {
                    Key = Key.Substring(0, 20);
                }

                // Check if there is a key-value pair.
                if (keyvalue.Length != 2) {
                    throw new ArgumentException(resman.GetString("Exception_WrongKeyVal"), Key);
                }

                // Values always get trimmed.
                Value = keyvalue[1].Trim();

                // Do some ODBC related substitions
                if (Key == ODBC_USERID) {
                    Key = CONN_USERID;
                } else if (Key == ODBC_PASSWORD) {
                    Key = CONN_PASSWORD;
                }

                NpgsqlEventLog.LogMsg(resman, "Log_ConnectionStringValues", LogLevel.Debug, Key, Value);

                // Add the pair to the dictionary..
                new_values.Add(Key, Value);
            }

            return new_values;
        }

        /// <summary>
        /// This method takes a version string as returned by SELECT VERSION() and returns
        /// a valid version string ("7.2.2" for example).
        /// This is only needed when running protocol version 2.
        /// This does not do any validity checks.
        /// </summary>
        private string ExtractServerVersion (string VersionString)
        {
            Int32               Start = 0, End = 0;

            // find the first digit and assume this is the start of the version number
            for ( ; Start < VersionString.Length && ! char.IsDigit(VersionString[Start]) ; Start++);

            End = Start;

            // read until hitting whitespace, which should terminate the version number
            for ( ; End < VersionString.Length && ! char.IsWhiteSpace(VersionString[End]) ; End++);

            return VersionString.Substring(Start, End - Start + 1);
        }

        /// <summary>
        /// This method takes a version string ("7.4.1" for example) and produces
        /// the required integer version numbers (7, 4, and 1).
        /// </summary>
        private ServerVersion ParseServerVersion (string VersionString)
        {
            String[]        Parts;

            Parts = VersionString.Split('.');

            if (Parts.Length == 2) {
                // Check if it is a devel version.
                if (Parts[1].ToLower().EndsWith("devel")) {
										Parts[1].Remove(Parts[1].Length - 5, 5);
                }

                // Coerce it into a 3-part version.
                Parts = new String[] { Parts[0], Parts[1], "0" };
            } else if (Parts.Length != 3) {
                throw new FormatException(String.Format("Internal: Backend sent bad version string: {0}", VersionString));
            }

            try {
                return new ServerVersion(Convert.ToInt32(Parts[0]), Convert.ToInt32(Parts[1]), Convert.ToInt32(Parts[2]));
            } catch (Exception E) {
                throw new FormatException(String.Format("Internal: Backend sent bad version string: {0}", VersionString), E);
            }
        }


        /// <summary>
        /// This method is required to set all the version dependent features flags.
        /// SupportsPrepare means the server can use prepared query plans (7.3+)
        /// </summary>
        private void ProcessServerVersion ()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessServerVersion");

            SupportsPrepare = ServerVersion.GreaterOrEqual(7, 3, 0);
        }

        /// <summary>
        /// The network stream connected to the backend.
        /// This can only be called when there is an active connection.
        /// </summary>
        internal Stream Stream {
            get
            {
                return _connector.Stream;
            }
        }

        /// <summary>
        /// The connector object connected to the backend.
        /// </summary>
        internal Connector Connector
        {
            get
            {
                return _connector;
            }
        }

        /// <summary>
        /// Check for mediator errors (sent by backend) and throw the appropriate
        /// exception if errors found.  This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckErrors()
        {
            if (_mediator.Errors.Count > 0) {
                throw new NpgsqlException(_mediator.Errors);
            }
        }

        /// <summary>
        /// Check for notifications and fire the appropiate events.
        /// This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckNotifications()
        {
            if (Notification != null) {
                foreach (NpgsqlNotificationEventArgs E in _mediator.Notifications) {
                    Notification(this, E);
                }
            }
        }

        /// <summary>
        /// Check for errors AND notifications in one call.
        /// </summary>
        internal void CheckErrorsAndNotifications()
        {
            CheckErrors();
            CheckNotifications();
        }

        // State
        internal void Query (NpgsqlCommand queryCommand)
        {
            CurrentState.Query(this, queryCommand );
        }

        internal void Authenticate (string password)
        {
            CurrentState.Authenticate(this, password );
        }

        internal void Startup ()
        {
            CurrentState.Startup(this);
        }

        internal void Parse (NpgsqlParse parse)
        {
            CurrentState.Parse(this, parse);
        }

        internal void Flush ()
        {
            CurrentState.Flush(this);
        }

        internal void Sync ()
        {
            CurrentState.Sync(this);
        }

        internal void Bind (NpgsqlBind bind)
        {
            CurrentState.Bind(this, bind);
        }

        internal void Execute (NpgsqlExecute execute)
        {
            CurrentState.Execute(this, execute);
        }


        /// <summary>
        /// Default SSL Callback implementation.
        /// </summary>
        internal Boolean DefaultCertificateValidationCallback(
            X509Certificate certificate,
            int[]        certificateErrors)
        {
            return true;
        }

        internal NpgsqlState CurrentState {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        internal NpgsqlBackEndKeyData BackEndKeyData {
            get
            {
                return backend_keydata;
            }
            set
            {
                backend_keydata = value;
            }
        }

        /// <summary>
        /// Backend server host name.
        /// </summary>
        internal String ServerName {
            get
            {
                return ConnectStringValueToString(CONN_SERVER);
            }
        }

        /// <summary>
        /// Backend server port.
        /// </summary>
        internal Int32 ServerPort {
            get
            {
                return ConnectStringValueToInt32(CONN_PORT, DEF_PORT);
            }
        }

        /// <summary>
        /// Backend database name.
        /// </summary>
        internal String DatabaseName {
            get
            {
                return ConnectStringValueToString(CONN_DATABASE, UserName);
            }
        }

        /// <summary>
        /// User name.
        /// </summary>
        internal String UserName {
            get
            {
                return ConnectStringValueToString(CONN_USERID);
            }
        }

        /// <summary>
        /// Password.
        /// </summary>
        internal String Password {
            get
            {
                return ConnectStringValueToString(CONN_PASSWORD);
            }
        }

        /// <summary>
        /// If true, the connection will attempt to use SSL.
        /// </summary>
        internal Boolean SSL {
            get
            {
                return ConnectStringValueToBool(CONN_SSL_ENABLED);
            }
        }

        /// <summary>
        /// Client encoding currently in use.
        /// This can only be called when there is an active connection.
        /// </summary>
        internal Encoding Encoding {
            get
            {
                return _connector.Encoding;
            }
        }

        /// <summary>
        /// The connection mediator.
        /// </summary>
        internal NpgsqlMediator	Mediator {
            get
            {
                return _mediator;
            }
        }

        /// <summary>
        /// Report if the connection is in a transaction.
        /// </summary>
        internal Boolean InTransaction {
            get
            {
                return _inTransaction;
            }
            set
            {
                _inTransaction = value;
            }
        }

        /// <summary>
        /// Report whether the current connection can support prepare functionality.
        /// </summary>
        internal Boolean SupportsPrepare {
            get
            {
                return _supportsPrepare;
            }
            set
            {
                _supportsPrepare = value;
            }
        }

        /// <summary>
        /// Version of the PostgreSQL backend.
        /// This can only be called when there is an active connection.
        /// </summary>
        internal ServerVersion ServerVersion {
            get
            {
                return _connector.ServerVersion;
            }
        }

        internal Hashtable OidToNameMapping {
            get
            {
                return _oidToNameMapping;
            }
            set
            {
                _oidToNameMapping = value;
            }

        }

        /// <summary>
        /// Protocol version in use.
        /// This can only be called when there is an active connection.
        /// </summary>
        internal ProtocolVersion BackendProtocolVersion {
            get
            {
                return _connector.BackendProtocolVersion;
            }
        }

        internal Int32 MinPoolSize {
            get
            {
                return ConnectStringValueToInt32(MIN_POOL_SIZE, DEF_MIN_POOL_SIZE);
            }
        }

        internal Int32 MaxPoolSize {
            get
            {
                return ConnectStringValueToInt32(MAX_POOL_SIZE, DEF_MAX_POOL_SIZE);
            }
        }

        /// <summary>
        /// Return a string value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// </summary>
        internal String ConnectStringValueToString(String Key)
        {
            return ConnectStringValueToString(Key, "");
        }

        /// <summary>
        /// Return a string value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// </summary>
        internal String ConnectStringValueToString(String Key, String Default)
        {
            if (! connection_string_values.Contains(Key)) {
                return Default;
            }

            return Convert.ToString(connection_string_values[Key]);
        }

        /// <summary>
        /// Return an integer value from the current connection string, even if the
        /// given key is not in the string or if the value is null.
        /// Throw an appropriate exception if the value cannot be coerced to an integer.
        /// </summary>
        internal Int32 ConnectStringValueToInt32(String Key)
        {
            return ConnectStringValueToInt32(Key, 0);
        }

        /// <summary>
        /// Return an integer value from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value cannot be coerced to an integer.
        /// </summary>
        internal Int32 ConnectStringValueToInt32(String Key, Int32 Default)
        {
            if (! connection_string_values.Contains(Key)) {
                return Default;
            }

            try {
                return Convert.ToInt32(connection_string_values[Key]);
            } catch (Exception E) {
                throw new ArgumentException(resman.GetString("Exception_InvalidIntegerKeyVal"), Key, E);
            }
        }

        /// <summary>
        /// Return a boolean value from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value is not recognized as a boolean.
        /// </summary>
        internal Boolean ConnectStringValueToBool(String Key)
        {
            return ConnectStringValueToBool(Key, false);
        }

        /// <summary>
        /// Return a boolean value from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value is not recognized as a boolean.
        /// </summary>
        internal Boolean ConnectStringValueToBool(String Key, Boolean Default)
        {
            if (! connection_string_values.Contains(Key)) {
                return Default;
            }

            switch (connection_string_values[Key].ToString().ToLower()) {
            case "t" :
            case "true" :
            case "y" :
            case "yes" :
                return true;

            case "f" :
            case "false" :
            case "n" :
            case "no" :
                return false;

            default :
                throw new ArgumentException(resman.GetString("Exception_InvalidBooleanKeyVal"), Key);

            }
        }

        /// <summary>
        /// Return a ProtocolVersion from the current connection string, even if the
        /// given key is not in the string.
        /// Throw an appropriate exception if the value is not recognized as
        /// integer 2 or 3.
        /// </summary>
        private ProtocolVersion ConnectStringValueToProtocolVersion(String Key)
        {
            if (! connection_string_values.Contains(Key)) {
                return ProtocolVersion.Version3;
            }

            switch (ConnectStringValueToInt32(Key)) {
            case 2 :
                return ProtocolVersion.Version2;

            case 3 :
                return ProtocolVersion.Version3;

            default :
                throw new ArgumentException("Invalid protocol version specified in ConnectionString", Key);

            }
        }
    }
}
