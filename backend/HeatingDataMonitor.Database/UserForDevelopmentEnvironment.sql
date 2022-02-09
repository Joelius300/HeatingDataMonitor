CREATE ROLE "heatingDataMonitorRole" WITH
	NOLOGIN
	NOSUPERUSER
	NOCREATEDB
	NOCREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1;

REVOKE ALL ON ALL TABLES IN SCHEMA public from "heatingDataMonitorRole";
GRANT SELECT, INSERT on heating_data to "heatingDataMonitorRole";

CREATE ROLE "heatingDataMonitorUser" WITH
	LOGIN
	NOSUPERUSER
	NOCREATEDB
	NOCREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1
	PASSWORD 'dontworrythispasswordwillchangeinproduction';

GRANT "heatingDataMonitorRole" TO "heatingDataMonitorUser"; 
