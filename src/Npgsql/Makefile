NPGSQL_LIB=Npgsql.dll
CC=mcs
CVS2CL=cvs2cl.pl
LDFLAGS=-r /usr/lib/System.Data.dll --target library -o $(NPGSQL_LIB)
OBJECTS:=$(shell ls *.cs)

all: $(OBJECTS)
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~

changelog:
	$(CVS2CL)
