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
insert into tableA(field_text) values ('Text with \' single quote');

insert into tableB(field_int2) values (2);
insert into tableB(field_timestamp) values ('2002-02-02 09:00:23.345');
insert into tableB(field_numeric) values (4.23);
insert into tableB(field_numeric) values (-4.3);

insert into tableC(field_date) values ('2002-03-04');
insert into tableC(field_time) values ('10:03:45.345');

insert into tableD(field_float4) values (.123456);
insert into tableD(field_float8) values (.123456789012345);
