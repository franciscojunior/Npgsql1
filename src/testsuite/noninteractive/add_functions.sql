-- ===================================================================
--
-- This file contains functions  that must be loaded in PostgreSQL for
-- the noninteractive tests to run
--
-- ===================================================================


--
-- Used in test_1
--
create function funcA() returns int as '
     select 0;

' language 'sql';


create function funcB() returns setof tablea as ' 
select * from tablea;
' language 'sql';


create function funcC() returns int8 as '
select count(*) from tablea;
' language 'sql';


create function funcC(int4) returns int8 as '
select count(*) from tablea where field_int4 = $1;
' language 'sql';

create function ambiguousParameterType(int2, int4, int8, text, varchar(10), char(5)) returns int4 as ' select 4 as result; ' language 'sql';

