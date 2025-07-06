# MCP Simple Server - NASA Mars Photos API

This MCP server provides tools to interact with NASA's Mars Photos API, allowing you to retrieve information about Mars rovers and their photos.

## Setup

### NASA API Key

To use the NASA Mars Photos API with higher rate limits, you'll need to get a free API key from [NASA's API portal](https://api.nasa.gov/).

1. Visit https://api.nasa.gov/
2. Sign up for a free account
3. Generate an API key for the Mars Photos API

### Environment Variable

Set the `NASA_API_KEY` environment variable with your API key:

**Windows (PowerShell):**
```powershell
$env:NASA_API_KEY="your-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set NASA_API_KEY=your-api-key-here
```

**Linux/macOS:**
```bash
export NASA_API_KEY=your-api-key-here
```

### Fallback

If no API key is provided, the server will use the `DEMO_KEY` which has limited rate limits but is sufficient for testing and development.

## Available Tools

- `get-rovers`: Returns a list of all available Mars rovers
- `get-rover-photo`: Returns photos for a specific rover and date

## Testing with Claude Desktop

### 1. Configure Claude Desktop

Create or edit your `claude_desktop_config.json` file (usually located in your user directory):

```json
{
  "mcpServers": {
    "mars-photos": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\ABSOLUTE\\PATH\\TO\\mcp-simple-server",
        "--no-build"
      ],
      "env": {
        "NASA_API_KEY": "your-api-key-here"
      }
    }
  }
}
```

**Important Notes:**
- Replace `C:\\ABSOLUTE\\PATH\\TO\\mcp-simple-server` with the actual absolute path to your project directory
- Replace `your-api-key-here` with your actual NASA API key
- The `env` section allows you to set environment variables specifically for this MCP server
- Use forward slashes (`/`) on macOS/Linux instead of backslashes

### 2. Restart Claude Desktop

After updating the configuration, restart Claude Desktop for the changes to take effect.

### 3. Test the Tools

Once connected, you can test the tools by asking Claude:

- "What Mars rovers are available?"
- "Show me photos from the Curiosity rover on 2015-6-3"

## Testing with VS Code

### 1. Configure VS Code Settings

Add the following to your VS Code settings (`settings.json`):

```json
{
  "mcp": {
    "inputs": [],
    "servers": {
      "mars-photos": {
        "command": "dotnet",
        "args": [
          "run",
          "--project",
          "C:\\ABSOLUTE\\PATH\\TO\\mcp-simple-server",
          "--no-build"
        ],
        "env": {
          "NASA_API_KEY": "your-api-key-here"
        }
      }
    }
  }
}
```

### 2. Test the Tools

Use the VS Code command palette to interact with the MCP tools or use the integrated chat interface.

When using the Copilot chat interface, switch to **Agent** mode. You may have best results selecting a Claude model instead of one of the default GPT-4x models. The OpenAI models sometimes avoid using tools, even when you explicitly ask them to.

## Running the Server Locally

For direct testing without MCP clients:

```bash
dotnet run
```

The server will start and listen for MCP client connections via STDIO transport.

## Example Usage

Once connected to an MCP client, you can use the tools like this:

1. **Get all rovers:**
   - Tool: `get-rovers`
   - Returns: List of all available Mars rovers with their details

2. **Get rover photos:**
   - Tool: `get-rover-photo`
   - Parameters: 
     - `roverName`: "curiosity", "opportunity", "spirit", "perseverance"
     - `earthDate`: "2015-6-3" (YYYY-M-D format)
   - Returns: Photos taken by the specified rover on the given date

## Troubleshooting

- **Server not starting**: Ensure you have .NET 9.0 installed
- **API key issues**: Verify the `NASA_API_KEY` environment variable is set correctly
- **Path issues**: Use absolute paths in the MCP configuration
- **Permission errors**: Ensure the project directory is accessible 