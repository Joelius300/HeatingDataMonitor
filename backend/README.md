# HeatingDataMonitor Backend

The backend itself is split into two parts.  
One part contains the models, the logic for retrieving data from the heating unit and the database context for the history aspect.  
The other part contains the actual API logic including handling background processes, responding to http requests, opening a SignalR-Hub for real-time communication and actually using the provided database context with a specific implementation (here PostgreSQL).

The solution also contains two standalone applications:
- One is an application which allows me to easily migrate the archived data from the old system to the new system by converting the datetime values.
- The other one acts as a fake heating unit which periodically sends data to a specified serial port. This allows for more elaborate testing and debugging without having to reinvent the wheel for testing (as there are no abstractions to `SerialPort`).

## Debugging
As mentioned above, there's a fake heating unit which can be used for debugging very close to the actual use case. If you don't need that, just start the application in development mode which will automatically use a fake receiver which just reads from a file.

In development mode, the CORS policy is relaxed for `localhost:4200` where the angular dev server runs so the SignalR connection still works even when back- and frontend are hosted separately.

### Start backend in dev mode from CLI
`env ASPNETCORE_ENVIRONMENT=Development ./HeatingDataMonitor`

## Deploy
- `dotnet publish -c Release -r linux-arm`
- Copy `bin\Release\netcoreapp3.1\linux-arm\publish` to `/home/pi/HeatingDataMonitor`
- `chmod +x /home/pi/HeatingDataMonitor/HeatingDataMonitor`

## PostgreSQL
### Setup
- `sudo apt install postgresql postgresql-client` (Note, this might not be the latest major but that's fine)
- Change user for first connect: `sudo su postgres`
- `psql`
- Do everything in `dbCreate.sql` (there are multiple ways of doing so)
- For connecting later on: `psql -U heatingDataMonitorUser -h 127.0.0.1 HeatingDataMonitor`

### Adjusting the sequence after importing old data
This is necessary depending on how you import the data. Otherwise the sequence might generate already existing ids.  
Note the double quotes in single quotes, those are very important for the case sensitivity of PostgreSQL.

`SELECT setval('"HeatingData_Id_seq"', WhateverIdTheLatestRowHas);`

## Systemd service
### Add and register
- `sudo nano /etc/systemd/system/heating-data-monitor.service`
- Paste 
  ```
  [Unit]
  Description=Heating Data Monitor Backend running on .NET Core

  [Service]
  WorkingDirectory=/home/pi/HeatingDataMonitor
  ExecStart=/home/pi/HeatingDataMonitor/HeatingDataMonitor
  Restart=always
  # Restart service after 10 seconds if the dotnet service crashes:
  RestartSec=10
  KillSignal=SIGINT
  SyslogIdentifier=heating-data-monitor
  User=pi
  Environment=ASPNETCORE_ENVIRONMENT=Production
  Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

  [Install]
  WantedBy=multi-user.target
  ```
- `sudo systemctl enable heating-data-monitor`

### Manage
- `sudo systemctl start heating-data-monitor`
- `sudo systemctl stop heating-data-monitor`
- `sudo systemctl status heating-data-monitor`
- `sudo journalctl -fu heating-data-monitor`
 
