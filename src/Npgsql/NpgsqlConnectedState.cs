// Npgsql.NpgsqlConnectedState.cs
// 
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
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

namespace Npgsql
{
	internal sealed class NpgsqlConnectedState : NpgsqlState
	{
		private readonly int ProtocolVersionMajor = 2;
		private readonly int ProtocolVersionMinor = 0;
    
		private static NpgsqlConnectedState _instance = null;
		private NpgsqlConnectedState()
		{
		}
		public static NpgsqlConnectedState Instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = new NpgsqlConnectedState();
				}
				return _instance;
			}
		}
		public override void Startup(NpgsqlConnection context)
		{
			NpgsqlStartupPacket startupPacket  = new NpgsqlStartupPacket(296,
																   ProtocolVersionMajor,
																   ProtocolVersionMinor,
																   context.DatabaseName,
																   context.UserName,
																   "",
																   "",
																   "");
			startupPacket.WriteToStream( context.TcpClient.GetStream(), context.Encoding );
			ProcessBackendResponses( context, new ProcessBackendMessage(ProcessStartupResponses) );
		}
		private void ProcessStartupResponses( NpgsqlConnection context, Object message )
		{
			

		}
	}
}