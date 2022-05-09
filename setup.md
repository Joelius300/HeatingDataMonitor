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

TODO: Explain all the steps necessary before setting it up (adjusting env vars in the compose file, etc.)

You can install and enable it with `docker-compose up -d`

## Setup backup job
TODO: document how to register the cron script to backup the current db contents to a csv on the network share.