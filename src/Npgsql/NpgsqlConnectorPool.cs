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
//	ConnectorPool.cs
// ------------------------------------------------------------------
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - creation

using System;
using System.Collections;
using Npgsql;
using System.Threading;

namespace Npgsql
{
    internal class ConnectorPool
    {
        /// <value>Unique static instance of the connector pool
        /// mamager.</value>
        internal static ConnectorPool ConnectorPoolMgr = new Npgsql.ConnectorPool();

        public ConnectorPool()
        {
            PooledConnectors = new Hashtable();
        }


        /// <value>Map of index to unused pooled connectors, avaliable to the
        /// next RequestConnector() call.</value>
        /// <remarks>This hashmap will be indexed by connection string.
        /// This key will hold a list of queues of pooled connectors available to be used.</remarks>
        private Hashtable PooledConnectors;

        /// <value>Map of shared connectors, avaliable to the
        /// next RequestConnector() call.</value>
        /// <remarks>This hashmap will be indexed by connection string.
        /// This key will hold a list of shared connectors available to be used.</remarks>
        private Hashtable SharedConnectors;

        /// <summary>
        /// Searches the shared and pooled connector lists for a
        /// matching connector object or creates a new one.
        /// </summary>
        /// <param name="Connection">The NpgsqlConnection that is requesting
        /// the connector. Its ConnectionString will be used to search the
        /// pool for available connectors.</param>
        /// <param name="Timeout">How long to wait for a connection to
        /// become available. Currently unused.</param>
        /// <returns>A connector object.</returns>
        public Npgsql.Connector RequestConnector (NpgsqlConnection Connection)
        {
            Connector       Connector = null;
            Boolean         Shared = false;

            // If sharing were implemented, I suppose Shared would be set based
            // on some property on the Connection.

            if (! Shared) {
                Connector = GetPooledConnecter(Connection);
            } else {
                // Connection sharing? What's that?
                throw new NotImplementedException("Internal: Shared pooling not implemented");
            }

            return Connector;
        }

        /// <summary>
        /// Releases a connector, possibly back to the pool for future use.
        /// </summary>
        /// <comments>
        /// Pooled connectors will be put back into the pool if there is room.
        /// Shared connectors should just have their use count decremented
        /// since they always stay in the shared pool.
        /// </comments>
        /// <param name="Connector">The connector to release.</param>
        public void ReleaseConnector (Connector Connector)
        {
            if (! Connector.Shared) {
                if (Connector.Connection.MaxPoolSize == 0) {
                    // No way we can pool when MaxPoolSize == 0
                    Connector.Close();
                } else {
                    PoolConnector(Connector);
                }
            } else {
                // Connection sharing? What's that?
                throw new NotImplementedException("Internal: Shared pooling not implemented");
            }
        }

        /// <summary>
        /// Find an available pooled connector in the non-shared pool, or create
        /// a new one if none found.
        /// </summary>
        private Npgsql.Connector GetPooledConnecter(NpgsqlConnection Connection)
        {
            Queue           Queue;
            Connector       Connector;

            // Try to find a queue.
            Queue = (Queue)PooledConnectors[Connection.ConnectionString];

            if (Queue == null || Queue.Count == 0) {
                // No queue, or an empty one, found.  Make a new connector.
                Connector = new Connector(false);
                Connector.Connection = Connection;
                return Connector;
            }

            // Found a queue with connectors.  Grab the top one.
            Connector = (Connector)Queue.Dequeue();
            Connector.Connection = Connection;

            return Connector;
        }

        /// <summary>
        /// Find an available shared connector in the shared pool, or create
        /// a new one if none found.
        /// </summary>
        private Npgsql.Connector GetSharedConnecter(NpgsqlConnection Connection)
        {
            Connector       Connector;

            Connector = (Connector)SharedConnectors[Connection.ConnectionString];

            if (Connector == null) {
                Connector = new Connector(true);

                SharedConnectors[Connection.ConnectionString] = Connector;
            }

            return Connector;
        }

        /// <summary>
        /// Put a pooled connector (back?) into the pool queue.  Create the queue if needed.
        /// </summary>
        /// <param name="Connector">Connector to pool</param>
        private void PoolConnector(Connector Connector)
        {
            Queue           Queue;

            // Try to find a queue.
            Queue = (Queue)PooledConnectors[Connector.Connection.ConnectionString];

            if (Queue == null) {
                // No queue found. Make a new one.
                Queue = new Queue();
                PooledConnectors[Connector.Connection.ConnectionString] = Queue;
            }

            // If there is room, push the connector into the queue,
            // otherwise it must be closed and forgotten.
            if (Queue.Count < Connector.Connection.MaxPoolSize) {
                Queue.Enqueue(Connector);
            } else {
                Connector.Close();
                return;
            }
        }
    }
}
