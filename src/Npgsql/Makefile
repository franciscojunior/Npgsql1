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
RESGENFLAGS=Design/ConnectionStringEditorForm.resx Npgsql.Design.ConnectionStringEditorForm.resources
# Allow compiling under Cygwin
ifeq ($(OS),"Windows_NT")
	CC=csc.exe
	RESGEN=resgen.exe
	LDFLAGS=/r:System.Data.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /res:Npgsql.NpgsqlCommand.bmp /res:Npgsql.NpgsqlConnection.bmp /res:Npgsql.NpgsqlDataAdapter.bmp /res:Npgsql.Design.ConnectionStringEditorForm.resources /target:library /out:$(NPGSQL_LIB)
else
	CC=mcs 
	RESGEN=monoresgen
	LDFLAGS=-r System.Data.dll -r System.Windows.Forms.dll -r System.Drawing.dll -resource:Npgsql.NpgsqlCommand.bmp -resource:Npgsql.NpgsqlConnection.bmp -resource:Npgsql.NpgsqlDataAdapter.bmp -resource:Npgsql.Design.ConnectionStringEditorForm.resources --target library -o $(MONO_BUILD_DIR)/$(NPGSQL_LIB)
endif
CVS2CL=cvs2cl.pl
OBJECTS:=$(shell ls *.cs ../NpgsqlTypes/*.cs ./Design/*.cs)

all: $(OBJECTS)
	mkdir -p $(MONO_BUILD_DIR)
	mkdir -p $(NUNIT_BUILD_DIR) 
	$(RESGEN) $(RESGENFLAGS)
	cp NpgsqlCommand.bmp Npgsql.NpgsqlCommand.bmp
	cp NpgsqlConnection.bmp Npgsql.NpgsqlConnection.bmp
	cp NpgsqlDataAdapter.bmp Npgsql.NpgsqlDataAdapter.bmp
	$(CC) $(LDFLAGS) $(OBJECTS)

clean:
	rm -rf $(NPGSQL_LIB) *~
	rm -f Npgsql.Design.ConnectionStringEditorForm.resources
	rm -f Npgsql.NpgsqlCommand.bmp
	rm -f Npgsql.NpgsqlConnection.bmp
	rm -f Npgsql.NpgsqlDataAdapter.bmp

changelog:
	$(CVS2CL)

nunittests:
	(cd ../testsuite/noninteractive/NUnit20/ && make && cd ../../../)
	(cp $(MONO_BUILD_DIR)/$(NPGSQL_LIB) $(NUNIT_BUILD_DIR))
	 
	(cd $(NUNIT_BUILD_DIR) && mono $(NUNIT_PATH) NpgsqlTests.dll && cd ../../)
