
//	ConnectorPool.cs
// ------------------------------------------------------------------
//	History
//		06/19/2002 usp - Introduction of collection compliant 
//			connector lists based on the LinkedList class.
//		06/20/2002 usp - 
//			RequestConnector(), bug removed: mShared property not set 
//			properly.
//			RequestConnector(), bug removed: Pooled connectors should
//			also be searched if a shared connector is requested.
//	Status
//		0.00.0003 - 06/20/2002 - ulrich sprick - edits
//		

using System;

namespace Npgsql
{
	/// <summary>
	/// The ConnectorPool class implements the functionality for 
	/// the administration of the connectors. Controls pooling and
	/// sharing of connectors.
	/// </summary>
	/// <status>test</status>
	
	internal class ConnectorPool
	{
		/// <value>Unique static instance of the connector pool
		/// mamager.</value>
		
		internal static ConnectorPool ConnectorPoolMgr 
			= new Npgsql.ConnectorPool();

		/// <value>List of unused, pooled connectors avaliable to the
		/// next RequestConnector() call.</value>
		
		internal Npgsql.ConnectorList PooledConnectors;

		/// <value>List of shared, in use conncetors.</value>
		
		internal Npgsql.ConnectorList SharedConnectors;
		
		/// <value>Returns the internal Shared Connectors list !!! for debug only</value>
		
		public Npgsql.ConnectorList DebugSharedConnectors
		{
			get { return this.SharedConnectors; }
		}
		
		/// <value>Returns the internal Pooled Connectors list !!! for debug only</value>
		
		public Npgsql.ConnectorList DebugPooledConnectors
		{
			get { return this.PooledConnectors; }
		}

		/// <summary>
		/// Default constructor, creates a new connector pool object.
		/// Should only be used once in an application, since more 
		/// than one connector pool does not make much sense..
		/// </summary>
		/// <status>test</status>
		
		internal ConnectorPool()
		{
			this.PooledConnectors = new Npgsql.ConnectorList();
			this.SharedConnectors = new Npgsql.ConnectorList();
		}
		
		/// <summary>
		/// Searches the shared and pooled connector lists for a
		/// matching connector object or creates a new one.
		/// </summary>
		/// <param name="ConnectString">used to connect to the
		/// database server</param>
		/// <param name="Shared">Allows multiple connections
		/// on a single connector. </param>
		/// <returns>A pooled connector object.</returns>
		/// <status>test</status>
		/// <revised>06/20/2002</revised> <by>usp</by>
		
		internal Npgsql.Connector RequestConnector ( 
			string ConnectString,
			bool Shared )
		{
			// if a shared connector is requested then the Shared
			// Connector List is searched first:

			if ( Shared )
			{
				foreach( Npgsql.Connector Connector in this.SharedConnectors )
				{
					if ( Connector.ConnectString == ConnectString )
					{	// Bingo!
						// Return the shared connector to caller.
						// The connector is already in use.
						Connector.mShareCount++;
						return Connector;
					}
				}
			}

			// if a shared connector could not be found or a
			// nonshared connector is requested, then the pooled
			// (unused) connectors are beeing searched.

			foreach( Npgsql.Connector Connector in this.PooledConnectors )
			{
				if ( Connector.ConnectString == ConnectString )
				{	// Bingo!
					// Remove the Connector from the pooled connectors list.
					this.PooledConnectors.Remove( Connector );
					// Make the connector shared if requested					
					if ( Connector.Shared = Shared )
					{
						this.SharedConnectors.Add( Connector );
						Connector.mShareCount = 1;
					}
					// done...
					Connector.InUse = true;
					return Connector;
				}
			}

			// No suitable connector found, so create new one
			
			Npgsql.Connector NewConnector = new Npgsql.Connector( 
				ConnectString, Shared 
				);

			// Shared connections must be added to the shared 
			// connectors list
			if ( Shared )
			{
				this.SharedConnectors.Add( NewConnector );
				NewConnector.mShareCount = 1;
			}

			// and then returned to the caller
			NewConnector.InUse = true;
			NewConnector.Open();
			return NewConnector;
		}
	}
}
