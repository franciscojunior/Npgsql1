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
//
//	Connector.cs
// ------------------------------------------------------------------
//	Project
//		Npgsql
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - created
//		          - 06/??/2004 - Glen Parker<glenebob@nwlink.com> rewritten

using System;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Data;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls;

using NpgsqlTypes;

namespace Npgsql
{
    /// <summary>
    /// !!! Helper class, for compilation only.
    /// Connector implements the logic for the Connection Objects to
    /// access the physical connection to the database, and isolate
    /// the application developer from connection pooling internals.
    /// </summary>
    internal class NpgsqlConnector
    {
        // Used to obtain a current key for the non-shared pool.
        // FIXME - this needs to be removed from this class
        internal NpgsqlConnection                Connection;

        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        internal event NotificationEventHandler  Notification;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateValidationCallback delegate.
        /// </summary>
        internal event CertificateValidationCallback    CertificateValidationCallback;


        // FIXME - these two need to be turned into events and handled properly
        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateSelectionCallback delegate.
        /// </summary>
        internal CertificateSelectionCallback    CertificateSelectionCallback;

        /// <summary>
        /// Mono.Security.Protocol.Tls.PrivateKeySelectionCallback delegate.
        /// </summary>
        internal PrivateKeySelectionCallback     PrivateKeySelectionCallback;

        // FIXME - should be private
        internal ConnectionState                 _connection_state;

        // The physical network connection to the backend.
        private Stream                           _stream;

        // Mediator which will hold data generated from backend.
        internal NpgsqlMediator                  _mediator;

        private ProtocolVersion                   _backendProtocolVersion;
        private ServerVersion                     _serverVersion;

        // Values for possible CancelRequest messages.
        // FIXME - should be private
        internal NpgsqlBackEndKeyData            _backend_keydata;

        // Flag for transaction status.
        // FIXME - should be private
        internal Boolean                         _inTransaction = false;

        // FIXME - should be private
        internal Boolean                         _supportsPrepare = false;

        // FIXME - should be private
        internal Hashtable                       _oidToNameMapping;

        private Encoding                         _encoding;

        private Boolean                          _isInitialized;

        private Boolean                          _shared;

        // FIXME - should be private
        internal NpgsqlState                      _state;

