
//	Connector.cs
// ------------------------------------------------------------------
//	Project
//		Npgsql
//	History
//		06/19/2002 usp - Connector derived from LinkedListItem for
//			keeping them in the ConnectorPool lists.
//		06/20/2002 usp - Instance counting, mShareCount access added 
//			for debugging purpose.
//	Status
//		0.00.0001 - 06/19/2002 - ulrich sprick - edits

using System;

namespace Npgsql
{
	/// <summary>
	/// !!! Helper class, for compilation only.
	/// </summary>

	internal class Socket
	{
		internal void Open() { return; }
		internal void Close() { return; }
	}

	/// <summary>
	/// The Connector class implements the logic for the Connection 
	/// Objects to access the physical connection to the database, and 
	/// isolate the application developer from connection pooling 
	/// internals.
	/// </summary>
	/// <modified>06/20/2002</modified> <by>usp</by>
	/// <status>test</status>
	
	
	internal class Connector : Npgsql.LinkedList.ListItem
	{
		/// <value>Instance Counting</value>
		/// <remarks>!!! for debugging only</remarks>
		
		private static int InstanceCounter;
		public int InstanceNumber;
		
		/// <value>Buffer for the public Pooled property</value>
		/// <remarks>Pooled will be ignored if Shared is set!</remarks>

		internal bool Pooled;

		/// <value>Buffer for the public Shared property</value>

		private bool mShared;

		/// <value>Controls the physical connection sharing.</value>
		/// <remarks>Can only be set via ConnectorPool.Request().</remarks>
		/// <status>test</status>

		internal bool Shared
		{
			get { return this.mShared; }
			set { if ( ! this.InUse ) this.mShared = value; }
		}

		/// <value>Counts the numbers of Connections that share
		/// this Connector. Used in Release() to decide wether this
		/// connector is to be moved to the PooledConnectors list.</value>

		internal int mShareCount;

		/// <value>Share count, read only</value>
		/// <remarks>!!! for debugging only</remarks>
		
		public int ShareCount 
		{
			get { return this.mShareCount; }
		}
		
		/// <value>Private Buffer for the connection string property.</value>
		/// <remarks>Compared to the requested connection string in the
		/// ConnectorPool.RequestConnector() function.
		/// Should not be modified if physical connection is open.</remarks>

		private string mConnectString;

		/// <summary>
		/// Used to connect to the database server. 
		/// </summary>
		/// <status>test</status>		

		public string ConnectString
		{
			get { return mConnectString; }
			set
			{
				if ( this.InUse ) // uuuuugh, bad habits...
				{
					throw new Npgsql.NpgsqlException( "Connection strings "
					+ " cannot be modified if connection is open." );
				}
				mConnectString = value;
			}
		}

		/// <value>Provides physical access to the server</value>
		// !!! to be fixed

		private Npgsql.Socket Socket;

		/// <value>True if the physical connection is in used 
		/// by a Connection Object. That is, if the connector is
		/// not contained in the PooledConnectors List.</value>
		
		internal bool InUse;

		/// <summary>
		/// Construcor, initializes the Connector object.
		/// </summary>
		/// <status>test</status>

		internal Connector( string ConnectString, bool Shared )
		{
			this.ConnectString = ConnectString;
			this.mShared = Shared;
			this.Pooled = true;
			Npgsql.Connector.InstanceCounter++;
			this.InstanceNumber = Npgsql.Connector.InstanceCounter;
		}

		/// <summary>
		/// Opens the physical connection to the server.
		/// </summary>
		/// <remarks>Usually called by the RequestConnector
		/// Method of the connection pool manager.</remarks>
		/// <status>test</status>

		internal void Open()
		{
			this.Socket = new Npgsql.Socket();
			this.Socket.Open(); // !!! to be fixed
		}

		/// <summary>
		/// Releases a connector back to the pool manager's garding. Or to the
		/// garbage collection.
		/// </summary>
		/// <remarks>The Shared and Pooled properties are no longer needed after
		/// evaluation inside this method, so they are left in their current state.
		///	They get new meaning again when the connector is requested from the
		/// pool manager later. </remarks>
		/// <status>test</status>

		public void Release()
		{
			if ( this.mShared )
			{
				// A shared connector is returned to the pooled connectors
				// list only if it is not used by any Connection object.
				// Otherwise the job is done by simply decrementing the
				// usage counter:
				if ( --this.mShareCount == 0 )
				{
					// Shared connectors are *always* pooled after usage.
					// Depending on the Pooled property at this point
					// might introduce a lot of trouble into an application...
					Npgsql.ConnectorPool.ConnectorPoolMgr.SharedConnectors.Remove( this );
					Npgsql.ConnectorPool.ConnectorPoolMgr.PooledConnectors.Add( this );
					this.Pooled = true;
					this.InUse = false;					
				}
			}
			else // it is a nonshared connector
			{
				if ( this.Pooled )
				{
					// Pooled connectors are simply put in the
					// PooledConnectors list for later recycling
					this.InUse = false;					
					Npgsql.ConnectorPool.ConnectorPoolMgr.PooledConnectors.Add( this );
				}
				else
				{
					// Unpooled private connectors get the physical
					// connection closed, they are *not* recyled later.
					// Instead they are (implicitly) handed over to the
					// garbage collection.
					// !!! to be fixed
					this.Socket.Close();
				}
			}
		}
	}
}
