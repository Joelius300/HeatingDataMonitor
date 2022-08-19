# HeatingDataMonitor Backend

## Architecture

Although I recently did a giant overhaul of the backend architecture with a clear net positive, I am not entirely happy with how it turned out - especially the database layer. I already have ideas how this could/should be structured instead, which will be probably be reevaluated and implemented as part of the push notifications (see ["Push notifications"-project](https://github.com/Joelius300/HeatingDataMonitor/projects/4)).

There are three main parts within the backend. The database is the center piece of the backend. Then on one side there is the receiver, which continuously parses the data from the heating unit and appends it to the database. Only the receiver has write access to the database. On the other hand, there is the API, which handles the communication with the frontend by querying the db for certain time periods as well as listening for new records added by the receiver and notifying the frontend clients in real time.

![architecture.svg](heating_data_monitor_architecture.drawio.svg)
