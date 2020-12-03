# DiscordHealthBot
A simple bot that check website health and then broadcast results to a Discord webhook.

# Configuration (appsettings.json)
Create an appsettings.json. This file should be located in the same folder as your binary.

## appsettings.json example
```json
{
	"ConnectionStrings": "",
    "JobSettings": {
        "TimeInterval": 30,
        "PollingInterval": 10,
        "FamilyReporting": true,
        "SendAlert": true,
        "AlertFloor": 10000,
		"FixedTime": true,
		"TimeUnit" :  "hour",
    },
    "EndPoints": [
          {
            "Address": "https://www.google.fr",
            "Family" : "SearchEngine"
          },
          {
            "Address": "https://www.qwant.fr",
            "Family" : "SearchEngine"
          },
          {
            "Address": "https://www.twitter.com",
            "Family" : "Social"
          }
    ],
    "DiscordWebhook": "https://discordapp.com/api/webhooks/xxxx/xxxxx"
}

```

## configuration variables

`ConnectionStrings` : database connection string to ensure data continuity in case of random crash

`FamilyReporting` : if true, broadcast average latency per family instead of per endpoint reporting

`TimeInterval` : Time (in seconds) between two discord annoucement

`PollingInterval`: Time (in seconds) between two polling loop

`EndPoints` : Endpoints to check

`DiscordWebHook` : Discord Web Hook Url

`SendAlert`: Immedialty send an alert when an endpoint is laggy

`AlertFloor` : Time in milliseconds that trigger `SendAlert`

`FixedTime` : Trigger the broadcast at a fixed time unit, it cancels TimeInterval

`TimeUnit` : Unit of time (string ) which configures FixedTime: "day", "hour", "minute"

# Docker Instructions 

docker pull pehanxi/discordhealthbot
