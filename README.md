# DiscordHealthBot
A simple bot that check website health and then broadcast results to a Discord webhook.

# Configuration (appsettings.json)
Create an appsettings.json. This file should be located in the same folder as your binary.

## appsettings.json example
```
{

    "JobSettings": {
        "TimeInterval": 30
    },
    "EndPoints": [
        "https://www.google.com",
        "https://www.twitter.com"
    ],
    "DiscordWebhook": "https://discordapp.com/api/webhooks/xxxx/xxxxx"
}

```

## configuration variables

`TimeInterval` : Time (in seconds) between two loops

`EndPoints` : Endpoints to check

`DiscordWebHook` : Discord Web Hook Url

# Docker Instructions 

docker pull pehanxi/discordhealthbot
