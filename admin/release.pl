#!/usr/bin/perl
# ------------------------------------------------------------------------
# This script is used to make new releases of Npgsql
# ------------------------------------------------------------------------

use strict;

# ========================================================================
# Set some global constants; these are all you will need to modify to add
# more files in a release

# Name of the release
my $RELEASE_NAME="npgsql";

# List of files to be packaged
#
# example $targets definition:
#
#my $targets = { "LICENSE.txt" => "file",
#  		"README.txt" => "file",
#  		"src/Npgsql" => "dir",
#  		"src/testsuite" => "dir"};
my $targets = { "src/Npgsql" => "dir",
		"src/testsuite" => "dir",
		"admin" => "dir"};


# ========================================================================
# ------------------------------------------------------------------------
# DO NOT MODIFY BELOW THIS LINE
# ------------------------------------------------------------------------
# ========================================================================


my $ERROR_INVALID_FILE_TYPE = 1;
my $ERROR_NOT_CVS_TAGGED = 2;

my $OK="OK\n";

# Change up to the root directory
chdir "..";

# Clean up 
print "Cleaning up...";
system("(cd src/Npgsql; make clean > /dev/null)");
system("rm -f admin/*.tar.gz > /dev/null");
print $OK;

#
# Figure out the new version number
#
my $version;
my $rel_version;
my $rel_tag;
open(IN, "grep AssemblyVersion src/Npgsql/AssemblyInfo.cs |");
while (<IN>) {
    $version = $_;
}
$version =~ /\"(.*)\"/;
$version = $1;
$rel_version = $version;
$rel_version =~ s/\./\-/;
$rel_tag = $version;
$rel_tag =~ s/\./_/;
my $RELEASE_TAG = "RELEASE-$rel_tag";

#
# Prepare $target_files to be passed to tar
#
my $target_files = [];
foreach my $file (sort keys %$targets) {
    if ( $targets->{ $file } eq "file" ) {
	push @{ $target_files }, $file;
    } elsif ( $targets->{ $file } eq "dir" ) {
	open( DIRFILES, "find $file -print |grep -v CVS | grep -v .#| sed 1d |");
	while(<DIRFILES>) {
	    $_ =~ s/\n/ /;
	    push @{ $target_files }, $_;
	}
    } else {
	print qq[Invalid type $targets->{$file}. Valid types are "file" and "dir"];
	exit $ERROR_INVALID_FILE_TYPE;
    }
}

#
# Verify that all files are properly tagged
#
print "Checking if all files are properly tagged as $RELEASE_TAG. NO implicit tagging will be performed...\n";
my $errors;
foreach my $file ( @$target_files ) {
    open(CVSLOG, "cvs status -v $file 2> /dev/null |grep $RELEASE_TAG | awk '{print $1}' |");
    print "$file... ";
    my $line;
    if (!<CVSLOG> || $_ ne $RELEASE_TAG) {
	print "not cvs tagged as $RELEASE_TAG\n";
	$errors = 1;
    } else {
	print $OK;
    }
}

if ($errors) {
    print "Please tag the listed files first\n";
    exit $ERROR_NOT_CVS_TAGGED;
} else {
    print $OK;
    print "Everything is properly tagged as $RELEASE_TAG\n";
}



#
# Create release tar.gzs
#
my $rel_file=$RELEASE_NAME . "_" . $rel_version . ".tar.gz";
print "Packaging source release...\n";
system("(tar -czvf $rel_file @$target_files > /dev/null; mv $rel_file admin/.)");
print "This is release version $version, since that's what's in AssemblyInfo.cs\n";
chdir "admin";
system("echo $rel_file is now in `pwd`\n");

