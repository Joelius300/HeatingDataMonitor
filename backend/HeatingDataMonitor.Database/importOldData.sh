#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
	SET TimeZone='UTC';
    \copy heating_data from old-data.csv DELIMITER ',' CSV HEADER
    SET TimeZone='Europe/Zurich';
EOSQL