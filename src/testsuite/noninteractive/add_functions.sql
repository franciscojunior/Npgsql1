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
declare
begin
     return 0;
end;
' language 'plpgsql';
