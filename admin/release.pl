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

# Name of cvs2pl script that generates the ChangeLog
my $CVS2CL="cvs2cl";

# List of files to be packaged
#
# example $targets definition:
#
#  my $targets = { "ChangeLog" => 1,
#  		"src/Npgsql" => 1,
#  		"src/testsuite" => 1,
#  		"admin" => 1};
#
my $targets = { "ChangeLog" => 1,
		"src/Npgsql" => 1,
		"src/testsuite" => 1,
		"src/testsuite/noninteractive" => 1,
		"admin" => 1};


# ========================================================================
# ------------------------------------------------------------------------
# DO NOT MODIFY BELOW THIS LINE
# ------------------------------------------------------------------------
# ========================================================================

#
# Error codes
#
my $ERROR_NOT_CVS_TAGGED = 1;
my $ERROR_DOES_NOT_COMPILE = 2;
my $ERROR_NONINTERACTIVE_TEST_DONT_PASS = 3;
my $ERROR_MISSING_LICENSE = 4;

#
# Check messages
#
my $OK="OK\n";
my $FAILED="FAILED\n";


# Change up to the root directory
chdir "..";

#
# Here we go, checking starts here...
#
banner();                     # Hello, this is a release script

# Gather some input
my ($npgsql_host) = gather_input();

check_compiles();             # Check if npgsql compiles
check_tests($npgsql_host);    # Run all tests
clean();                      # Clean up the mess we made so far
changelog();                  # Create a ChangeLog

# Figure out the release info and release tag
my ($rel_version, $version, $RELEASE_TAG) = get_release_info();

# Prepare the list of files to be passed to tar
my $target_files = prepare_release_files( $targets );


# Make sure that the notices for this project are in all the areas that
# they should be
check_notices( $target_files );

# Verify that all files are properly tagged
verify_tagged( $RELEASE_TAG );

# Create the release file
create_release( $RELEASE_NAME, $rel_version, $version, $target_files );

#
# Our job is done
#
exit 0;





# ========================================================================
# ------------------------------------------------------------------------
# Helper functions
# ------------------------------------------------------------------------
# ========================================================================
#
# All these subroutines are used above
#

######################################################################
# Print out a banner
######################################################################
sub banner() {
print "-------------------------------------------------------------------------\n";
print "This is the release script of $RELEASE_NAME. A few checks will be made...\n";
print "-------------------------------------------------------------------------\n";
}

######################################################################
# Gather some input from the user
######################################################################
sub gather_input() {
    my $npgsql_host;

    print "Please enter the IP address of the npgsql host database used for running the non-interactive test suite[default=127.0.0.1]: ";
    chomp ($npgsql_host = <STDIN>);

    if ($npgsql_host eq "") {
	$npgsql_host = "127.0.0.1";
    }

    return $npgsql_host;
}



######################################################################
# Make sure that npgsql compiles correctly
######################################################################
sub check_compiles() {
    print "Checking if npgsql compiles correctly...";
    if (system("(cd src/Npgsql; make 2>&1 > /dev/null)") == 0) {
	print $OK;
    } else {
	failed($ERROR_DOES_NOT_COMPILE);
    }
}

######################################################################
# Make sure that all noninteractive tests pass
######################################################################
sub check_tests() {
    my ($npgsql_host) = ( @_ );

    print "Checking if all non-interactive tests pass...";
    if (system("(cd src/testsuite/noninteractive; make $npgsql_host 2>&1 > /dev/null)") == 0) {
	print $OK;
    } else {
	failed($ERROR_NONINTERACTIVE_TEST_DONT_PASS);
    }
}

######################################################################
# Clean up the mess we made so far
######################################################################
sub clean() {
    print "Cleaning up ...";
    system("(cd src/Npgsql; make clean > /dev/null)");
    system("(cd src/testsuite/noninteractive; make clean > /dev/null)");
    system("rm -f admin/*.tar.gz > /dev/null");
    system("rm -f ChangeLog* > /dev/null");
    system("find . -name *~ -print0 | xargs -0 rm -f > /dev/null");
    print $OK;
}

######################################################################
# Create a ChangeLog
######################################################################
sub changelog() {
    print "Creating a ChangeLog file ...";
    system("$CVS2CL 2> /dev/null");
    print $OK;
}

