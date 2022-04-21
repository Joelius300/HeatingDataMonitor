# Setup

## Enable Serial Port on Raspberry Pi (4)

- Get and mount a Serial HAT (e.g. [ModMyPi Serial HAT RS232](https://www.pi-shop.ch/modmypi-serial-hat-rs232))
- `sudo raspi-config`
- Interfacing Options -> Serial
- Disable Login Shell
- Enable Serial Hardware
- Reboot
- `sudo nano /boot/config.txt`
- Add `dtoverlay=pi3-miniuart-bt` to the end
- Reboot
- Test (cat, echo, minicom and putty are all reasonable choices)
- If it doesn't work, it's most likely your cable but you can check if `/boot/cmdline.txt` still contains `console=serial0,115200` and if so remove it.

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

You can install and enable it with `docker-compose up -d` but you will want to adjust certain config files beforehand.

- docker-compose.yml contains many config options like the api-base-url and passwords.
- database passwords for specific SQL users need to be adjusted in the mounted SQL files before the first run.
- if there is data from an old system you would like to import automatically on the first run, it has to be exported into old-data.csv in the root project folder. The command to export the relevant columns is as follows: \
  ```bash
  psql -d "HeatingDataMonitor" -h localhost -U heatingDataMonitorUser -c "\copy \"HeatingData\" (\"SPS_Zeit\",\"ReceivedTime\",\"Kessel\",\"Ruecklauf\",\"Abgas\",\"CO2_Soll\",\"CO2_Ist\",\"Saugzug_Ist\",\"Puffer_Oben\",\"Puffer_Unten\",\"Platine\",\"Betriebsphase_Kessel\",\"Aussen\",\"Vorlauf_HK1_Ist\",\"Vorlauf_HK1_Soll\",\"Betriebsphase_HK1\",\"Vorlauf_HK2_Ist\",\"Vorlauf_HK2_Soll\",\"Betriebsphase_HK2\",\"Boiler_1\",\"DI_0\",\"DI_1\",\"DI_2\",\"DI_3\",\"A_W_0\",\"A_W_1\",\"A_W_2\",\"A_W_3\",\"A_EA_0\",\"A_EA_1\",\"A_EA_2\",\"A_EA_3\",\"A_EA_4\",\"A_PHASE_0\",\"A_PHASE_1\",\"A_PHASE_2\",\"A_PHASE_3\",\"A_PHASE_4\") TO '/mnt/data_backups/$(date +%Y-%m-%dT%H_%M_%S%z).csv' DELIMITER ',' CSV HEADER;"
  ```

## Setup backup job

TODO: document how to register the cron script to backup the current db contents to a csv on the network share.
