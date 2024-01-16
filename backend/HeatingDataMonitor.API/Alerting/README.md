# Alerting

The alerting implemented in this project is very simple at the moment. \
The `AlertMonitor` service periodically updates and checks the different alerts.
When an alert proclaims it should be fired, it will produce a notification that is forwarded it to all the registered notification providers.
After the notifications were sent, the monitoring service resets the alerts.
If an alert could not be sent, it will not be reset and therefore another attempt at sending the notification will be made in the next iteration.

In an industrial setting, this would be a separate application running in a separate container to ensure independence from the web app.
I decided against going that far for simplicity and because the web API doesn't have many responsibilities at the moment.

To go all the way I would:
- Split this into its own project and application
- Setup container where this monitoring and alerting would run
- Use [Apprise](https://github.com/caronc/apprise) under the hood
- Allow users to specify which alerts they want to get and where they want to get them
- Exchange heartbeats with the web API, so you can also alert when the API is down
- Additional redundancy to ensure no critical notifications are missed but this is not my expertise and I'd have to look into it

Notes on implementation ideas:

- `AlertMonitor` takes an `IHeatingDataReceiver`, `ICollection<INotificationProvider>`, `ICollection<IAlert>`
- whenever it gets new data it loops over all the alerts and calls `Update()` on them
- afterwards it loops again and checks `PendingNotification` for non-null
- if it is non-null, it loops through the notification providers and passes it to `Fire()`
- afterwards it calls `ResetNotification` on the alert
- with try-catch it is guarded against notification-firing-failures
- In this setup it doesn't make sense to batch them up but in an industrial setting it might
- There will just be one notification service which directly calls the `signal-cli` using CliWrap. The sender and the group to send to are configurable.
- Before sending, must call receive once. Maybe have some check in place that only calls receive if it's been more than 24h since the last time.
- Document the setup of the `signal-cli` but don't duplicate their docs.
