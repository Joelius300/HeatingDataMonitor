# HeatingDataMonitor Backend

## Architecture

Although I recently did a giant overhaul of the backend architecture with a clear net positive, I am not entirely happy with how it turned out - especially the database layer. I already have ideas how this could/should be structured instead, which will be probably be reevaluated and implemented as part of the push notifications (see ["Push notifications"-project](https://github.com/Joelius300/HeatingDataMonitor/projects/4)).

There are three main parts within the backend. The database is the center piece of the backend. Then on one side there is the receiver, which continuously parses the data from the heating unit and appends it to the database. Only the receiver has write access to the database. On the other hand, there is the API, which handles the communication with the frontend by querying the db for certain time periods as well as listening for new records added by the receiver and notifying the frontend clients in real time.

![architecture.svg](heating_data_monitor_architecture.drawio.svg)

## Systemd service

TODO remove but may be helpful for receiver

### Add and register

-   `sudo nano /etc/systemd/system/heating-data-monitor.service`
-   Paste

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

-   `sudo systemctl enable heating-data-monitor`

### Manage

-   `sudo systemctl start heating-data-monitor`
-   `sudo systemctl stop heating-data-monitor`
-   `sudo systemctl status heating-data-monitor`
-   `sudo journalctl -fu heating-data-monitor`
