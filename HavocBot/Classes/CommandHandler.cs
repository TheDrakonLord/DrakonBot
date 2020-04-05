/* Title: CommandHandler.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This class profides the framework for handling commands recieved from the user
 * 
 * Dependencies:
 *     botEvents.cs
 *     Form1.cs
 *     globals.cs
 *     HavocBot.cs
 *     InfoModule.cs
 *     Program.cs
 *     
 * Dependent on:
 *     Discord.net
 *      
 * References:
 *     This code is adapted from the instructions and samples provided by the Discord.net Documentation found at:
 *     https://docs.stillu.cc/guides/introduction/intro.html
 */
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using System;

namespace HavocBot
{
    /// <summary>
    /// Provides the framework for handling commands recieved from the user
    /// </summary>
    public class commandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        /// <summary>
        /// Retrieve client and CommandService instance via actor
        /// </summary>
        /// <param name="client">the discord client</param>
        /// <param name="commands">the command service</param>
        public commandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        /// <summary>
        /// Identifies and loads the known commands
        /// </summary>
        /// <returns>returns task complete</returns>
        public async Task installCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += handleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null).ConfigureAwait(false);
        }

        /// <summary>
        /// Recieves a command and interprets it
        /// </summary>
        /// <param name="messageParam">message recieved form the server</param>
        /// <returns>returns task complete</returns>
        private async Task handleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            if (messageParam as SocketUserMessage == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!((messageParam as SocketUserMessage).HasCharPrefix('!', ref argPos) ||
                (messageParam as SocketUserMessage).HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
(messageParam as SocketUserMessage).Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, messageParam as SocketUserMessage);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null).ConfigureAwait(false);

            //log the command recieve in the log
            globals.logMessage("Command",$"{messageParam as SocketUserMessage}");
        }
    }
}
