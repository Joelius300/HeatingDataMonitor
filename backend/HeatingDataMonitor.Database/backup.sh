#!/bin/bash
backupFolder=/home/pi/HeatingDataMonitorBackups
backupFile=$backupFolder/$(date +%Y-%m-%dT%H_%M_%S%z).csv.gz
backupUserPassword=dontworrythispasswordwillchangeinproduction

# cannot directly copy from table because timescaledb chunks the data
docker exec -t heatingdatamonitor-database-1 psql postgresql://backup_user:$backupUserPassword@127.0.0.1/heating_data_monitor -c "\copy (SELECT * FROM heating_data) TO STDOUT DELIMITER ',' CSV HEADER;" | gzip -c > $backupFile

# to avoid confusion for future me regarding the nesting of these commands, it's as follows:
# - on the rpi cron executes this script as the root user -> root invokes docker exec
# - inside the docker container psql is invoked with a psql command
# - psql commands aren't (fully) evaluated inside the db but client side (IIUC) so the output file STDOUT in the \copy command refers to the docker container's STDOUT
# - docker exec redirects that STDOUT of the docker container to the STDOUT of the rpi (that's the -t)
# - the rpi's STDOUT is piped to gzip which compresses all the csv data to a file on the rpi (specified by $backupFile)

# At 03:00 on Friday, run the backup script
# (sudo crontab -l 2>/dev/null; echo "0 3 * * 5 /home/pi/HeatingDataMonitor/backend/HeatingDataMonitor.Database/backup.sh") | sudo crontab -

