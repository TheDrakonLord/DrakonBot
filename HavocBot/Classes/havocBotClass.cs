/* Title: HavocBot.cs
 * Author: Neal Jamieson
 * Version: 0.0.0.0
 * 
 * Description:
 *     This class handles the basic functions of the bot and also provides the method necessary for the bot to announce 
 *     a triggered event.
 * 
 * Dependencies:
 *     botEvents.cs
 *     CommandHandler.cs
 *     globals.cs
 *     Form1.cs
 *     InfoModule.cs
 *     Program.cs
 * 
 * Dependent on:
 *     Discord.net
 *     xml.linq
 *      
 * References:
 *     Code adapted from examples in the following. This document also provided a reference for the use of the discord.net api
 *         https://docs.stillu.cc/guides/introduction/intro.html
 *     xml.linq: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/
 *     xml: https://www.w3schools.com/xml/xml_elements.asp 
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Xml.Linq;
using System.IO;

namespace HavocBot
{
    /// <summary>
    /// Main class for the havoc bot. initializes the command handler and service as well as the client.
    /// also contains methods for the announcing of events.
    /// </summary>
    public sealed class havocBotClass
    {
        //declare necessary variables
        private static DiscordSocketClient _client;
        private commandHandler _cHandler;
        private CommandService _cService;
        private static IMessageChannel _mainChannel;
        private static IMessageChannel _utilityChannel;

        /// <summary>
        /// The main asyncronous method. this must be run asyncronously or it will not function
        /// </summary>
        /// <returns></returns>
        public async Task mainAsync()
        {
            //initialize the client, command handler, and command service
            _client = new DiscordSocketClient();
            _cService = new CommandService();
            _cHandler = new commandHandler(_client, _cService);




            _client.Log += log;


            try
            {
                globals.token = System.IO.File.ReadAllText("token.txt");
            }
            catch (FileNotFoundException)
            {
                globals.logMessage(Properties.strings.errorTokenMissing);
                throw;
            }

            
            // have the client login and start
            await _client.LoginAsync(TokenType.Bot, globals.token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);
            await _client.SetGameAsync(globals.statusMessage, null, ActivityType.Playing).ConfigureAwait(false);
            _client.JoinedGuild += joinedGuild;
            _client.LeftGuild += leftGuild;
            _client.Ready += botReady;
            

            // load the commands
            await _cHandler.installCommandsAsync().ConfigureAwait(false);
            // Block this task until the program is closed.
            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// When a message is recieved this sends the appropriate message to the log
        /// </summary>
        /// <param name="msg">Message to be sent to the log</param>
        /// <returns>completed task state</returns>
        private Task log(LogMessage msg)
        {
            // queue the message to be sent to the log
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// When an event is triggered, this loads the event from the data file and sends the announcement to the target channel.
        /// </summary>
        /// <param name="name">the name of the event to be triggered. case sensitive.</param>
        public static void eventTriggered(string name)
        {
            //identify the target channel (this is set in globals)
            

            if (globals.getEvent(name, out botEvents retrievedEvent))
            {
                _mainChannel = _client.GetChannel(globals.guildSettings[$"g{retrievedEvent.guild}"][1]) as IMessageChannel;
                //build the embed to be sent to the target channel
                EmbedBuilder eventEmbed;
                string mentions = retrievedEvent.mentions;
                string caption = $"The event \"{name}\" has begun";

#pragma warning disable CA1307 // Specify StringComparison
                if (!mentions.Equals("none") && !mentions.Equals("rsvp"))
#pragma warning restore CA1307 // Specify StringComparison
                {
                    caption = $"{mentions} {caption}";
                }
#pragma warning disable CA1307 // Specify StringComparison
                if (mentions.Equals("rsvp"))
#pragma warning restore CA1307 // Specify StringComparison
                {
                    mentions = retrievedEvent.allRSVPIDs();
                    caption = $"{caption} {mentions}";
                }

                eventEmbed = globals.generateEventEmbed(retrievedEvent);

                //send the announcement to the target channel
                _mainChannel.SendMessageAsync(caption, false, eventEmbed.Build());

                //log that an event has been triggered in the log
                globals.logMessage("Event Triggered",retrievedEvent.name);

                retrievedEvent.repeatDate();

                IEnumerable<XElement> eventRetrieve =
                       from el in globals.commandStorage.Elements("events")
                       select el;

                eventRetrieve = from el in eventRetrieve.Elements("event")
                                where (string)el.Element("name") == retrievedEvent.name
                                select el;

                XElement xdate = (from el in eventRetrieve.Descendants("start")
                                  select el).Last();

                xdate.Value = retrievedEvent.startDate.ToString();

                xdate = (from el in eventRetrieve.Descendants("end")
                         select el).Last();

                xdate.Value = retrievedEvent.endDate.ToString();

                globals.commandStorage.Save(globals.storageFilePath);
#pragma warning disable CA1307 // Specify StringComparison
                if (!retrievedEvent.repeat.Equals("none"))
#pragma warning restore CA1307 // Specify StringComparison
                {
                    globals.eventCalendar[name] = retrievedEvent.startDate;
                    globals.logMessage(Properties.strings.msgRepeat);
                }
                else
                {
                    // remove the event from the list of active events
                    globals.eventCalendar.Remove(name);
                }
            }
            else
            {
                //if the event is missing send an error to the target channel and log the error
                globals.logMessage(Properties.strings.exceptionEventTrigger);
                _mainChannel.SendMessageAsync(Properties.strings.errorEventTrigger);
            }
        }

        /// <summary>
        /// Displays message that the bot is undergoing maintenance
        /// </summary>
        public static void showDownTime()
        {
            foreach (var x in globals.guildSettings)
            {
                _utilityChannel = _client.GetChannel(x.Value[0]) as IMessageChannel;
                EmbedBuilder downEmbed = new EmbedBuilder();

                downEmbed.WithTitle("Bot Undergoing Maintenance");
                downEmbed.AddField("Details", "The bot is now undergoing maintenance, during this time the bot may go offline several times and may not respond to commands. Downtime is not expected to take longer than an hour.");

                _utilityChannel.SendMessageAsync("", false, downEmbed.Build());
            }
        }

        /// <summary>
        /// Displays message that the bot is undergoing maintenance
        /// </summary>
        public static void showAllPatchNotes()
        {
            foreach (var x in globals.guildSettings)
            {
                _utilityChannel = _client.GetChannel(x.Value[0]) as IMessageChannel;
                var patchEmbed = new EmbedBuilder();
                patchEmbed.WithTitle(globals.versionID);

                patchEmbed.AddField(" * *Changes * *", globals.patchnotes);

                patchEmbed.WithFooter(globals.patchID);
                patchEmbed.WithTimestamp(globals.patchDate);
                patchEmbed.WithColor(Color.Gold);

                _utilityChannel.SendMessageAsync(Properties.strings.msgPatchNotes, false, patchEmbed.Build()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Event handler for when the bot joins a guild
        /// </summary>
        /// <param name="newGuild">the guid the bot just joined</param>
        /// <returns>task complete</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task joinedGuild(SocketGuild newGuild)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!globals.allGuilds.Contains(newGuild.Id))
            {
                globals.allGuilds.Add(newGuild.Id);
                globals.logMessage("Connected to Guild",$"{newGuild.Name} ({newGuild.Id})");

                //globals.commandStorage.Element("guilds").Add(new XElement("guild", newGuild.Id.ToString()));
                //globals.commandStorage.Save(globals.storageFilePath);
            }
            else
            {
                globals.logMessage($"Join error. Already connected to {newGuild.Name} ({newGuild.Id})");
            }
        }

        /// <summary>
        /// Event handler for when the bot leaves a guild
        /// </summary>
        /// <param name="leavingGuild">the guild the bot left</param>
        /// <returns>task complete</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task leftGuild(SocketGuild leavingGuild)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (globals.allGuilds.Contains(leavingGuild.Id))
            {
                globals.allGuilds.Remove(leavingGuild.Id);
                globals.logMessage($"The bot has left the guild {leavingGuild.Name} ({leavingGuild.Id})");
            }
            else
            {
                globals.logMessage($"Bot departure error. {leavingGuild.Name} ({leavingGuild.Id}) not found in collection");
            }
        }

        /// <summary>
        /// Changes the bot's status message
        /// </summary>
        /// <param name="message">the new status message to be set</param>
        /// <returns></returns>
        public static void statusChange(string message)
        {
            _client.SetGameAsync(message, null, ActivityType.Playing).ConfigureAwait(false);
        }

        /// <summary>
        /// Event handler for when the bot has finished loading and is ready to run
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task botReady()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            globals.logMessage(Properties.strings.logBotReady);
            IReadOnlyCollection<SocketGuild> conGuilds = _client.Guilds;
            foreach (SocketGuild x in conGuilds)
            {
                if (!globals.allGuilds.Contains(x.Id))
                {
                    globals.allGuilds.Add(x.Id);
                    globals.playbackPIDs.Add(x.Id, 0);
                    globals.playbackQueues.Add(x.Id, new Queue<string>());
                    globals.logMessage("Connected to Guild",$"{x.Name} ({x.Id})");
                }
                else
                {
                    globals.logMessage($"Ready Join error. Already connected to {x.Name} ({x.Id})");
                }
            }
        }
    }
}
