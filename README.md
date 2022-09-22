# Project Iris
Working with the Twitter API

## 30k'
The project will leverage `Docker-Compose` as an orchestration provider, 
[Redis](https://redis.io/ "Redis") as our Queue provider and data persistance layer as data longevity is not the current goal for this project. 
We'll be managing the following containers:
1. [Redis](https://redis.com/)
2. [RedisInsight](https://redis.com/redis-enterprise/redis-insight/)
3. Ingress .Net Core 
4. Normalization .Net Core Worker(s)
5. Asp.Net Core Web App

## The Back End
![Activity](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/Flow.puml "Activity")

Tweet data will continuously be received via stream by a consumption worker that remains connected to the Twitter stream. 
> This worker will only receive and queue so as to reduce the chance for falling behind the Twitter api due to latency brought on by our processing.

When a new Tweet is queued, a side-car Worker will receive the tweet and process the data into our format before persisting to Redis. 
> This stage will provide us scalability as if we ever fall behind Twitter and our Queue grows too large, we'll have the ability to introduce additional workers to catch us up

We'll provide statistics on our Redis cache using 
[Redis Insight](https://redis.com/redis-enterprise/redis-insight/)
which will be made available locally at
[localhost:8091](http://localhost:8091)

Finally, any users actively viewing the metric dashboard will receive an updated set of metric data via 
[Server Sent Event](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events "MDN SSE").
Said dashboard will be made available when running locally at
[LocalHost:8080](http://localhost:8080)

To get started, clone this repo locally, open a terminal window, and head to the `Src` directory.
To take full advantage, make sure you have
[Docker](https://www.docker.com/)
and
[Docker Compose](https://docs.docker.com/compose/)
setup on you machine. Both of which should be around if you have 
[Docker Desktop](https://www.docker.com/products/docker-desktop) 
installed.

With the source downloaded and a terminal at the `Src` directory, you'll need to sign up for a [Twitter developer](https://developer.twitter.com/en) account.
Once you have an account you'll need to follow all the steps to get a `Bearer Token` which should be documented and outlined during signup.

Create a new `twitter.json` file locally to act as a kind of secrets file which will hold your `Bearer token`.
This project will use this as an optional app config file and looks to settings namespaced with `IRIS_`.
In this case a single entry is necessary for this file and should look as so:
``` Json
{
	"IRIS_TWITTER_BEARER": "<bearer token>"
}
```

Save this file and make sure it's in the `Src` directory, then run the following:
```
docker-compose build
docker-compose up -d
```
>Note: When starting services, the compose file will setup a single instance of each service however in this model the workers represent the scalable dimension of the solution. You can choose to scale out the number of workers you spawn to emulate a scenario where we're not processing Twitter data fast enough to keep up. To spawn 2 workers rather than 1, the new set of commands may look as so:
```
docker-compose build
docker-compose up -d --scale worker=2
```

When all services have started and are running, open a browser to 
[LocalHost:8080](http://localhost:8080) 
and watch the streaming Twitter goodness.

## Arts and Crafts

![System Landscape](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/structurizr-2-SystemLandscape.puml "Landscape")

### Emoji
![Emoji System Context](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/structurizr-2-iamcalemojidata-SystemContext.puml "Emoji System Context")

![Emoji Container](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/structurizr-2-iamcalemojidata-Container.puml)

### Twitter
![Twitter System Context](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/structurizr-2-Twitter-SystemContext.puml "Emoji System Context")

![Twitter Container](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/structurizr-2-Twitter-Container.puml)
