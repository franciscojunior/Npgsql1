using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using ItcCodeLib;

namespace ItcSocketTest
{
	/// <summary>
	/// The Listener class listens at the specified tcp port for incoming 
	/// connections. If a client connects to the port, a new Session object
	/// is instanciated for each client to handle the communication protocol.
	/// <br/><b>Behaviour:</b> After the constructor has set up listener socket,
	/// a call to Listen() opens the port. An incoming connection is handled
	/// in the BeginAcceptCallback() method: a new Session gets created and
	/// inserted into the Sessions list. Thereafter the callback calls the 
	/// Listen() method again in order to allow more clients to connect.
	/// </summary>
	
	public class Listener
	{
		/// <summary>
		/// PortNumber at which the server listens for incoming connections. 
		/// Default port is 11000.
		/// </summary>
		
		public int PortNumber = 11000;
		
		/// <summary>
		/// The listener socket waits for incoming connections.
		/// </summary>
		
		private Socket Socket = new Socket( AddressFamily.InterNetwork, 
			SocketType.Stream, ProtocolType.Tcp );
				
		/// <summary>
		/// The list of sessions for the connected clients 
		/// </summary>
		
		private ItcCodeLib.LinkedList sessions = new ItcCodeLib.LinkedList();
		
		/// <summary>
		///	Creates the Socket and binds it to the endpoint.
		/// </summary>
		
		public Listener()
		{
			Console.WriteLine("Listener: Constructor");
		}
		
		/// <summary>
		/// Starts listening at the endpoint for incoming connections.
		/// </summary>
		
		public void Start()
		{
			try // Start work: Begin accept
			{
				// create a new socket if required
				if ( this.Socket == null )
				{
					this.Socket = new Socket( AddressFamily.InterNetwork, 
						SocketType.Stream, ProtocolType.Tcp );
				}
				// Get Host IP Address and setup server endpoint
				IPHostEntry LocalHostInfo = Dns.Resolve( Dns.GetHostName() );
				IPAddress LocalIpAddress = LocalHostInfo.AddressList[ 0 ];
				IPEndPoint EndPoint = new IPEndPoint( LocalIpAddress, this.PortNumber );
				Console.WriteLine("Server: Listening" );
				Console.WriteLine("   Hostname:  {0}", Dns.GetHostName());
				Console.WriteLine("   End point: {0}", EndPoint.ToString());
				// Bind to endpoint
				this.Socket.Bind( EndPoint );
				this.Socket.Listen( 100 );
				this.Socket.BeginAccept( new System.AsyncCallback( AcceptCallback ), this );
				Console.WriteLine("   Waiting for incoming connections...");
			} 
			catch ( SocketException E ) 
			{
				Console.WriteLine( "SocketException : {0}", E.ToString() );
			} 
			catch ( ObjectDisposedException E )
			{
				Console.WriteLine( "Object DisposedException : {0}", E.ToString() );
			}
			catch ( Exception E )  
			{
				Console.WriteLine( "Unexpected exception : {0}", E.ToString());
			}
		}
		
		/// <summary>
		/// Gets called when a client connects to the server. A new socket
		/// is created for the client communication.
		/// </summary>
		/// <param name="AResult"></param>
		/// <remarks>
		///	If the listener has been stopped before, an access to the 
		///	EndAccept() method will throw an ObjectDisposedException.
		/// </remarks>
		
		private void AcceptCallback( IAsyncResult AResult )
		{
			try 
			{
				// Get a new socket for the session
				Socket SessionSocket = this.Socket.EndAccept( AResult );
				// Create a new session controller and start a new session
				ItcSocketTest.Session NewSession = new ItcSocketTest.Session(); 
				NewSession.Socket = SessionSocket;
				this.sessions.Add( NewSession );
				Console.WriteLine("Listener: connection accepted, starting new session");
				NewSession.Start();
			}
			catch ( ArgumentNullException E )
			{
				Console.WriteLine( "Argument null exception: {0}", E.ToString() );
			}
			catch ( ArgumentException E )
			{
				Console.WriteLine( "Argument exception: {0}", E.ToString() );
			}
			catch ( SocketException E )
			{
				Console.WriteLine( "Socket exception: {0}", E.ToString() );
			}
			catch ( ObjectDisposedException E )
			{
				Console.WriteLine( "Listener.AcceptCallback: Socket has been disposed"  );
				// the socket has been closed, so it is unusable. Clear the
				// reference to it to get the socket garbage collected.
				this.Socket = null;
			}
			catch ( System.Exception E )
			{
				Console.WriteLine( "System exception: {0}", E.ToString() );	
			}
		}
		
		/// <summary>
		/// Stops the server listening for incoming connections.
		/// </summary>
		
		public void Stop()
		{
			Console.WriteLine("Listener.Stop: Stopping the server" );
			if ( this.Socket.Connected ) 
			{
				this.Socket.Shutdown( SocketShutdown.Both );
			}
			this.Socket.Close();
		}
	}
}
