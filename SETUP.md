# Setup

## Install Raspberry Pi OS 64-bit

- Use Raspberry Pi Imager with the appropriate settings (hostname, ssh, internet, locale, etc.)
- Expand filesystem (in raspi-config)
- Update: `sudo apt update && sudo apt full-upgrade`, then reboot
- Update SSH Port
  - `sudo vi /etc/ssh/sshd_config`
  - `sudo systemctl restart ssh`
  - To connect: `ssh pi@heating-data-monitor.local -p port`, not ":port"

## Enable Serial Port on Raspberry Pi (4)

- Get and mount a Serial HAT (e.g. [ModMyPi Serial HAT RS232](https://www.pi-shop.ch/modmypi-serial-hat-rs232))
- `sudo raspi-config`
- Interfacing Options -> Serial
- Disable Login Shell
- Enable Serial Hardware
- Reboot
- `sudo vi /boot/config.txt`
- Add line `dtoverlay=pi3-miniuart-bt` to the end (below enable_uart)
- Reboot
- Test (cat, echo, minicom and putty are all reasonable choices)
- If it doesn't work, it's most likely your cable but you can check if `/boot/cmdline.txt` still contains `console=serial0,115200` and if so remove it.

## Firewall

- `sudo apt-get install ufw`
- `sudo ufw default deny incoming`
- `sudo ufw default allow outgoing`
- `sudo ufw allow port/tcp` (replace port with the custom ssh port you chose)
- `sudo ufw allow http`
- `sudo ufw enable`
- `sudo ufw status verbose` should result in (again, port replaced with your ssh port)

  ```
  Status: active
  Logging: on (low)
  Default: deny (incoming), allow (outgoing), disabled (routed)
  New profiles: skip

  To                         Action      From
  --                         ------      ----
  80/tcp                     ALLOW IN    Anywhere
  port/tcp                   ALLOW IN    Anywhere
  80/tcp (v6)                ALLOW IN    Anywhere (v6)
  port/tcp (v6)              ALLOW IN    Anywhere (v6)
  ```

## Setup signal notifications

In this setup I used a landline number to create a Signal account and link it on the Raspberry Pi.
The process might change in future `signal-cli` versions, so check [Usage](https://github.com/AsamK/signal-cli#usage), [Registration with captcha](https://github.com/AsamK/signal-cli/wiki/Registration-with-captcha) and [Linking other devices (Provisioning)](https://github.com/AsamK/signal-cli/wiki/Linking-other-devices-(Provisioning)).

On main PC:

```bash
./signal-cli -u NUMBER register --captcha SOLVED-CAPTCHA-URL --voice
# listen for code
./signal-cli -u NUMBER verify VERIFICATION-CODE
./signal-cli -u NUMBER updateProfile blablabla
./signal-cli -u NUMBER receive
./signal-cli -u NUMBER send -m "Test from signal-cli" SOME-OTHER-NUMBER
# accept incoming message (trust) on that other number
# add account to group
./signal-cli -u NUMBER receive
./signal-cli -u NUMBER listGroups
# get the group id from this
./signal-cli -u NUMBER send -m "Sent from the command line using signal-cli :)" -g GROUP-ID
# if the message was sent to the group chat, it all works. Can put the number and group-id in config or rather store somewhere for config on PI.
```

Then on the Raspberry Pi install and link it:

I used the appropriate [binary distribution](https://github.com/AsamK/signal-cli/wiki/Binary-distributions) from [projektzentrisch](https://media.projektzentrisch.de/temp/signal-cli/), you'll need to see which one is recent and works.

Alternatively, you could do this with the `signal-cli-rest-api` through docker but I found this easier and it also allows testing. The account data is mounted into the docker container anyway, so they act on the same database.

```bash
curl -o signal-cli0127r2-ubuntu2004-arm64.gz https://media.projektzentrisch.de/temp/signal-cli/signal-cli0127r2_ubuntu2004_arm64.gz
gzip -d signal-cli0127r2-ubuntu2004-arm64.gz
chmod +x signal-cli0127r2-ubuntu2004-arm64
signal-cli link -n "Heating Raspberry Pi"
# copy link and keep process running
```

On main PC again:

```bash
signal-cli -u NUMBER addDevice --uri COPIED-LINK
```

This will end the linking process on the Pi. Then you can send a test message on Raspberry Pi like above. The group-id will not change. Do not forget to receive something first, must be able to fetch a new message from a group.

## Setup heating-data-monitor

To deploy a certain version of heating-data-monitor, you first need to clone said version locally:

- `sudo apt install git`
- `git clone https://github.com/Joelius300/HeatingDataMonitor.git`
- `cd HeatingDataMonitor`

### Setup receiver

The receiver is deployed without docker for multiple reasons including serial port troubles and ease of updates for the other applications without losing data.

- cd into the receiver folder: `cd backend/HeatingDataMonitor.Receiver`
- Edit the `appsettings.json` file with the appropriate connection string for the receiver
- Run install script: `bash deploy-on-rpi.sh`. This will install .NET and publish the app to ~/HeatingDataMonitorReceiver
- Now just register and enable the systemd service
  - `sudo cp heating-data-monitor-receiver.service /etc/systemd/system/heating-data-monitor-receiver.service`
  - `sudo systemctl enable heating-data-monitor-receiver` (then once again with start to kick it off if you're ready)

#### Cheatsheet - Managing the systemd service

- `sudo systemctl start heating-data-monitor-receiver`
- `sudo systemctl stop heating-data-monitor-receiver`
- `sudo systemctl status heating-data-monitor-receiver`
- `sudo journalctl -u heating-data-monitor-receiver -f -p 6` (7=Trace/Debug, 6=Information, 4=Warning, 3=Error, 2=Critical)

### Setup all the other applications

Unlike the receiver, the rest of the application including database, backend, frontend and reverse proxy use docker and docker-compose for fast and simple deployment.

Firstly, install Docker: `curl -sSL https://get.docker.com | sudo sh /dev/stdin`

Then you can install and enable the applications with `sudo docker compose up -d` but you will want to adjust certain config files beforehand.

- docker-compose.yml contains many config options like the api-base-url and passwords.
- database passwords for specific SQL users need to be adjusted in the mounted SQL files before the first run.
- if there is data from an old system you would like to import automatically on the first run, it has to be exported into old-data.csv in the root project folder. The command to export the relevant columns is as follows:
  ```bash
  psql -d "HeatingDataMonitor" -h localhost -U heatingDataMonitorUser -c "\copy \"HeatingData\" (\"SPS_Zeit\",\"ReceivedTime\",\"Kessel\",\"Ruecklauf\",\"Abgas\",\"CO2_Soll\",\"CO2_Ist\",\"Saugzug_Ist\",\"Puffer_Oben\",\"Puffer_Unten\",\"Platine\",\"Betriebsphase_Kessel\",\"Aussen\",\"Vorlauf_HK1_Ist\",\"Vorlauf_HK1_Soll\",\"Betriebsphase_HK1\",\"Vorlauf_HK2_Ist\",\"Vorlauf_HK2_Soll\",\"Betriebsphase_HK2\",\"Boiler_1\",\"DI_0\",\"DI_1\",\"DI_2\",\"DI_3\",\"A_W_0\",\"A_W_1\",\"A_W_2\",\"A_W_3\",\"A_EA_0\",\"A_EA_1\",\"A_EA_2\",\"A_EA_3\",\"A_EA_4\",\"A_PHASE_0\",\"A_PHASE_1\",\"A_PHASE_2\",\"A_PHASE_3\",\"A_PHASE_4\") TO '/mnt/data_backups/$(date +%Y-%m-%dT%H_%M_%S%z).csv' DELIMITER ',' CSV HEADER;"
  ```

## Setup backup job

Run these commands to setup a job that creates a db backup every friday at 03:00. It's a root job because it invokes a command inside a docker container. \
Make sure to edit the backup script and specify the desired backup location as well as the backup user's db password.

```bash
chmod +x /home/pi/HeatingDataMonitor/backend/HeatingDataMonitor.Database/backup.sh
(sudo crontab -l 2>/dev/null; echo "0 3 * * 5 /home/pi/HeatingDataMonitor/backend/HeatingDataMonitor.Database/backup.sh") | sudo crontab -
```

If you find the need to inspect the logs of the cron job, it won't be as easy as you think. The output is mailed to you so install `postfix` with "local only" mode in the setup prompt. Then use `sudo tail -f /var/mail/root`.

### Mount NFS drive as backup folder

In the NAS' control panel, you need to add an NFS permission record for the specific folder with the hostname (without the .local) of the rpi.

Then you can mount it as follows:

```
sudo apt install nfs-common
export NAS_HOST=your NAS' hostname/ip
export MOUNT_PATH=NAS' mount path as dictated by the control panel
export BACKUP_FOLDER=/mnt/data_backups
sudo mkdir $BACKUP_FOLDER
sudo mount -t nfs -vvvv $NAS_HOST:$MOUNT_PATH $BACKUP_FOLDER
```

If it doesn't work, make sure you can see the folder you want to mount with `showmount -e $NAS_HOST`.

After successfully mounting, touch a file in the mounted folder and see if it shows up on the drive.

To make it persistent across reboots, add the following line to `/etc/fstab` (of course manually replacing the env variables or echoing it first).

```
$NAS_HOST:$MOUNT_PATH $BACKUP_FOLDER nfs defaults 0 0
```

**Sources:**

- https://linuxize.com/post/how-to-mount-an-nfs-share-in-linux/
- https://kb.synology.com/en-us/DSM/tutorial/How_to_access_files_on_Synology_NAS_within_the_local_network_NFS
- https://unix.stackexchange.com/questions/106122/mount-nfs-access-denied-by-server-while-mounting-on-ubuntu-machines

### Archive weather data from nearby weather stations

Add the following user cron job:

```bash
*/10 * * * * bash ~/meteo-fetch.sh
```

`~/meteo-fetch.sh`
```bash
#!/bin/sh
LINES=$(curl https://data.geo.admin.ch/ch.meteoschweiz.messwerte-aktuell/VQHA80.csv -s | grep -E "^(stations|you|care|about)")
{ echo "${LINES}" >> /mnt/data_backups/meteo.csv; } || { echo "${LINES}" >> ~/backup-meteo.csv; }
```

Check logs with `sudo tail -f /var/mail/pi`

If there was an outage and it couldn't write to the mounted drive, use this to rescue the missed data AFTER rebooting and fixing the underlying issue (otherwise it can't write to `/mnt/data_backups`):
Sometimes a reboot is enough, other times, you need `sudo mount -a` and similar shenanigans. Also, don't forget to start the NAS again after a power outage.

```bash
cat backup-meteo.csv >> /mnt/data_backups/meteo.csv
```

then you can delete the backup file `rm backup-meteo.csv`

**Sources:**

- https://opendata.swiss/en/dataset/automatische-wetterstationen-aktuelle-messwerte/resource/53304634-b947-4415-b19a-c947827f331b
