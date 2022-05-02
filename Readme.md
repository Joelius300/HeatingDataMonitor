# Summary
Heating-data-monitor is the creative name of the monitoring system I built for our heating unit. It consists of a database, a receiving/parsing application, a real-time web application, some scripts, sarcastic documentation and more.

This is the first web application I've ever worked on and it's given me some wonderful as well as some dreadful first experiences. To pay homage to the immense impact this project has had on me and how it shaped my path as a programmer, I _will_ write down some words on it's journey **TODO** :)

_Note: I'm currently doing [another rework](https://github.com/Joelius300/HeatingDataMonitor/projects/2) to retire my [second big rework](https://github.com/Joelius300/HeatingDataMonitor/tree/v1) which superseded the [initial system](https://github.com/Joelius300/HeatingDataMonitor/tree/v0), which was actually my first ever application involving web dev I believe._

The data retrieval, storage and sending is done with an [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core) backend using [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) and [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction).  
Reading the semicolon separated values from the heating unit is done with [CsvHelper](https://github.com/JoshClose/CsvHelper).  
The user interface is realized with an [Angular](https://angular.io/) frontend using [flex-layout](https://github.com/angular/flex-layout) and [angular-material](https://material.angular.io/).  
Both front- and backend are hosted on a [Raspberry Pi](https://www.raspberrypi.org/) 4 (with [Raspbian Buster Lite](https://www.raspberrypi.org/downloads/raspberry-pi-os/)) using [NGINX](https://www.nginx.com/) and [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel).  
The Raspberry Pi is connected to the heating unit via [RS232](https://en.wikipedia.org/wiki/RS-232) using a [ModMyPi Serial HAT](https://www.pi-shop.ch/modmypi-serial-hat-rs232).  
The data management below EF Core is done with [PostgreSQL](https://www.postgresql.org/).

I'm using this application as an opportunity to gain experience in several areas including front- and backend web development, database management, hosting and server management (on a small scale with linux), code management (git & GitHub), working with hardware (Raspberry Pi & RS232) and a lot more.

# Goal
The app isn't finished yet. The end-goal is to have the following features:

TODO: Get some checkmarks going here and add more sophisticated goals

- Real time view (dashboard) of the current state of the heating unit
- History view with graph to inspect the activity within a certain timespan
- A forecast when the heating unit needs to be fired up for the boiler temp not to get below a certain point (using ML)
- ~~Real time graph of archived datapoints in the last 15 minutes (so the 15 latest entries, updated in real time)~~
- Real time graph of the last x values received from the heating unit

Also, I don't know what many of the values mean nor their correct unit or mapping (for enums). I'll have to contact the manufacturer of the heating unit in order to correctly display and interpret those.

# Demo
The interface isn't the prettiest but it gets the job done :)

![Demo](Demo.gif)

# Setup
## Enable Serial Port on Raspberry Pi (4)
- Get a Serial HAT (e.g. [ModMyPi Serial HAT RS232](https://www.pi-shop.ch/modmypi-serial-hat-rs232))
- Mount
- `sudo raspi-config`
- Interfacing Options -> Serial
- Disable Login Shell
- Enable Serial Hardware
- Reboot
- `sudo nano /boot/config.txt`
- Add `dtoverlay=pi3-miniuart-bt` to the end
- Reboot
- Test (using minicom and putty is probably easiest)
- If it doesn't work, you should most likely check your cable but you can check if `/boot/cmdline.txt` still contains `console=serial0,115200` and remove it.

## Firewall
- `sudo apt-get install ufw`
- `sudo ufw default deny incoming`
- `sudo ufw default allow outgoing`
- `sudo ufw allow ssh`
- `sudo ufw allow sftp`
- `sudo ufw allow http`
- `sudo ufw enable`
- `sudo ufw status verbose` should result in 
  ```
  Status: active
  Logging: on (low)
  Default: deny (incoming), allow (outgoing), disabled (routed)
  New profiles: skip  
  To                         Action      From
  --                         ------      ----
  22/tcp                     ALLOW IN    Anywhere
  80/tcp                     ALLOW IN    Anywhere
  115/tcp                    ALLOW IN    Anywhere
  22/tcp (v6)                ALLOW IN    Anywhere (v6)
  80/tcp (v6)                ALLOW IN    Anywhere (v6)
  115/tcp (v6)               ALLOW IN    Anywhere (v6)
  ```

## Setup receiver
The receiver is deployed without docker for multiple reasons but mostly serial port troubles..
TODO: document how to install and enable receiver. Will most likely be a bash script to compile the app, copy it somewhere and then register a systemd service

## Setup application
Unlike the receiver, the rest of the application including database, backend, frontend and reverse proxy use docker and docker-compose for fast and simple deployment.

TODO: Explain all the steps necessary before setting it up (adjusting env vars in the compose file and angular env files, etc.)

You can install and enable it with `docker-compose up -d`

## Setup backup job
TODO: document how to register the cron script to backup the current db contents to a csv on the network share.

# Credits / Licenses
This project is licensed under the [GNU Affero General Public License Version 3](https://www.gnu.org/licenses/agpl-3.0.en.html). This applies to every file within this repository unless there is a license notice at the top of the file that says otherwise.  
If you have any questions about this or would like to use a specific part under a different license, please open an issue and I'll try my best to assist.
