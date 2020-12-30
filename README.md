# DiscordHealthBot
A simple bot that check website health and then broadcast results to a Discord webhook.

# Configuration (appsettings.json)
Create an appsettings.json. This file should be located in the same folder as your binary.

## appsettings.json example
```json
{
    "JobSettings": {
        "TimeInterval": 30,
        "PollingInterval": 10,
        "FamilyReporting": true,
        "SendAlert": true,
        "AlertFloor": 10000,
	"FixedTime": true,
	"TimeUnit" :  "hour",
	"ConnectionStrings": "myredisserver.com, password=xxx",
	"StoreData" : false
    },
    "EndPoints": [
          {
            "Address": "https://www.google.fr",
            "FamilyName" : "SearchEngine"
          },
          {
            "Address": "https://www.qwant.fr",
            "FamilyName" : "SearchEngine"
          },
          {
            "Address": "https://www.twitter.com",
            "FamilyName" : "Social"
          }
    ],
    "DiscordWebhook": "https://discordapp.com/api/webhooks/xxxx/xxxxx"
}

```

## configuration variables

`FamilyReporting` : if true, broadcast average latency per family instead of per endpoint reporting

`TimeInterval` : Time (in seconds) between two discord annoucement

`PollingInterval`: Time (in seconds) between two polling loop

`EndPoints` : Endpoints to check

`DiscordWebHook` : Discord Web Hook Url

`SendAlert`: Immedialty send an alert when an endpoint is laggy

`AlertFloor` : Time in milliseconds that trigger `SendAlert`

`FixedTime` : Trigger the broadcast at a fixed time unit, it cancels TimeInterval

`TimeUnit` : Unit of time (string ) which configures FixedTime: "day", "hour", "minute"

`StoreData` : allowed application to store data using Redis to ensure data continuity in case of random crash

`ConnectionStrings` : connection strings to Redis Server

# Docker Instructions 

docker pull pehanxi/discordhealthbot
