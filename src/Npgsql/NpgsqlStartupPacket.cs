// created on 9/6/2002 at 16:56


// Npgsql.NpgsqlStartupPacket.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
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

using System;
using System.IO;
using System.Text;
using System.Net;

namespace Npgsql
{

    /// <summary>
    /// This class represents a StartupPacket message of PostgreSQL
    /// protocol.
    /// </summary>
    ///
    internal sealed class NpgsqlStartupPacket
    {

        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlStartupPacket";

        // Private fields.
        private Int32 packet_size;
        private Int32 protocol_version;
        private String database_name;
        private String user_name;
        private String arguments;
        private String unused;
        private String optional_tty;

        public NpgsqlStartupPacket(Int32 packet_size,
                                   Int32 protocol_version_major,
                                   Int32 protocol_version_minor,
                                   String database_name,
                                   String user_name,
                                   String arguments,
                                   String unused,
                                   String optional_tty)
        {

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            // Just copy the values.

            // [FIXME] Validate params? We are the only clients, so, hopefully, we
            // know what to send.

            this.packet_size = packet_size;
            this.protocol_version = (protocol_version_major<<16) | protocol_version_minor;
            this.database_name = database_name;
            this.user_name = user_name;
            this.arguments = arguments;
            this.unused = unused;
            this.optional_tty = optional_tty;

        }


        public void WriteToStream(Stream output_stream, Encoding encoding)
        {

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteToStream");

            // [FIXME] Need exception handling ?

            if (protocol_version == ProtocolVersion.Version2) // Protocol 2.0
            {

                // Packet length = 296
                output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.packet_size)), 0, 4);

                // Protocol version = 2.0
                output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.protocol_version)), 0, 4);

                // Database name.
                PGUtil.WriteLimString(this.database_name, 64, output_stream, encoding);

                // User name.
                PGUtil.WriteLimString(this.user_name, 32, output_stream, encoding);

                // Arguments.
                PGUtil.WriteLimString(this.arguments, 64, output_stream, encoding);

                // Unused.
                PGUtil.WriteLimString(this.unused, 64, output_stream, encoding);

                // Optional tty.
                PGUtil.WriteLimString(this.optional_tty, 64, output_stream, encoding);
                output_stream.Flush();

            }

            if (protocol_version >= ProtocolVersion.Version3) // Protocol 3+
            {
                PGUtil.WriteInt32(output_stream, 4 + 4 + 5 + (encoding.GetByteCount(user_name) + 1) + 9 + (encoding.GetByteCount(database_name) + 1) + 10 + 4 + 1);

                // Protocol version = 3.0

                PGUtil.WriteInt32(output_stream, this.protocol_version);

                // User name.

                PGUtil.WriteString("user", output_stream, encoding);

                // User name.
                PGUtil.WriteString(user_name, output_stream, encoding);

                // Database name.
                PGUtil.WriteString("database", output_stream, encoding);

                // Database name.
                PGUtil.WriteString(database_name, output_stream, encoding);
                
                // DateStyle.
                PGUtil.WriteString("DateStyle", output_stream, encoding);

                // DateStyle.
                PGUtil.WriteString("ISO", output_stream, encoding);

                output_stream.WriteByte(0);
                output_stream.Flush();

            }




        }
    }
}
