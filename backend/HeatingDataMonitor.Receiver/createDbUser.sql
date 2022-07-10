CREATE USER receiver_user WITH PASSWORD 'dontworrythispasswordwillchangeinproduction';
GRANT SELECT, INSERT ON heating_data TO receiver_user;