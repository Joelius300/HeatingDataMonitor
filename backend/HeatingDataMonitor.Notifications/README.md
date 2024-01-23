# Notifications

Also read the [Alerting readme](../HeatingDataMonitor.Alerting/README.md).

There are many approaches for notifications:

1. Using [CliWrap](https://github.com/Tyrrrz/CliWrap) to call `signal-cli`. This works well locally but inside Docker it's annoying to set up. Most lightweight solution and has the least dependencies.
2. Use [signal-cli-rest-api](https://github.com/bbernhard/signal-cli-rest-api) (builds on `signal-cli`) and call it with [Refit](https://github.com/reactiveui/refit). This means another service has to constantly be running and there is a further abstraction layer (dependency) between this app and signal. Similar complexity to call, a bit less lightweight but simpler to set up and manage as it's just another, already set up, docker service running in the stack.
3. Use [Apprise](https://github.com/caronc/apprise) on top of signal-cli-rest-api to unify notification providers and make working with different providers easy. Highest abstraction and cost, overkill for this project, but probably very nice when you want to have many different providers.
4. Probably more, haven't investigated further

I started with approach 1 but switched to approach 2 because I didn't want to fiddle around with the low level library dependencies that `signal-cli` needs and I would have to manage inside the existing backend docker image.
Since there's just one provider and the entire system is very simple, I didn't split the different providers into packages but to be extra clean and separate the dependencies etc. I would have done so.
