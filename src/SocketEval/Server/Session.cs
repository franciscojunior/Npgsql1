using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using ItcCodeLib;

namespace ItcSocketTest
{
	/// <summary>
	/// The Session class implements a backend, that handles the communication protocol
	/// with the client.
	/// </summary>
	
	public class Session : ItcCodeLib.LinkedList.ListItem
	{
		/// <summary>
		/// The Socket is used for communicating with the connected client.
		/// </summary>
		
		public Socket Socket;
		
		/// <summary>
		/// The receiver buffer
		/// </summary>
		private const int buffersize = 1024; 
		private byte[] buffer = new byte[ Session.buffersize ];
		
		/// <summary>
		/// Holds the message from the client
		/// </summary>
		
		private StringBuilder stringbuilder = new StringBuilder();
		
		/// <summary>
		/// Default construktor.
		/// </summary>
		
		public Session()
		{
			Console.WriteLine( "Session: Constructor" );
			return;
		}
		
		/// <summary>
		/// Sets up a new session controller for operating the client-server protocol
		/// </summary>
		/// <param name="ServerSocket">The socket created in the accept procedure</param>
		
		public Session( System.Net.Sockets.Socket ServerSocket )
		{
			Console.WriteLine( "Session: Constructor" );
			// Store the parameters
			this.Socket = ServerSocket;
		}
		
		/// <summary>
		/// Starts up the session
		/// </summary>
		
		public void Start()
		{
			Console.WriteLine( "Session: Starting" );
			this.Socket.BeginReceive( this.buffer, 0, Session.buffersize , 0,
				new AsyncCallback( this.ReadCallback ), this 
			);		
		}
		
		/// <summary>
		/// Collects the data sent by the client.
		/// </summary>
		/// <param name="AResult"></param>
		
		private void ReadCallback( IAsyncResult AResult )
		{
			Console.WriteLine( "Session.ReadCallback" );			
			Session state = (Session) AResult.AsyncState;
			if ( state == this ) Console.WriteLine("Session.ReadCallback: They are equal...");			
			int BytesRead = this.Socket.EndReceive( AResult );
			if ( BytesRead > 0 )
			{
				// There  might be more data, so store  the data received so far.
				this.stringbuilder.Append( Encoding.ASCII.GetString( 
					this.buffer, 0, BytesRead ));
				// Check for end-of-file tag. If  it is not there, read more data.
				String Content = this.stringbuilder.ToString();
				if ( Content.IndexOf( "<EOF>" ) > -1 ) 
				{
					// Client terminates the session
					Console.WriteLine( "Read {0} bytes from socket. \n Data : {1}",
						Content.Length, Content );
					// Echo the data back to the client.
					//// Send( handler, content );
				} 
				else // more data expecting 
				{
					Console.WriteLine( "Session.ReadCallback: Read more" );					
					this.Socket.BeginReceive( this.buffer, 0, Session.buffersize, 0,
						new AsyncCallback( ReadCallback ), this 
					);
				}
			}
		}
		
		/// <summary>
		/// Sends a message to the client.
		/// </summary>
		/// <param name="Message">the message to be sent</param>
		
		private void Send( string Message ) 
		{
			byte[] ByteData = Encoding.ASCII.GetBytes( Message );		
			this.Socket.BeginSend(ByteData, 0, ByteData.Length, 0,
				new AsyncCallback( this.SendCallback ), this 
			);
		}

		private void SendCallback(IAsyncResult ar) 
		{
			try 
			{
				// Retrieve the socket from the state object.
				Socket handler = (Socket) ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);
				Console.WriteLine("Sent {0} bytes to client.", bytesSent);

				handler.Shutdown(SocketShutdown.Both);
				handler.Close();

			} 
			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}
		}
				
	}
}
