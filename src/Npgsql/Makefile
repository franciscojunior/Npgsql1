OUTPUT_FILE=Npgsql.dll
CC=mcs
LDFLAGS=-r /usr/lib/System.Data.dll --target library -o $(OUTPUT_FILE)
OBJECTS=NpgsqlCommand.cs NpgsqlConnection.cs NpgsqlException.cs \
	NpgsqlParameter.cs NpgsqlParameterCollection.cs AssemblyInfo.cs \
	PGUtil.cs

all:
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(OUTPUT_FILE) *~