NPGSQL_LIB=Npgsql.dll
# Allow compiling under Cygwin
ifeq ($(OS_TYPE),"WINDOWS_NT")
	CC=csc.exe
	LDFLAGS=/r:System.Data.dll /target:library /out:$(NPGSQL_LIB)
else
	CC=mcs
	LDFLAGS=-r /usr/lib/System.Data.dll --target library -o $(NPGSQL_LIB)
endif
CVS2CL=cvs2cl.pl
OBJECTS:=$(shell ls *.cs)

all: $(OBJECTS)
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~

changelog:
	$(CVS2CL)
