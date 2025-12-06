# ValheimDiscordBot

A Discord bot that monitors and displays the current player count for a Valheim server.

## Features

- Real-time player count updates in bot's status
- Configurable update interval
- Docker support with environment variables
- Custom slash commands (e.g., `/echo`)

## Configuration

### Environment Variables

The bot can be configured using environment variables (recommended for Docker) or `appsettings.json`:

| Variable | Description | Default |
|----------|-------------|---------|
| `DiscordToken` | Discord bot token (required) | - |
| `ValheimServer__Host` | Valheim server hostname or IP | `apaluchdev.com` |
| `ValheimServer__QueryPort` | Valheim server query port | `2457` |
| `Bot__StatusUpdateIntervalSeconds` | How often to update player count (seconds) | `60` |
| `Bot__CommandPrefix` | Command prefix for bot commands | `/` |

### User Secrets (Development)

For local development, use .NET User Secrets to store your Discord token:

```bash
dotnet user-secrets set "DiscordToken" "your_discord_token_here"
```

### appsettings.json

You can also configure the bot using `appsettings.json`:

```json
{
  "ValheimServer": {
    "Host": "your-server.com",
    "QueryPort": 2457
  },
  "Bot": {
    "StatusUpdateIntervalSeconds": 60,
    "CommandPrefix": "/"
  }
}
```

## Quick Start with Docker

### Prerequisites
- Docker installed on your system
- A Discord bot token ([Create one here](https://discord.com/developers/applications))

### Running with Docker

1. **Build the Docker image**
   ```bash
   docker build -t valheim-discord-bot .
   ```

2. **Run the container**
   ```bash
   docker run -d \
     --name valheim-discord-bot \
     --restart unless-stopped \
     -e DiscordToken="YOUR_DISCORD_TOKEN" \
     -e ValheimServer__Host="your-server.com" \
     -e ValheimServer__QueryPort="2457" \
     -e Bot__StatusUpdateIntervalSeconds="60" \
     -e Bot__CommandPrefix="/" \
     valheim-discord-bot
   ```

3. **View logs**
   ```bash
   docker logs -f valheim-discord-bot
   ```

4. **Stop and remove the container**
   ```bash
   docker stop valheim-discord-bot
   docker rm valheim-discord-bot
   ```

### Using Pre-built Image (If available on Docker Hub)

```bash
docker run -d \
  --name valheim-discord-bot \
  --restart unless-stopped \
  -e DiscordToken="YOUR_DISCORD_TOKEN" \
  -e ValheimServer__Host="your-server.com" \
  -e ValheimServer__QueryPort="2457" \
  apaluch/valheim-discord-bot:latest
```

## Running Locally (Without Docker)

### Prerequisites
- .NET 10 SDK installed

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/apaluchdev/ValheimDiscordBot.git
   cd ValheimDiscordBot
   ```

2. **Set your Discord token using user secrets**
   ```bash
   dotnet user-secrets set "DiscordToken" "your_discord_token_here"
   ```

3. **Update `appsettings.json` with your server details (optional)**
   ```json
   {
     "ValheimServer": {
       "Host": "your-server.com",
       "QueryPort": 2457
     }
   }
   ```

4. **Run the bot**
   ```bash
   dotnet run
   ```

## Docker Management Commands

### View Running Containers
```bash
docker ps
```

### View All Containers (including stopped)
```bash
docker ps -a
```

### View Logs
```bash
docker logs -f valheim-discord-bot
```

### Restart the Bot
```bash
docker restart valheim-discord-bot
```

### Update to Latest Version
```bash
# Stop and remove the current container
docker stop valheim-discord-bot
docker rm valheim-discord-bot

# Pull latest changes
git pull

# Rebuild and start
docker build -t valheim-discord-bot .
docker run -d \
  --name valheim-discord-bot \
  --restart unless-stopped \
  -e DiscordToken="YOUR_DISCORD_TOKEN" \
  -e ValheimServer__Host="your-server.com" \
  -e ValheimServer__QueryPort="2457" \
  valheim-discord-bot
```

### Remove Everything (Clean Slate)
```bash
docker stop valheim-discord-bot
docker rm valheim-discord-bot
docker rmi valheim-discord-bot
```

## Troubleshooting

### Bot won't connect to Discord
- Verify your `DISCORD_TOKEN` is correct
- Check logs: `docker logs -f valheim-discord-bot`
- Ensure the bot has proper permissions in your Discord server

### Can't connect to Valheim server
- Verify the server host and query port are correct
- The query port is typically the game port + 1 (default: 2457)
- Ensure the Valheim server has query enabled
- Check firewall rules allow UDP traffic on the query port

### Bot crashes on startup
- Check logs for error messages: `docker logs valheim-discord-bot`
- Verify all required environment variables are set
- Ensure the Discord token is valid

### Environment variables not working
- In docker run, use format: `-e KEY="value"` (quotes recommended)
- Variables use double underscore `__` for nested config: `ValheimServer__Host`

## Development

### Adding Commands

Create a new command class in `Commands/` folder following the example in `Commands/Echo/EchoCommand.cs`:

```csharp
using Discord.Commands;

namespace ValheimDiscordBot.Commands.YourCommand
{
    public class YourCommand : ModuleBase<SocketCommandContext>
    {
        [Command("yourcommand")]
        [Summary("Description of your command")]
        public async Task ExecuteAsync([Remainder] string input)
        {
            await ReplyAsync($"You said: {input}");
        }
    }
}
```

### Building for Production

```bash
# Build optimized release
docker build --build-arg BUILD_CONFIGURATION=Release -t valheim-discord-bot:latest .

# Tag for registry
docker tag valheim-discord-bot:latest yourusername/valheim-discord-bot:latest

# Push to registry
docker push yourusername/valheim-discord-bot:latest
```

## Getting a Discord Bot Token

1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application" and give it a name
3. Go to the "Bot" section
4. Click "Add Bot"
5. Under "Token", click "Copy" to copy your bot token
6. Under "Privileged Gateway Intents", enable "Message Content Intent"
7. Go to "OAuth2" > "URL Generator"
8. Select scopes: `bot`, `applications.commands`
9. Select bot permissions: `Send Messages`, `Read Messages/View Channels`
10. Copy the generated URL and open it in your browser to invite the bot

## License

MIT

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.