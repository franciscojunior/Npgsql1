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

create table tableB(
field_serial serial,
field_int2 int2,
field_timestamp timestamp,
field_numeric numeric(11,7)
);

create table tableC(
field_serial serial,
field_date date,
field_time time
);

create table tableD(
field_serial serial,
field_float4 float4,
field_float8 float8
);


create table tableE(
field_serial serial,
field_point point, 
field_box box, 
field_lseg lseg, 
field_path path, 
field_polygon polygon, 
field_circle circle
);

