# Makefile
#
#	Copyright (C) 2002 The Npgsql Development Team
#	npgsql-general@gborg.postgresql.org
#	http://gborg.postgresql.org/project/npgsql/projdisplay.php
#
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
# 
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
# 
# You should have received a copy of the GNU Lesser General Public
# License along with this library; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

NPGSQL_LIB=Npgsql.dll
BUILD_DIR=build
MONO_BUILD_DIR=$(BUILD_DIR)/mono
NUNIT_BUILD_DIR=$(BUILD_DIR)/nunittests

NUNIT_PATH=/home/fxjr/Mono/mcs/nunit20/nunit-console.exe` 
# Allow compiling under Cygwin
ifeq ($(OS),"Windows_NT")
	CC=csc.exe
	LDFLAGS=/r:System.Data.dll /target:library /out:$(NPGSQL_LIB)
else
	CC=mcs 
	LDFLAGS=-r System.Data.dll --target library -o $(MONO_BUILD_DIR)/$(NPGSQL_LIB)
endif
CVS2CL=cvs2cl.pl
OBJECTS:=$(shell ls *.cs ../NpgsqlTypes/*.cs)

all: $(OBJECTS)
	mkdir -p $(MONO_BUILD_DIR)
	mkdir -p $(NUNIT_BUILD_DIR) 
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~

changelog:
	$(CVS2CL)

nunittests:
	(cd ../testsuite/noninteractive/NUnit20/ && make && cd ../../../)
	(cp $(MONO_BUILD_DIR)/$(NPGSQL_LIB) $(NUNIT_BUILD_DIR))
	 
	(cd $(NUNIT_BUILD_DIR) && mono $(NUNIT_PATH) NpgsqlTests.dll && cd ../../)
