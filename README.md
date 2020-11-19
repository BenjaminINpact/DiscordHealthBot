# DiscordHealthBot
A simple bot that check website health and then broadcast results to a Discord webhook.

# Configuration (appsettings.json)
Create an appsettings.json. This file should be located in the same folder as your binary.

## appsettings.json example
```json
{

    "JobSettings": {
        "TimeInterval": 30,
        "FamilyReporting": true
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

`FamilyReporting` : if true

`TimeInterval` : Time (in seconds) between two loops

`EndPoints` : Endpoints to check

`DiscordWebHook` : Discord Web Hook Url

# Docker Instructions 

docker pull pehanxi/discordhealthbot
