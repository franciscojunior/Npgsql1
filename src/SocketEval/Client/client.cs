using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace ItcSocketTest
{
	/// <summary>
	/// Zusammendfassende Beschreibung für client.
	/// </summary>
	
	public class Client
	{
		/// <summary>
		/// The socket for communication with the server
		/// </summary>
		
		private Socket socket = new System.Net.Sockets.Socket( AddressFamily.InterNetwork,	
			SocketType.Stream, ProtocolType.Tcp );
		
		/// <summary>
		/// Name of the host to connect to.
		/// </summary>
		
		public string HostName = Dns.GetHostName();
		
		/// <summary>
		/// Port number for connecting to remote end point. Default is 11000.
		/// </summary>
		
		public int PortNumber = 11000;
		
		/// <summary>
		/// Reflects the connection state
		/// </summary>
		
		private enum SocketState { Disconnected, Connecting, Connected };
		SocketState State = SocketState.Disconnected;
		
		/// <summary>
		/// The default constructor.
		/// </summary>
		
		public Client()
		{
			return;
		}
		
		/// <summary>
		/// Starts a session with a server
		/// </summary>
		
		public void Connect()
		{
			// Setup a remote end point for the socket
			IPHostEntry HostInfo = Dns.Resolve( this.HostName );
			IPAddress IpAddress = HostInfo.AddressList[0];
			IPEndPoint RemoteEP = new IPEndPoint( IpAddress, this.PortNumber );
			// create a new socket if required
			if ( this.socket == null )
			{
				Console.WriteLine("Client.Connect: Creating new socket" );
				this.socket = new System.Net.Sockets.Socket( AddressFamily.InterNetwork,	
					SocketType.Stream, ProtocolType.Tcp 
				);
			}
			// Connect to the remote endpoint.
			Console.WriteLine( "Client.Connect: Connecting" );
			this.socket.BeginConnect( RemoteEP, new AsyncCallback( this.ConnectCallback ), 
				this 
				);
			this.State = SocketState.Connecting;
		}
		
		/// <summary>
		/// Gets called when the connection is completed.
		/// </summary>
		/// <param name="AResult"></param>
		
		private void ConnectCallback( IAsyncResult AResult ) 
		{
			try 
			{
				// complete connect procedure
				Console.Write("Client.ConnectCallback: ");
				this.socket.EndConnect( AResult );
                Console.WriteLine("    Connected to {0}", this.socket.RemoteEndPoint.ToString()	);
				// reflect connection state
				this.State = SocketState.Connected;
			} 
			catch ( ObjectDisposedException e )
			{ 
				Console.WriteLine(e.ToString()); 
			}
			catch ( InvalidOperationException e )
			{ 
				Console.WriteLine(e.ToString()); 
			}
			catch ( SocketException e )
			{ 
				// The server is not listening
				Console.WriteLine(e.ToString()); 
			}
			catch ( Exception e )
			{ 
				Console.WriteLine(e.ToString()); 
			}
		}

		/// <summary>
		/// Send a message to the remote end point.
		/// </summary>
		/// <param name="Message">the message to be sent</param>

		public void Send( string Message )
		{
			// prevent illegal usage
			if ( this.State != SocketState.Connected ) return;
			// Convert the string data to byte data using ASCII encoding.
			byte[] ByteData = Encoding.ASCII.GetBytes( Message );
			// initiate send
			this.socket.BeginSend( ByteData, 0, ByteData.Length, 0,
				new AsyncCallback( this.SendCallback ), this
			);
		}
		
		private void SendCallback( IAsyncResult AResult ) 
		{
			try 
			{
				// check parameter
				if ( AResult.AsyncState == this ) Console.WriteLine("Client.SendCallback: They are equal...");
				// complete sending
				int BytesSent = this.socket.EndSend( AResult );
				Console.WriteLine("Sent {0} bytes to server.", BytesSent );
			} 
			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}
		}

	}
}