        // FIXME - these all need to be properties
        internal String                           Host;
        internal Int32                            Port;
        internal String                           Database;
        internal String                           UserName;
        internal String                           Password;
        internal bool                             SSL;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Shared">Controls whether the connector can be shared.</param>
        public NpgsqlConnector(String Host, Int32 Port, String Database, String UserName, String Password, bool SSL, bool Shared)
        {
            _connection_state = ConnectionState.Closed;
            _shared = Shared;
            _isInitialized = false;
            _state = NpgsqlClosedState.Instance;
            _mediator = new NpgsqlMediator();
            _oidToNameMapping = new Hashtable();

            this.Host = Host;
            this.Port = Port;
            this.Database = Database;
            this.UserName = UserName;
            this.Password = Password;
            this.SSL = SSL;
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        internal ConnectionState State {
            get
            {
                return _connection_state;
            }
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

        /// <summary>
        /// Default SSL CertificateValidationCallback implementation.
        /// </summary>
        internal bool DefaultCertificateValidationCallback(
            X509Certificate       certificate,
            int[]                 certificateErrors)
        {
            if (CertificateValidationCallback != null) {
                return CertificateValidationCallback(certificate, certificateErrors);
            } else {
                return true;
            }
        }

        /// <summary>
        /// Version of backend server this connector is connected to.
        /// </summary>
        internal ServerVersion ServerVersion
        {
            get
            {
                return _serverVersion;
            }
            set
            {
                _serverVersion = value;
            }
        }

        internal Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        /// <summary>
        /// Backend protocol version in use by this connector.
        /// </summary>
        internal ProtocolVersion BackendProtocolVersion
        {
            get
            {
                return _backendProtocolVersion;
            }
            set
            {
                _backendProtocolVersion = value;
            }
        }

        /// <summary>
        /// The physical connection stream to the backend.
        /// </summary>
        internal Stream Stream {
            get
            {
                return _stream;
            }
            set
            {
                _stream = value;
            }
        }

        /// <summary>
        /// Reports if this connector is fully connected.
        /// </summary>
        internal Boolean IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            set
            {
                _isInitialized = value;
            }
        }

        internal NpgsqlState CurrentState {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }


        /// <value>Reports whether this connector can be shared.</value>
        /// <remarks>Set true if this connector is shared among multiple
        /// connections.</remarks>
        internal bool Shared
        {
            get
            {
                return _shared;
            }
        }

        internal NpgsqlBackEndKeyData BackEndKeyData {
            get
            {
                return _backend_keydata;
            }
        }

        internal Hashtable OidToNameMapping {
            get
            {
                return _oidToNameMapping;
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
        /// This method is required to set all the version dependent features flags.
        /// SupportsPrepare means the server can use prepared query plans (7.3+)
        /// </summary>
        // FIXME - should be private
        internal void ProcessServerVersion ()
        {
            this._supportsPrepare = (ServerVersion >= new ServerVersion(7, 3, 0));
        }

        /// <value>Counts the numbers of Connections that share
        /// this Connector. Used in Release() to decide wether this
        /// connector is to be moved to the PooledConnectors list.</value>
        // internal int mShareCount;

        /// <summary>
        /// Opens the physical connection to the server.
        /// </summary>
        /// <remarks>Usually called by the RequestConnector
        /// Method of the connection pool manager.</remarks>
        internal void Open(ProtocolVersion PV)
        {
            Encoding = Encoding.Default;
            BackendProtocolVersion = (PV == ProtocolVersion.Unknown) ? ProtocolVersion.Version3 : PV;

            // Reset state to initialize new connector in pool.
            CurrentState = NpgsqlClosedState.Instance;

            CurrentState.Open(this);

            // Check for protocol not supported.  If we have been told what protocol to use,
            // we will not try this step.
            if (_mediator.Errors.Count > 0 && PV == ProtocolVersion.Unknown)
            {
                // If we attempted protocol version 3, it may be possible to drop back to version 2.
                if (BackendProtocolVersion == ProtocolVersion.Version3) {
                    NpgsqlError       Error0 = (NpgsqlError)_mediator.Errors[0];

                    // If NpgsqlError.ReadFromStream_Ver_3() encounters a version 2 error,
                    // it will set its own protocol version to version 2.  That way, we can tell
                    // easily if the error was a FATAL: protocol error.
                    if (Error0.BackendProtocolVersion == ProtocolVersion.Version2)
                    {
                        // Try using the 2.0 protocol.
                        _mediator.ResetResponses();
                        BackendProtocolVersion = ProtocolVersion.Version2;
                        CurrentState = NpgsqlClosedState.Instance;
                        CurrentState.Open(this);
                    }
                }
            }

            // Check for errors and do the Right Thing.
            // FIXME - CheckErrors needs to be moved to Connector
            CheckErrors();

            _backend_keydata = _mediator.BackendKeyData;

            // Change the state of connection to open.
            _connection_state = ConnectionState.Open;

            String       ServerVersionString = String.Empty;

            // First try to determine backend server version using the newest method.
            try {
                ServerVersionString = ((NpgsqlParameterStatus)_mediator.Parameters["__npgsql_server_version"]).ParameterValue;
            } catch {}

            // Fall back to the old way, SELECT VERSION().
            // This should not happen for protocol version 3+.
            if (ServerVersionString.Length == 0)
            {
                NpgsqlCommand command = new NpgsqlCommand("select version();set DATESTYLE TO ISO;", this);
                ServerVersionString = PGUtil.ExtractServerVersion( (String)command.ExecuteScalar() );
            }

            // Cook version string so we can use it for enabling/disabling things based on
            // backend version.
            ServerVersion = PGUtil.ParseServerVersion(ServerVersionString);

            // Adjust client encoding.

            //NpgsqlCommand commandEncoding1 = new NpgsqlCommand("show client_encoding", _connector);
            //String clientEncoding1 = (String)commandEncoding1.ExecuteScalar();

            // FIXME - need to remove dependency on Connection

            if (Connection.ConnectStringValueToString(NpgsqlConnection.CONN_ENCODING, NpgsqlConnection.DEF_ENCODING).ToUpper() == "UNICODE")
            {
                Encoding = Encoding.UTF8;
                NpgsqlCommand commandEncoding = new NpgsqlCommand("SET CLIENT_ENCODING TO UNICODE", this);
                commandEncoding.ExecuteNonQuery();
            }


            _oidToNameMapping = NpgsqlTypesHelper.LoadTypesMapping(this);

            ProcessServerVersion();

            CurrentState = NpgsqlReadyState.Instance;

            // The connector is now fully initialized. Beyond this point, it is
            // safe to release it back to the pool.
            IsInitialized = true;
        }


        /// <summary>
        /// Closes the physical connection to the server.
        /// </summary>
        internal void Close()
        {
            // HACK HACK
            // There needs to be a cleaner way to close this thing...
            try {
                Stream.Close();
            } catch {}
        }
    }
}
