# Project Iris
Working with the Twitter API

## 30k'
The project will leverage `Docker-Compose` as an orchestration provider, 
[Redis](https://redis.io/ "Redis") as our Queue provider and data persistance layer as data longevity is not the current goal for this project. 
We'll be managing the following containers:
1. Redis
2. Normalization .Net Core Worker(s)
3. Ingress .Net Core 
4. Asp.Net Core Web App

## The Back End
![Activity](https://www.plantuml.com/plantuml/proxy?src=https://raw.githubusercontent.com/jbudbo/ProjectIris/master/puml/Flow.puml "Activity")

Tweet data will continuously be received via stream by a consumption worker that remains connected to the Twitter stream. 
> This worker will only receive and queue so as to reduce the chance for falling behind the Twitter api due to latency brought on by our processing.

When a new Tweet is queued, a side-car Worker will receive the tweet and process the data into our format before persisting to Redis. 
> This stage will provide us scalability as if we ever fall behind Twitter and our Queue grows too large, we'll have the ability to introduce additional workers to catch us up

We'll provide statistics on our Redis cachie using 
[Redis Insight](https://redis.com/redis-enterprise/redis-insight/)
which will be made available locally at
[localhost:8091](http://localhost:8091)

Finally, any users actively viewing the metric dashboard will receive an updated set of metric data via 
[Server Sent Event](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events "MDN SSE").
Said dashboard will be made available when running locally at
[LocalHost:8080](http://localhost:8080)