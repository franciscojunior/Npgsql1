NPGSQL_LIB=Npgsql.dll
CC=mcs
CVS2CL=cvs2cl.pl
LDFLAGS=-r /usr/lib/System.Data.dll --target library -o $(NPGSQL_LIB)
OBJECTS=NpgsqlCommand.cs NpgsqlConnection.cs NpgsqlException.cs \
	NpgsqlParameter.cs NpgsqlParameterCollection.cs AssemblyInfo.cs \
	PGUtil.cs NpgsqlEventLog.cs NpgsqlPasswordPacket.cs \
	NpgsqlStartupPacket.cs NpgsqlBackEndKeyData.cs NpgsqlAsciiRow.cs \
	NpgsqlRowDescription.cs

all: $(OBJECTS)
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~

changelog:
	$(CVS2CL)
