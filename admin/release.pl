#!/usr/bin/perl
# ------------------------------------------------------------------------
# This script is used to make new releases of Npgsql
# ------------------------------------------------------------------------

use strict;

# ------------------------------------------------------------------------
# Set some global constants; these are all you will need to modify to add
# more files in a release

my $RELEASE_NAME="npgsql";
my $targets = "LICENSE.txt README.txt" .
    " `find src/Npgsql -print |grep -v CVS |sed 1d`" .
    " `find src/testsuite -print |grep -v CVS |sed 1d`" ;
# ------------------------------------------------------------------------

#
# Figure out the new version number
#
my $version;
my $rel_version;
open(IN, "grep AssemblyVersion ../src/Npgsql/AssemblyInfo.cs |");
while (<IN>) {
    $version = $_;
}
$version =~ /\"(.*)\"/;
$version = $1;
$rel_version = $version;
$rel_version =~ s/\./\-/;


#
# Create release tar.gzs
#

# Clean up 
print "Cleaning up...\n";
system("(cd ../src/Npgsql; make clean)");

my $rel_file=$RELEASE_NAME . "_" . $rel_version . ".tar.gz";
print "Packaging source release...\n";
system("(cd ..; tar -czvf $rel_file $targets; mv $rel_file admin/.)");
print "This is release version $version, since that's what's in AssemblyInfo.cs\n";
system("echo $rel_file is now in `pwd`\n");
