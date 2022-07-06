-- Order of the columns (esp. first two) has to remain the same for compatibility reasons.
-- While we could switch them, I don't want to be the guy to debug the issues if they don't get switched somewhere.
CREATE TABLE heating_data (
    sps_zeit timestamp without time zone NOT NULL,
    received_time timestamp with time zone NOT NULL,
    kessel real NOT NULL,
    ruecklauf real NOT NULL,
    abgas real NOT NULL,
    co2_soll real NOT NULL,
    co2_ist real NOT NULL,
    saugzug_ist real NOT NULL,
    puffer_oben real NOT NULL,
    puffer_unten real NOT NULL,
    platine real NOT NULL,
    betriebsphase_kessel smallint NOT NULL,
    aussen real NOT NULL,
    vorlauf_hk1_ist real NOT NULL,
    vorlauf_hk1_soll real NOT NULL,
    betriebsphase_hk1 smallint NOT NULL,
    vorlauf_hk2_ist real NOT NULL,
    vorlauf_hk2_soll real NOT NULL,
    betriebsphase_hk2 smallint NOT NULL,
    boiler_1 real NOT NULL,
    di_0 boolean NOT NULL,
    di_1 boolean NOT NULL,
    di_2 boolean NOT NULL,
    di_3 boolean NOT NULL,
    a_w_0 boolean NOT NULL,
    a_w_1 boolean NOT NULL,
    a_w_2 boolean NOT NULL,
    a_w_3 boolean NOT NULL,
    a_ea_0 boolean NOT NULL,
    a_ea_1 boolean NOT NULL,
    a_ea_2 boolean NOT NULL,
    a_ea_3 boolean NOT NULL,
    a_ea_4 boolean NOT NULL,
    a_phase_0 boolean NOT NULL,
    a_phase_1 boolean NOT NULL,
    a_phase_2 boolean NOT NULL,
    a_phase_3 boolean NOT NULL,
    a_phase_4 boolean NOT NULL
);

SELECT create_hypertable('heating_data', 'received_time');

CREATE OR REPLACE FUNCTION record_added() -- trigger functions cannot have arguments, use NEW directly
    RETURNS TRIGGER AS $$
BEGIN
    PERFORM pg_notify('record_added', to_char(NEW.received_time, 'YYYY-MM-DD"T"HH24:MI:SS"."FF6"Z"'));
    RETURN NEW; -- returns the record that triggered this function
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER record_added
AFTER INSERT ON heating_data
FOR EACH ROW EXECUTE PROCEDURE record_added();
