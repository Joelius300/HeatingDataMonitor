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

# Questions and resources
There were some questions and issues that came up along the journey as well as some resources I used.
- [Correct way of specifying similar locations for nginx (StackOverflow)](https://superuser.com/questions/1559030/correct-way-of-specifying-similar-locations-for-nginx)
- [Selection isn't highlighted (Issue in ngx-daterangepicker)](https://github.com/fetrarij/ngx-daterangepicker-material/issues/295)
- [How (not) to use Machine Learning for time series forecasting: Avoiding the pitfalls](https://towardsdatascience.com/how-not-to-use-machine-learning-for-time-series-forecasting-avoiding-the-pitfalls-19f9d7adf424)
- [Forecasting: Principles and Practice (by Rob J Hyndman and George Athanasopoulos)](https://otexts.com/fpp3/)
- [An Overview of ML.Net (by KTL Solutions)](https://www.erpsoftwareblog.com/2019/04/an-overview-of-ml-net/)
- [Angular Docs](https://angular.io/docs)
- [Angular Material Docs](https://material.angular.io/)
- [SignalR Core Docs](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1)

# Credits / Licenses
This project is licensed under the [GNU Affero General Public License Version 3](https://www.gnu.org/licenses/agpl-3.0.en.html). This applies to every file within this repository unless there is a license notice at the top of the file that says otherwise.  
If you have any questions about this or would like to use a specific part under a different license, please open an issue and I'll try my best to assist.

## Used in the project
- Frontend page icon (TODO: SELECT AND ATTRIBUTE)
  - https://icons8.com/icon/22561/temperature-inside (https://icons8.com/license)
  - https://www.flaticon.com/free-icon/thermometer_899746 (https://support.flaticon.com/hc/en-us/articles/207248209-How-I-must-insert-the-attribution-)
- [angular/components](https://github.com/angular/components) ([MIT](https://github.com/angular/components/blob/master/LICENSE))  
  Component infrastructure and Material Design components for Angular
- [angular/flex-layout](https://github.com/angular/flex-layout) ([MIT](https://github.com/angular/flex-layout/blob/master/LICENSE))  
  Provides HTML UI layout for Angular applications; using Flexbox and a Responsive API
- [μPlot](https://github.com/leeoniya/uPlot) ([MIT](https://github.com/leeoniya/uPlot/blob/master/LICENSE))  
  A small, fast chart for time series, lines, areas, ohlc & bars
- [ngx-daterangepicker-material](https://github.com/fetrarij/ngx-daterangepicker-material) ([MIT](https://github.com/fetrarij/ngx-daterangepicker-material/blob/master/LICENSE))  
  Pure Angular 2+ date range picker with material design theme
- [Moment.js](https://momentjs.com/) ([MIT](https://github.com/moment/moment/blob/develop/LICENSE))  
  Parse, validate, manipulate, and display dates in javascript.
- [CsvHelper](https://github.com/JoshClose/CsvHelper)  ([Apache-2.0/Ms-Pl](https://github.com/JoshClose/CsvHelper/blob/master/LICENSE.txt))  
  Library to help reading and writing CSV files
