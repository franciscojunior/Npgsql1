-- ===================================================================
--
-- This file contains tables that must be loaded in PostgreSQL for the
-- noninteractive tests to run
--
-- ===================================================================


--
-- Used in test_1
--
create table tableA(
field_serial serial,
field_text text,
field_int4 int4,
field_int8 int8,
field_bool bool
);