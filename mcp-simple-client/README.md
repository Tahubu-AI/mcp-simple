# MCP Simple Client

A .NET client application that demonstrates the Model Context Protocol (MCP) by connecting to a Mars Photos API server. This client provides an interactive chat interface with Claude AI to query Mars rover photos through MCP tools.

## Features

- **Interactive Menu System**: Choose between different conversation modes without command-line arguments
- **Conversation History**: Optional retention of chat history for context-aware conversations
- **System Prompt Control**: Toggle date tool instructions to demonstrate different behaviors
- **Rate Limiting Protection**: Graceful handling of API rate limits with user-friendly error messages
- **Real-time Streaming**: See AI responses as they're generated

## Prerequisites

- .NET 9.0 SDK
- Anthropic API key (Claude access)
- The `mcp-simple-server` project must be built and available

## Setup

1. **Set your Anthropic API key** using one of these methods:
   - User Secrets: `dotnet user-secrets set "ANTHROPIC_API_KEY" "your-api-key-here"`
   - Environment Variable: `ANTHROPIC_API_KEY=your-api-key-here`
   - Configuration file: Add to `appsettings.json`

2. **Build the server project** (if not already built):
   ```bash
   cd ../mcp-simple-server
   dotnet build
   ```

## Running the Client

### From Command Line

```bash
cd mcp-simple-client
dotnet run
```

### From Visual Studio

- Open the solution in Visual Studio
- Set `mcp-simple-client` as the startup project
- Press F5 or use Debug > Start Debugging

## Interactive Menu System

When you start the application, you'll see a configuration menu with these options:

### Quick Options (1-4)

1. **History Enabled + System Prompt Enabled** - Full context with date tool instructions
2. **History Enabled + System Prompt Disabled** - Full context without date tool instructions  
3. **History Disabled + System Prompt Enabled** - Independent queries with date tool instructions
4. **History Disabled + System Prompt Disabled** - Independent queries without date tool instructions

### Custom Configuration (Option 5)
- Toggle conversation history on/off
- Toggle system prompt (date tool instructions) on/off
- Set custom history limit (if history is enabled)

## Usage

### Basic Commands
- **Type your question** and press Enter to chat with Claude
- **Type `menu`** to change configuration settings anytime
- **Type `exit`** to quit the application

### Sample Questions

Here are some example questions you can try:

#### Rover Information
- **"Which rovers are available?"** - Lists all available Mars rovers
- **"What tools are available?"** - Shows available MCP tools

#### Photo Queries
- **"Show me Curiosity photos from July 2nd"** - Gets photos from a specific date
- **"Show me one of the NAV_RIGHT_B photos"** - Gets photos from a specific camera
- **"I would like to know more about this particular image"** - Gets detailed information about a photo

#### Date-Related Queries (demonstrates system prompt)
- **"What photos were taken today?"** - Uses current date
- **"Show me photos from yesterday"** - Uses relative date
- **"Get photos from last week"** - Uses relative date range

## Configuration Modes Explained

### Conversation History
- **Enabled**: The AI remembers previous messages and can reference them
- **Disabled**: Each query is independent, no context from previous messages

### System Prompt
- **Enabled**: AI automatically uses the `get-current-date` tool for date-related questions
- **Disabled**: AI won't automatically use date tools, may give less accurate date responses

## Demo Scenarios

### Scenario 1: Date Tool Demonstration

1. Choose **Option 3** (History Disabled + System Prompt Enabled)
2. Ask: "What Curiosity photos were taken today?"
3. Observe how the AI automatically calls the date tool
4. Switch to **Option 4** (History Disabled + System Prompt Disabled)
5. Ask the same question and notice the difference

### Scenario 2: Conversation History Demonstration

1. Choose **Option 1** (History Enabled + System Prompt Enabled)
2. Ask: "Which rovers are available?"
3. Follow up with: "Tell me more about Curiosity"
4. Follow up with: "Show me Curiosity photos from July 2nd"
5. When it lists the available photos from various cameras that day, follow up with "Show me a photo from [camera name]"
6. Notice how the AI references the previous conversation
7. Switch to **Option 3** (History Disabled + System Prompt Enabled)
8. Ask the same questions and notice the lack of context

## Error Handling

The client includes robust error handling for:

- **Rate Limiting**: Graceful handling with retry suggestions
- **Network Issues**: Clear error messages for connectivity problems
- **API Errors**: User-friendly error descriptions

## Architecture

- **MCP Client**: Connects to the Mars Photos server via stdio transport
- **Anthropic SDK**: Integrates with Claude AI for natural language processing
- **Interactive Menu**: Provides easy configuration switching
- **Streaming Responses**: Real-time AI response display

## Troubleshooting

### Common Issues

1. **"Project file not found"**: Ensure the server project is built and the path resolution works
2. **"API key not found"**: Set your Anthropic API key using user secrets or environment variables
3. **"Rate limit exceeded"**: Wait a moment and try again, or reduce your prompt length

### Path Resolution

The client automatically resolves the server project path regardless of whether you run from:

- Command line in the project directory
- Visual Studio debugger
- Compiled executable

## Development

This client demonstrates:

- MCP client implementation in .NET
- Anthropic SDK integration
- Effectively using a system prompt
- Conversation history management
- Interactive console applications
- Error handling and user experience
- Configuration management