######################################################################
# We failed somewhere; exit
######################################################################
sub failed() {
    my ($error) = @_;

    print $FAILED;
    exit $error;
}

######################################################################
# Figure out the new version number and release tag
######################################################################
sub get_release_info() {
    my $version;
    my $rel_version;
    my $rel_tag;

    $version = `grep AssemblyVersion src/Npgsql/AssemblyInfo.cs`;
    $version =~ /\"(.*)\"/;
    $version = $1;
    $rel_version = $version;
    $rel_version =~ s/\./\-/;
    $rel_tag = $version;
    $rel_tag =~ s/\./_/;

    my $RELEASE_TAG = "RELEASE-$rel_tag";

    return ( $rel_version, $version, $RELEASE_TAG );
}

######################################################################
# Prepare $target_files to be passed to tar
######################################################################
sub prepare_release_files() {
    my ( $targets ) = ( @_ );
    my $target_files;

    foreach my $file (sort keys %$targets) {
	if ( -f $file ) {
	    push @{ $target_files }, $file;
	} elsif ( -d $file ) {
	    open( DIRFILES, "find $file -print |grep -v CVS | grep -v .#| sed 1d |");
	    while(<DIRFILES>) {
		$_ =~ s/\n/ /;  #Strip trailing newline
		$_ =~ s/\s*$//; #Strip trailing whitespace
		push @{ $target_files }, $_;
	    }
	}
    }

    return $target_files;
}





######################################################################
# Make sure that npgsql includes the proper notices where it should
######################################################################
sub check_notices() {
    my $target_files = shift;
    my $copyright = "Copyright\ \(C\)\ 2002\ The\ Npgsql\ Development\ Team";
    my $licenses = { LGPL => "GNU Lesser General Public" };
    my $errors = 0;

    foreach my $file ( @$target_files ) {
	if ( $file =~ m/.*\.cs$/
	     || $file =~ m/Makefile/
	     || $file =~ m/.*\.pl/
	     ) {	    
	    my $found_license = 0;
	    my $file_content;

	    $/ = undef;               # Slurp in whole file
	    open ( CS_IN, "$file" );
	    $file_content = <CS_IN>;
	    close ( CS_IN );
	    
	    # Make sure the license is there
	    foreach my $license ( keys %$licenses ) {
		if ( $file_content =~ m/$licenses->{$license}/ ) {
		    $found_license = 1;
		}
	    }
	    if ( !$found_license ) {
		print "File $file is missing a proper license\n";
		$errors = 1;
	    }

#FIXME: This doesn't work. Hm...
	    # Make sure the copyright is there
#	    if ( $file_content !~ m/$copyright/ ) {
#		print "File $file is missing a proper copyright\n";
#		$errors = 1;
#	    }
	}
    }

    if ( $errors ) {
	print "Please add the proper notices in the listed files first\n";
	exit $ERROR_MISSING_LICENSE;
    }
}



######################################################################
# Verify that all files are properly tagged
######################################################################
sub verify_tagged() {
    my $errors;

    print "Checking if all files are properly tagged as $RELEASE_TAG. NO implicit tagging will be performed ...\n";
    foreach my $file ( @$target_files ) {
	# Ignore the ChangeLog file
	if ($file ne "ChangeLog") { 
	    my $line = `cvs status -v $file 2> /dev/null |grep $RELEASE_TAG | awk '{print $1}'`;
	    print "$file... ";
	    if ($line ne $RELEASE_TAG) {
		print "not cvs tagged as $RELEASE_TAG\n";
		$errors = 1;
	    } else {
		print $OK;
	    }
	}
    }

    if ($errors) {
	print "Please tag the listed files first\n";
	exit $ERROR_NOT_CVS_TAGGED;
    } else {
	print $OK;
	print "Everything is properly tagged as $RELEASE_TAG\n";
    }
}


######################################################################
# Create release tar.gz
######################################################################
sub create_release() {
    my ( $RELEASE_NAME, $rel_version, $version, $target_files ) = ( @_ );
    my $rel_file=$RELEASE_NAME . "_" . $rel_version . ".tar.gz";

    print "Packaging source release ...\n";
    system("(tar -czvf $rel_file @$target_files > /dev/null; mv $rel_file admin/.)");
    print "This is release version $version, since that's what's in AssemblyInfo.cs\n";
    chdir "admin";
    system("echo $rel_file is now in `pwd`\n");   
}

