// created on 13/6/2002 at 21:06

// Npgsql.NpgsqlAsciiRow.cs
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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using NpgsqlTypes;


namespace Npgsql
{

    /// <summary>
    /// This class represents the AsciiRow message sent from PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class NpgsqlAsciiRow
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlAsciiRow";

        private ArrayList							data;
        private readonly Int16	READ_BUFFER_SIZE = 300; //[FIXME] Is this enough??
        private NpgsqlRowDescription row_desc;
        private Hashtable							oid_to_name_mapping;
        private Int32                 protocol_version;



        public NpgsqlAsciiRow(NpgsqlRowDescription rowDesc, Hashtable oidToNameMapping, Int32 protocolVersion)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            data = new ArrayList();
            row_desc = rowDesc;
            oid_to_name_mapping = oidToNameMapping;
            protocol_version = protocolVersion;

        }

        public void ReadFromStream(Stream inputStream, Encoding encoding)
        {
            if (protocol_version == ProtocolVersion.Version2)
            {
                ReadFromStream_Ver_2(inputStream, encoding);
            }
            else
            {
                ReadFromStream_Ver_3(inputStream, encoding);
            }
        }

        private void ReadFromStream_Ver_2(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_2()");

            Byte[]       input_buffer = new Byte[READ_BUFFER_SIZE];
            Byte[]       null_map_array = new Byte[(row_desc.NumFields + 7)/8];

            Array.Clear(null_map_array, 0, null_map_array.Length);

            // Read the null fields bitmap.
            PGUtil.CheckedStreamRead(inputStream, null_map_array, 0, null_map_array.Length );

            // Get the data.
            for (Int16 field_count = 0; field_count < row_desc.NumFields; field_count++)
            {

                // Check if this field isn't null
                if (IsBackendNull(null_map_array, field_count))
                {
                    // Field is null just keep next field.

                    data.Add(DBNull.Value);
                    continue;
                }

                // Read the first data of the first row.

                PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, 4);

                Int32 field_value_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
                field_value_size -= 4;
                Int32 bytes_left = field_value_size;

                StringBuilder result = new StringBuilder();

                while (bytes_left > READ_BUFFER_SIZE)
                {
                    // Now, read just the field value.
                    PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, READ_BUFFER_SIZE);

                    // Read the bytes as string.
                    result.Append(new String(encoding.GetChars(input_buffer, 0, READ_BUFFER_SIZE)));

                    bytes_left -= READ_BUFFER_SIZE;
                }

                // Now, read just the field value.
                PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, bytes_left);

                // Read the bytes as string.
                result.Append(new String(encoding.GetChars(input_buffer, 0, bytes_left)));


                // Add them to the AsciiRow data.
                data.Add(NpgsqlTypesHelper.ConvertBackendStringToSystemType(oid_to_name_mapping, result.ToString(), row_desc[field_count].type_oid, row_desc[field_count].type_modifier));

            }
        }

        private void ReadFromStream_Ver_3(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3()");

            Byte[] input_buffer = new Byte[READ_BUFFER_SIZE];

            PGUtil.ReadInt32(inputStream, input_buffer);
            Int16 numCols = PGUtil.ReadInt16(inputStream, input_buffer);

            for (Int16 field_count = 0; field_count < numCols; field_count++)
            {
                Int32 field_value_size = PGUtil.ReadInt32(inputStream, input_buffer);

                if (field_value_size == -1) // Null value
                {
                    // Field is null just keep next field.

                    data.Add(DBNull.Value);
                    continue;

                }
                Int32 bytes_left = field_value_size;

                StringBuilder result = new StringBuilder();

                while (bytes_left > READ_BUFFER_SIZE)
                {
                    // Now, read just the field value.
                    PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, READ_BUFFER_SIZE);

                    // Read the bytes as string.
                    result.Append(new String(encoding.GetChars(input_buffer, 0, READ_BUFFER_SIZE)));

                    bytes_left -= READ_BUFFER_SIZE;
                }

                // Now, read just the field value.
                PGUtil.CheckedStreamRead(inputStream, input_buffer, 0, bytes_left);

                if (row_desc[field_count].format_code == FormatCode.Text)
                {
                    // Read the bytes as string.
                    result.Append(new String(encoding.GetChars(input_buffer, 0, bytes_left)));
                    // Add them to the AsciiRow data.
                    data.Add(NpgsqlTypesHelper.ConvertBackendStringToSystemType(oid_to_name_mapping, result.ToString(), row_desc[field_count].type_oid, row_desc[field_count].type_modifier));
                }
                else
                    data.Add(NpgsqlTypesHelper.ConvertBackendBytesToStytemType(oid_to_name_mapping, input_buffer, encoding, field_value_size, row_desc[field_count].type_oid, row_desc[field_count].type_modifier));
            }
        }

        // Using the given null field map (provided by the backend),
        // determine if the given field index is mapped null by the backend.
        // We only need to do this for version 2 protocol.
        private static Boolean IsBackendNull(Byte[] null_map_array, Int32 index)
        {
            
            // Get the byte that holds the bit index position.
            Byte test_byte = null_map_array[index/8];

            // Now, check if index bit is set.
            // To this, get its position in the byte, shift to
            // MSB and test it with the byte 10000000.
            return (((test_byte << (index%8)) & 0x80) == 0);
        }


        public Boolean IsDBNull(Int32 index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IsDBNull", index);

            // Check valid index range.
            if ((index < 0) || (index >= row_desc.NumFields))
                throw new ArgumentOutOfRangeException("index");

            return (this.data[index] == DBNull.Value);
        }

        public Object this[Int32 index]
        {
            get
            {

                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, index);

                if ((index < 0) || (index >= row_desc.NumFields))
                    throw new ArgumentOutOfRangeException("this[] index value");
                
                return data[index];



            }
        }
    }

}
