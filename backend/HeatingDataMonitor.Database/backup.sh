#!/bin/bash
backupFolder=~/HeatingDataMonitorBackups
backupUserPassword=dontworrythispasswordwillchangeinproduction

psql postgresql://backup_user:$backupUserPassword@127.0.0.1/heating_data_monitor -c "\copy (SELECT * FROM heating_data) TO '$backupFolder/$(date +%Y-%m-%dT%H_%M_%S%z).csv' DELIMITER ',' CSV HEADER;"

# TODO register but this doesn't seem to work? (crontab -l 2>/dev/null; echo "0 3 * * 5 ~/HeatingDataMonitor/backend/HeatingDataMonitor.Database/backup.sh") | crontab -
