-- Create the db on the postgres service-db, then reopen the connection to the actual db
CREATE DATABASE heating_data_monitor;

-- Order of the columns (esp. first two) has to remain the same for compatibility reasons.
-- While we could switch them, I don't want to be the guy to debug the issues if they don't get switched somewhere.
CREATE TABLE heating_data (
    sps_zeit timestamp without time zone NOT NULL,
    received_time timestamp with time zone NOT NULL,
    kessel real NULL,
    ruecklauf real NULL,
    abgas real NULL,
    co2_soll real NULL,
    co2_ist real NULL,
    saugzug_ist real NULL,
    puffer_oben real NULL,
    puffer_unten real NULL,
    platine real NULL,
    betriebsphase_kessel smallint NULL,
    aussen real NULL,
    vorlauf_hk1_ist real NULL,
    vorlauf_hk1_soll real NULL,
    betriebsphase_hk1 smallint NULL,
    vorlauf_hk2_ist real NULL,
    vorlauf_hk2_soll real NULL,
    betriebsphase_hk2 smallint NULL,
    boiler_1 real NULL,
    di_0 smallint NULL,
    di_1 smallint NULL,
    di_2 smallint NULL,
    di_3 smallint NULL,
    a_w_0 smallint NULL,
    a_w_1 smallint NULL,
    a_w_2 smallint NULL,
    a_w_3 smallint NULL,
    a_ea_0 smallint NULL,
    a_ea_1 smallint NULL,
    a_ea_2 smallint NULL,
    a_ea_3 smallint NULL,
    a_ea_4 smallint NULL,
    a_phase_0 smallint NULL,
    a_phase_1 smallint NULL,
    a_phase_2 smallint NULL,
    a_phase_3 smallint NULL,
    a_phase_4 smallint NULL
);

SELECT create_hypertable('heating_data', 'received_time');
