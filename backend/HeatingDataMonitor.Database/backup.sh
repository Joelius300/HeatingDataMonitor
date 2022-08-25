#!/bin/bash
backupFolder=~/HeatingDataMonitorBackups
backupFile=$backupFolder/$(date +%Y-%m-%dT%H_%M_%S%z).csv.gz
backupUserPassword=dontworrythispasswordwillchangeinproduction

# cannot directly copy from table because timescaledb chunks the data
psql postgresql://backup_user:$backupUserPassword@127.0.0.1/heating_data_monitor -c "\copy (SELECT * FROM heating_data) TO STDOUT DELIMITER ',' CSV HEADER;" | gzip -c > $backupFile

# At 03:00 on Friday, run the backup script
# (crontab -l 2>/dev/null; echo "0 3 * * 5 ~/HeatingDataMonitor/backend/HeatingDataMonitor.Database/backup.sh") | crontab -

