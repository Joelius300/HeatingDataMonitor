# Summary
This is a web app which allows me and my family to monitor the heating unit of our house in real time.

The interface is in swiss german. Because the heating unit is german, the names (class, column) are also specified in german which allows for easier reference in manuals and co.

The data retrieval, storage and sending is done with an [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core) backend using [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) and [SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction).  
Reading the semicolon separated values from the heating unit is done with [CsvHelper](https://github.com/JoshClose/CsvHelper).  
The user interface is realized with an [Angular](https://angular.io/) frontend using [flex-layout](https://github.com/angular/flex-layout) and [angular-material](https://material.angular.io/).  
Both front- and backend are hosted on a [Raspberry Pi](https://www.raspberrypi.org/) 4 (with [Raspbian Buster Lite](https://www.raspberrypi.org/downloads/raspberry-pi-os/)) using [NGINX](https://www.nginx.com/) and [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel).  
The Raspberry Pi is connected to the heating unit via [RS232](https://en.wikipedia.org/wiki/RS-232) using a [ModMyPi Serial HAT](https://www.pi-shop.ch/modmypi-serial-hat-rs232).  
The data management below EF Core is done with [PostgreSQL](https://www.postgresql.org/).

I'm using this application as an opportunity to gain experience in several areas including front- and backend web development, database management, hosting and server management (on a small scale with linux), code management (git & GitHub), working with hardware (Raspberry Pi & RS232) and probably more.

# Goal
The app isn't finished yet. The end-goal is to have the following features:

- Real time view of the current state of the heating unit
- History view with graph to inspect the activity within a certain timespan
- Real time graph of archived datapoints in the last 15 minutes (so the 15 latest entries, updated in real time)
- A forecast when the heating unit needs to be fired up for the boiler temp not to get below a certain point (using ML)

Also, I don't know what many of the values mean nor their correct unit or mapping (for enums). I'll have to contact the manufacturer of the heating unit in order to correctly display and interpret those.

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

## Hosting with nginx
This could probably be improved but it's working and I get why it's working - for now that's good enough :)

Composed from the following sources:

- https://github.com/diginex/nginx-spa/blob/master/default.conf (SPA)
- https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-3.1#configure-nginx (API)
- https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/blazor/server?view=aspnetcore-3.1#linux-with-nginx (SignalR / WebSocket)

### Install
- `sudo apt-get install nginx`

### Manage
- `sudo service nginx start`
- `sudo service nginx stop`
- `sudo service nginx restart`

### /etc/nginx/sites-available/default
```
server {
    listen       80;
    server_name  localhost;

    root /var/www/html;
    index index.html;

    location /api {
        proxy_pass         http://localhost:5000/api;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }

    location /realTimeFeed {
        proxy_pass         http://localhost:5000/realTimeFeed;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection $connection_upgrade;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }

    location / {
        root   /usr/share/nginx/html;
        try_files $uri /index.html; # redirect all request to index.html
    }

    error_page   500 502 503 504  /50x.html;
    location = /50x.html {
        root   /usr/share/nginx/html;
    }
}
```

### Add to the top of the http region in /etc/nginx/nginx.conf
I'm still not sure if that's the correct place to put this but it seems like it has to go inside the `http` context which is defined in here.

```
# For SignalR
map $http_upgrade $connection_upgrade {
    default Upgrade;
    ''      close;
}
```
