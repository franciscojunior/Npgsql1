NPGSQL_LIB=Npgsql.dll
CC=mcs
LDFLAGS=-r /usr/lib/System.Data.dll --target library -o $(NPGSQL_LIB)
OBJECTS=NpgsqlCommand.cs NpgsqlConnection.cs NpgsqlException.cs \
	NpgsqlParameter.cs NpgsqlParameterCollection.cs AssemblyInfo.cs \
	PGUtil.cs NpgsqlEventLog.cs NpgsqlPasswordPacket.cs \
	NpgsqlStartupPacket.cs

all: $(OBJECTS)
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~
