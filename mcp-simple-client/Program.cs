using Anthropic.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace mcp_simple_client;

class Program
{
    private const string SystemPrompt = @"You are a helpful assistant that can access Mars photos through the MCP tools. Maintain context from the conversation history.

IMPORTANT: If a user asks a question involving a relative date (such as ""today"", ""yesterday"", ""last week"", ""next month"") or a specific date, always use the get-current-date tool to determine the current date before answering or using other tools. This ensures your answers are accurate and up-to-date.";

    static async Task Main(string[] args)
    {
        Console.WriteLine("MCP Simple Client - Mars Photos API");
        Console.WriteLine("===================================");

        // Create the MCP client
        var builder = Host.CreateApplicationBuilder(settings: null);

        builder.Configuration
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();

        // Get the solution root directory by navigating from the application base directory
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectDir = Directory.GetParent(appBaseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
        var serverProjectPath = Path.Combine(projectDir ?? Directory.GetCurrentDirectory(), "mcp-simple-server", "mcp-simple-server.csproj");

        var clientTransport = new StdioClientTransport(new()
        {
            Name = "Demo Server",
            Command = "dotnet",
            Arguments = new[] { "run", "--project", serverProjectPath, "--no-build" },
        });

        await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

        var tools = await mcpClient.ListToolsAsync();
        foreach (var tool in tools)
        {
            Console.WriteLine($"Connected to server with tools: {tool.Name}");
        }

        // Create Anthropic client
        using var anthropicClient = new AnthropicClient(new APIAuthentication(builder.Configuration["ANTHROPIC_API_KEY"]))
            .Messages
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        var options = new ChatOptions
        {
            MaxOutputTokens = 1000,
            ModelId = "claude-3-5-sonnet-20241022",
            Tools = [.. tools]
        };

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("MCP Client Started!");
        Console.ResetColor();

        // Show menu and get user choice
        var (enableHistory, useSystemPrompt, maxHistoryItems) = ShowMenu();

        // Configuration for conversation history
        var conversationHistory = new List<ChatMessage>();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nSelected Configuration:");
        Console.WriteLine($"  History: {(enableHistory ? "✅ Enabled" : "❌ Disabled")}");
        Console.WriteLine($"  System Prompt: {(useSystemPrompt ? "✅ Enabled" : "❌ Disabled")}");
        if (enableHistory)
        {
            Console.WriteLine($"  Max History Items: {maxHistoryItems}");
        }
        Console.WriteLine("\nType 'menu' to change settings, 'exit' to quit.");
        Console.ResetColor();

        PromptForInput();
        while (Console.ReadLine() is string query && !"exit".Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                PromptForInput();
                continue;
            }

            if ("menu".Equals(query, StringComparison.OrdinalIgnoreCase))
            {
                (enableHistory, useSystemPrompt, maxHistoryItems) = ShowMenu();
                conversationHistory.Clear(); // Clear history when changing settings
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nConfiguration Updated:");
                Console.WriteLine($"  History: {(enableHistory ? "✅ Enabled" : "❌ Disabled")}");
                Console.WriteLine($"  System Prompt: {(useSystemPrompt ? "✅ Enabled" : "❌ Disabled")}");
                if (enableHistory)
                {
                    Console.WriteLine($"  Max History Items: {maxHistoryItems}");
                }
                Console.WriteLine("\nType 'menu' to change settings, 'exit' to quit.");
                Console.ResetColor();
                PromptForInput();
                continue;
            }

            try
            {
                if (enableHistory)
                {
                    // Add user message to history
                    conversationHistory.Add(new ChatMessage(ChatRole.User, query));

                    // Prepare messages for the API call
                    var messagesToSend = new List<ChatMessage>();
                    
                    // Add system message if enabled
                    if (useSystemPrompt)
                    {
                        messagesToSend.Add(new ChatMessage(ChatRole.System, SystemPrompt));
                    }

                    // Add recent conversation history (up to maxHistoryItems)
                    var recentHistory = conversationHistory
                        .TakeLast(maxHistoryItems + 1) // +1 to include current message
                        .ToList();
                    
                    messagesToSend.AddRange(recentHistory);

                    // Get streaming response
                    var fullResponse = "";
                    await foreach (var message in anthropicClient.GetStreamingResponseAsync(messagesToSend, options))
                    {
                        Console.Write(message);
                        fullResponse += message;
                    }
                    Console.WriteLine();

                    // Add assistant response to history
                    conversationHistory.Add(new ChatMessage(ChatRole.Assistant, fullResponse));

                    // Keep conversation history manageable
                    if (conversationHistory.Count > maxHistoryItems * 2)
                    {
                        // Remove the oldest messages, keeping the most recent maxHistoryItems * 2
                        conversationHistory = conversationHistory.TakeLast(maxHistoryItems * 2).ToList();
                    }
                }
                else
                {
                    // No history mode
                    var messagesToSend = new List<ChatMessage>();
                    
                    // Add system message if enabled
                    if (useSystemPrompt)
                    {
                        messagesToSend.Add(new ChatMessage(ChatRole.System, SystemPrompt));
                    }
                    
                    messagesToSend.Add(new ChatMessage(ChatRole.User, query));

                    await foreach (var message in anthropicClient.GetStreamingResponseAsync(messagesToSend, options))
                    {
                        Console.Write(message);
                    }
                    Console.WriteLine();
                }
            }
            catch (RateLimitsExceeded ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  Rate limit exceeded: {ex.Message}");
                Console.WriteLine("💡 Try reducing your prompt length or wait a moment before trying again.");
                Console.ResetColor();
                
                // Remove the user message from history if we're in history mode
                if (enableHistory && conversationHistory.Count > 0)
                {
                    conversationHistory.RemoveAt(conversationHistory.Count - 1);
                }
            }
            catch (Exception ex) when (ex.Message.Contains("rate_limit") || ex.Message.Contains("Rate limit"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  Rate limit exceeded: {ex.Message}");
                Console.WriteLine("💡 Try reducing your prompt length or wait a moment before trying again.");
                Console.ResetColor();
                
                // Remove the user message from history if we're in history mode
                if (enableHistory && conversationHistory.Count > 0)
                {
                    conversationHistory.RemoveAt(conversationHistory.Count - 1);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"🌐 Network Error: {ex.Message}");
                Console.WriteLine("💡 Check your internet connection and try again.");
                Console.ResetColor();
                
                // Remove the user message from history if we're in history mode
                if (enableHistory && conversationHistory.Count > 0)
                {
                    conversationHistory.RemoveAt(conversationHistory.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"💥 Unexpected Error: {ex.Message}");
                Console.WriteLine($"Type: {ex.GetType().Name}");
                Console.ResetColor();
                
                // Remove the user message from history if we're in history mode
                if (enableHistory && conversationHistory.Count > 0)
                {
                    conversationHistory.RemoveAt(conversationHistory.Count - 1);
                }
            }

            PromptForInput();
        }

        static (bool enableHistory, bool useSystemPrompt, int maxHistoryItems) ShowMenu()
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("MCP Client Configuration Menu");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("1. Conversation History: Enabled  + System Prompt: Enabled");
            Console.WriteLine("2. Conversation History: Enabled  + System Prompt: Disabled");
            Console.WriteLine("3. Conversation History: Disabled + System Prompt: Enabled");
            Console.WriteLine("4. Conversation History: Disabled + System Prompt: Disabled");
            Console.WriteLine("5. Custom Configuration");
            Console.WriteLine(new string('=', 50));
            
            while (true)
            {
                Console.Write("Select an option (1-5): ");
                var choice = Console.ReadLine()?.Trim();
                
                switch (choice)
                {
                    case "1":
                        return (true, true, 10);
                    case "2":
                        return (true, false, 10);
                    case "3":
                        return (false, true, 10);
                    case "4":
                        return (false, false, 10);
                    case "5":
                        return GetCustomConfiguration();
                    default:
                        Console.WriteLine("Invalid choice. Please enter 1-5.");
                        break;
                }
            }
        }

        static (bool enableHistory, bool useSystemPrompt, int maxHistoryItems) GetCustomConfiguration()
        {
            Console.WriteLine("\nCustom Configuration:");
            
            // Get history preference
            bool enableHistory;
            while (true)
            {
                Console.Write("Enable conversation history? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response == "y" || response == "yes")
                {
                    enableHistory = true;
                    break;
                }
                else if (response == "n" || response == "no")
                {
                    enableHistory = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Please enter 'y' or 'n'.");
                }
            }
            
            // Get system prompt preference
            bool useSystemPrompt;
            while (true)
            {
                Console.Write("Enable system prompt (date tool instructions)? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response == "y" || response == "yes")
                {
                    useSystemPrompt = true;
                    break;
                }
                else if (response == "n" || response == "no")
                {
                    useSystemPrompt = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Please enter 'y' or 'n'.");
                }
            }
            
            // Get max history items if history is enabled
            int maxHistoryItems = 10;
            if (enableHistory)
            {
                while (true)
                {
                    Console.Write("Maximum history items to retain (default: 10): ");
                    var response = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(response))
                    {
                        break;
                    }
                    if (int.TryParse(response, out var limit) && limit > 0)
                    {
                        maxHistoryItems = limit;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Please enter a positive number.");
                    }
                }
            }
            
            return (enableHistory, useSystemPrompt, maxHistoryItems);
        }

        static void PromptForInput()
        {
            Console.WriteLine("Enter a command (or 'menu' to change settings, 'exit' to quit):");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            Console.ResetColor();
        }
    }
}
