-- ===================================================================
--
-- This file contains data that must be loaded in PostgreSQL for the
-- noninteractive tests to run
--
-- ===================================================================


--
-- Used in test_1
--
insert into tableA(field_text) values ('Random text');
insert into tableA(field_int4) values (4);
insert into tableA(field_int8) values (8);
insert into tableA(field_bool) values (true);
