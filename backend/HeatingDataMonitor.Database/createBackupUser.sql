CREATE USER backup_user WITH PASSWORD 'dontworrythispasswordwillchangeinproduction';
GRANT SELECT ON heating_data TO backup_user;
