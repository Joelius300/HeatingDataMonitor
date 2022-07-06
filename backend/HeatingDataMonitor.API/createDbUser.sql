CREATE USER api_user WITH PASSWORD 'dontworrythispasswordwillchangeinproduction';
GRANT SELECT ON heating_data TO api_user;